using EasyOffer.FrontEnd.Authentication.IdentityServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer.Repositories
{
    public interface IUserRepo
    {
        UserEntity GetUserByUserId(long userId);

        UserEntity GetUserByProvider(string provider, string uid);

        UserEntity GetUserByEmail(string email);

        void Insert(UserEntity entity);

        void UpdateUserById(UserEntity entity);
    }
}
