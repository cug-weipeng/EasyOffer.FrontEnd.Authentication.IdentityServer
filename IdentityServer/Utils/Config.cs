using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer
{
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password",

                    Claims = new []
                    {
                        new Claim("UserId",      "123"),
                        new Claim("FullName",    "Alice Wei"),
                        new Claim("FirstName",   "Alice"),
                        new Claim("LastName",    "Wei"),
                        new Claim("Gender",      "male"),
                        new Claim("BirthDate",   "1216"),
                        new Claim("Email",       "wei@wei.com"),
                        new Claim("PhoneNumber", "1301111111"),
                        new Claim("Avatar",      "asdasdas"),
                        new Claim("RedeemCode",  "asdas"),
                        new Claim("Integrals",   "asdasd"),
                        new Claim("Level",       "asdasfdas"),
                        new Claim("Verified",    "asfdsafas")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password",

                    Claims = new []
                    {
                        new Claim("name", "Bob"),
                        new Claim("website", "https://bob.com")
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "My API",new List<string>
                {
                    "user_id",
                    "full_name",
                    "first_name",
                    "last_name",
                    "gender",
                    "birth_date",
                    "email",
                    "phone_number",
                    "avatar",
                    "redeem_code",
                    "integrals",
                    "level",
                    "verified"
                })
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = { "api1" }
                },
                // resource owner password grant client
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" }
                },
                // OpenID Connect implicit flow client (MVC)
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.Implicit,
                
                    // where to redirect to after login
                    RedirectUris = { "http://localhost:5002/signin-oidc" },

                    // where to redirect to after logout
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    }
                },
                new Client()
               {//微软客户端
                    ClientId="external.microsoft",
                    ClientName="Microsoft external login client",
                    //客户端密码
                     ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes=GrantTypes.Code,
                    EnableLocalLogin = false,
                    RequireConsent= false,
                    IdentityProviderRestrictions = new List<string>(){ "Microsoft"},
                    //允许登录后重定向的地址列表，可以有多个
                    //允许登录后重定向的地址列表，可以有多个
                    RedirectUris = {"https://silumart.com"},
                    AllowedScopes = { "api1" }
               },
                new Client()
               {//微软客户端
                    ClientId="external.weibo",
                    ClientName="weibo external login client",
                    //客户端密码
                     ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes=GrantTypes.Code,
                    EnableLocalLogin = false,
                    RequireConsent= false,
                    IdentityProviderRestrictions = new List<string>(){ "Weibo"},
                    //允许登录后重定向的地址列表，可以有多个
                    //允许登录后重定向的地址列表，可以有多个
                    RedirectUris = {"https://silumart.com"},
                    AllowedScopes = { "api1" }
               },
                new Client()
               {//微软客户端
                    ClientId="external.facebook",
                    ClientName="facebook external login client",
                    //客户端密码
                     ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes=GrantTypes.Code,
                    EnableLocalLogin = false,
                    RequireConsent= false,
                    IdentityProviderRestrictions = new List<string>(){ "Facebook"},
                    //允许登录后重定向的地址列表，可以有多个
                    //允许登录后重定向的地址列表，可以有多个
                    RedirectUris = {"https://silumart.com"},
                    AllowedScopes = { "api1" }
               },
                new Client()
               {//微软客户端
                    ClientId="external.google",
                    ClientName="Google external login client",
                    //客户端密码
                     ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedGrantTypes=GrantTypes.Code,
                    EnableLocalLogin = false,
                    RequireConsent= false,
                    IdentityProviderRestrictions = new List<string>(){ "Google"},
                    //允许登录后重定向的地址列表，可以有多个
                    //允许登录后重定向的地址列表，可以有多个
                    RedirectUris = {"https://silumart.com"},
                    AllowedScopes = { "api1" }
               }
            };
        }
    }
}
