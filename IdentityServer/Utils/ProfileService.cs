using EasyOffer.FrontEnd.Authentication.IdentityServer.Entities;
using EasyOffer.FrontEnd.Authentication.IdentityServer.Repositories;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer
{
    public class ProfileService : IProfileService
    {
        private UserRepo _userRepo = new UserRepo();

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            try
            {
                long userId = 0;
                var sub = context.Subject.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
                if (!string.IsNullOrEmpty(sub))
                {
                    if (long.TryParse(sub, out userId))
                    {
                        var user = _userRepo.GetUserByUserId(userId);

                        if (user != null)
                        {
                            var claims = GetUserClaims(user);

                            //set issued claims to return  
                            context.IssuedClaims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type)).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //log your error  
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            try
            {
                //get subject from context (set in ResourceOwnerPasswordValidator.ValidateAsync),  

                UserEntity user = null;
                var userIdStr = context.Subject.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
                if (long.TryParse(userIdStr,out long userid))
                {
                    user = _userRepo.GetUserByUserId(userid);
                }

                if (user != null)
                {
                    if (!user.Disabled)
                    {
                        context.IsActive = true;
                    }
                }

            }
            catch (Exception ex)
            {
                //handle error logging  
            }
        }

        public static Claim[] GetUserClaims(UserEntity user)
        {
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
