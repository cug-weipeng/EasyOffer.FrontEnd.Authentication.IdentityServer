using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer.Models
{
    public class TokenDto
    {
        public long TokenId { get; set; }

        public long UserId { get; set; }

        public string SecretKey { get; set; }

        public DateTime? ExpiryTime { get; set; }
    }
}
