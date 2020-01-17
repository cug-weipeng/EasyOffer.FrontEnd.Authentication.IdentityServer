using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer
{
    public class AuthorizeRequestValidator : ICustomAuthorizeRequestValidator
    {
        public Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
        {
            throw new NotImplementedException();
        }
    }
    public class TokenRequestValidator : ICustomTokenRequestValidator
    {

        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            throw new NotImplementedException();
        }
    }
}
