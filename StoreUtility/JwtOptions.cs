using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreUtility
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;
    }
}
