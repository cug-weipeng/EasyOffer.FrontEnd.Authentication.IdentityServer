using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading.Tasks;
using EasyOffer.FrontEnd.Authentication.IdentityServer.Entities;
using EasyOffer.FrontEnd.Authentication.IdentityServer.Models;
using EasyOffer.FrontEnd.Authentication.IdentityServer.Repositories;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer.Controllers
{
    [AllowAnonymous]
    public class ExternalController : Controller
    {

        private readonly TestUserStore _users;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IConfiguration _configuration;
        private readonly IAuthorizeResponseGenerator _authorizeResponseGenerator;
        private readonly ITokenService _tokenService;

        private UserRepo _userRepo;

        public ExternalController(
          IIdentityServerInteractionService interaction,
          IClientStore clientStore,
          IAuthenticationSchemeProvider schemeProvider,
          IEventService events,
          IConfiguration configuration,
          IAuthorizeResponseGenerator authorizeResponseGenerator,
          ITokenService tokenService,
          TestUserStore users = null)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            _users = users ?? new TestUserStore(TestUsers.Users);

            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _configuration = configuration;
            _authorizeResponseGenerator = authorizeResponseGenerator;
            _tokenService = tokenService;

            _userRepo = new UserRepo();
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        public async Task<IActionResult> Challenge(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            {
                // windows authentication needs special handling
                return await ProcessWindowsLoginAsync(returnUrl);
            }
            else
            {
                // start challenge and roundtrip the return URL and scheme 
                var props = new AuthenticationProperties
                {
                    RedirectUri = Url.Action(nameof(Callback)),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", provider },
                    }
                };

                return Challenge(props, provider);
            }
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // lookup our user and external provider info
            var (user, provider, providerUserId, claims) = FindUserFromExternalProvider(result);
            if (user == null)
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = AutoProvisionUser(provider, providerUserId, claims);
            }

            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = GetUserClaims(user).ToList();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForWsFed(result, additionalLocalClaims, localSignInProps);
            ProcessLoginCallbackForSaml2p(result, additionalLocalClaims, localSignInProps);


            // issue authentication cookie for user
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.UserId.ToString(), user.FullName));
            await HttpContext.SignInAsync(user.UserId.ToString(), user.FirstName, provider, localSignInProps, additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context != null)
            {
                if (await _clientStore.IsPkceClientAsync(context.ClientId))
                {
                    // if the client is PKCE then we assume it's native, so this change in how to
                    // return the response is for better UX for the end user.
                    // return View("Redirect", new RedirectViewModel { RedirectUrl = returnUrl });
                    throw new NotImplementedException();
                }
            }
            return Redirect(returnUrl);
        }

        private async Task<IActionResult> ProcessWindowsLoginAsync(string returnUrl)
        {
            // see if windows auth has already been requested and succeeded
            var result = await HttpContext.AuthenticateAsync(AccountOptions.WindowsAuthenticationSchemeName);
            if (result?.Principal is WindowsPrincipal wp)
            {
                // we will issue the external cookie and then redirect the
                // user back to the external callback, in essence, treating windows
                // auth the same as any other external authentication mechanism
                var props = new AuthenticationProperties()
                {
                    RedirectUri = Url.Action("Callback"),
                    Items =
                    {
                        { "returnUrl", returnUrl },
                        { "scheme", AccountOptions.WindowsAuthenticationSchemeName },
                    }
                };

                var id = new ClaimsIdentity(AccountOptions.WindowsAuthenticationSchemeName);
                id.AddClaim(new Claim(JwtClaimTypes.Subject, wp.Identity.Name));
                id.AddClaim(new Claim(JwtClaimTypes.Name, wp.Identity.Name));

                // add the groups as claims -- be careful if the number of groups is too large
                if (AccountOptions.IncludeWindowsGroups)
                {
                    var wi = wp.Identity as WindowsIdentity;
                    var groups = wi.Groups.Translate(typeof(NTAccount));
                    var roles = groups.Select(x => new Claim(JwtClaimTypes.Role, x.Value));
                    id.AddClaims(roles);
                }

                await HttpContext.SignInAsync(
                    IdentityServer4.IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    new ClaimsPrincipal(id),
                    props);
                return Redirect(props.RedirectUri);
            }
            else
            {
                // trigger windows auth
                // since windows auth don't support the redirect uri,
                // this URL is re-triggered when we call challenge
                return Challenge(AccountOptions.WindowsAuthenticationSchemeName);
            }
        }

        private (UserEntity user, string provider, string providerUserId, IEnumerable<Claim> claims) FindUserFromExternalProvider(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");


            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            var user = _userRepo.GetUserByProvider(provider, userIdClaim.Value);

            return (user, provider, providerUserId, claims);
        }

        private UserEntity AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims)
        {
            var userId = new IdGenerationSvc(_configuration).GenerateId();
            var user = new UserEntity
            {
                UserId = userId,
                Email = GetClaimValue(claims, JwtClaimTypes.Email) ?? GetClaimValue(claims, ClaimTypes.Email),
                FullName = GetClaimValue(claims, JwtClaimTypes.Name) ?? GetClaimValue(claims, ClaimTypes.Name),
                FirstName = GetClaimValue(claims, JwtClaimTypes.GivenName) ?? GetClaimValue(claims, ClaimTypes.GivenName),
                LastName = GetClaimValue(claims, JwtClaimTypes.FamilyName) ?? GetClaimValue(claims, ClaimTypes.Surname),
                Password = GetClaimValue(claims, JwtClaimTypes.Email) ?? GetClaimValue(claims, ClaimTypes.Email),
                Avatar = GetClaimValue(claims, JwtClaimTypes.Picture) ?? GetClaimValue(claims, ClaimTypes.Webpage) ?? "none", // TODO: 用户头像
                IPAddress = GetUserIP(),
                Integrals = 0,
                Verified = false,
                Level = 1,
                Status = "Pending",
                Disabled = true,//TODO: 外部账户设为禁用  需要设置为其他值
                ExternalProvider = provider,
                ExternalSubjectId = providerUserId,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now
            };

            _userRepo.Insert(user);

            return user;
        }
        private string GetClaimValue(IEnumerable<Claim> claims, string type)
        {
            var claim = claims.FirstOrDefault(t => t.Type == type);
            if (claim == null)
                return default;

            return claim.Value;
        }

        private string GetUserIP()
        {

            var userHostAddress = HttpContext?.Request?.Headers["X-EZ-IPAddress"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userHostAddress))
            {
                userHostAddress = HttpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(userHostAddress))
            {
                userHostAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();
            }

            return userHostAddress;

        }

        private void ProcessLoginCallbackForOidc(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var id_token = externalResult.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = id_token } });
            }

        }

        private void ProcessLoginCallbackForWsFed(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        private void ProcessLoginCallbackForSaml2p(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
        }

        public static Claim[] GetUserClaims(UserEntity user)
        {
            if(user == null)
            {
                return new Claim[] { };
            }

            return new Claim[]
            {
                new Claim("user_id",user.UserId.ToString()),
                new Claim("full_name",user.FullName),
                new Claim("first_name",user.FirstName),
                new Claim("last_name",user.LastName),
                new Claim("avatar",user.Avatar),
                new Claim("email",user.Email)
            };
        }
    }
}
