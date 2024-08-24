using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoreModels.Dtos
{
    public class LoginResponseDto
    {
        public UserDto User { get; set; }
        public string Token { get; set; }
    }
}
