using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StoreModels.Dtos;

namespace StoreAPIs.Service.IService
{
    public interface IAuthService
    {
        Task<bool> Logout();
 		Task<string> Register(AdminUserRegisterDto userRegisterDto);
        Task<LoginResponseDto> Login(UserLoginDto userLoginDto);
        Task<bool> AssignRole(string email, string roleName);
    }
}
