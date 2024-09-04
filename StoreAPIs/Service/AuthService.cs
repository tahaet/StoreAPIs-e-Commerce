using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using StoreAPIs.Service.IService;
using StoreDataAccess;
using StoreModels;
using StoreModels.Dtos;
using StoreUtility;

namespace StoreAPIs.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly AppDbContext db;
        private readonly IJwtTokenGenerator jwtTokenGenerator;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db,
            IJwtTokenGenerator jwtTokenGenerator,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.db = db;
            this.jwtTokenGenerator = jwtTokenGenerator;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = db.ApplicationUsers.FirstOrDefault(u =>
                u.Email.ToLower() == email.ToLower()
            );
            if (user != null)
            {
                if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                {
                    //create role if it does not exist
                    roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }
                await userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(UserLoginDto userLoginDto)
        {
            var user = db.ApplicationUsers.FirstOrDefault(x =>
                x.Email.ToLower() == userLoginDto.Email.ToLower()
            );
            bool isValid = await userManager.CheckPasswordAsync(user, userLoginDto.Password);
            if (user is null || !isValid)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }
            var roles = await userManager.GetRolesAsync(user);
            var token = jwtTokenGenerator.GenerateToken(user, roles);
            UserDto userDto = new UserDto()
            {
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                CompanyId = user.CompanyId,
                Role = roles.FirstOrDefault(),
            };
            LoginResponseDto loginResponseDto = new LoginResponseDto()
            {
                User = userDto,
                Token = token
            };
            return loginResponseDto;
        }

        public async Task<string> Register(AdminUserRegisterDto userRegisterDto)
        {
            var ApplicationUser = new ApplicationUser();

            ApplicationUser.UserName = userRegisterDto.Email;
            ApplicationUser.Email = userRegisterDto.Email;
            ApplicationUser.Name = userRegisterDto.Name;
            ApplicationUser.StreetAddress = userRegisterDto.StreetAddress;
            ApplicationUser.City = userRegisterDto.City;
            ApplicationUser.PostalCode = userRegisterDto.PostalCode;
            ApplicationUser.State = userRegisterDto.State;
            ApplicationUser.PhoneNumber = userRegisterDto.PhoneNumber;
            if (userRegisterDto.Role == SD.Role_Company)
            {
                ApplicationUser.CompanyId = userRegisterDto.CompanyId;
            }

            try
            {
                var result = await userManager.CreateAsync(
                    ApplicationUser,
                    userRegisterDto.Password
                );
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(userRegisterDto.Role))
                    {
                        if (
                            roleManager
                                .GetRoleNameAsync(new IdentityRole(userRegisterDto.Role))
                                .Result != null
                        )
                        {
                            await userManager.AddToRoleAsync(ApplicationUser, userRegisterDto.Role);
                        }
                    }
                    else
                        await userManager.AddToRoleAsync(ApplicationUser, SD.Role_Customer);

                    var userToReturn = db.ApplicationUsers.FirstOrDefault(x =>
                        x.Email == userRegisterDto.Email
                    );
                    UserDto userDto = new UserDto()
                    {
                        Id = userToReturn.Id,
                        Name = userToReturn.Name,
                        Email = userToReturn.Email,
                        PhoneNumber = userToReturn.PhoneNumber,
                        CompanyId = userToReturn.CompanyId,
                        Role = userToReturn.Role
                    };
                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch (Exception ex) { }
            return "Error encountered";
        }
    }
}
