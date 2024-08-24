using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Security;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StoreAPIs.Service.IService;
using StoreDataAccess.Repository.IRepository;
using StoreModels;
using StoreModels.Dtos;
using StoreUtility;

namespace StoreAPIs.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        
        private readonly IAuthService authService;
        private readonly ILogger<AuthController> logger;

        public AuthController(
            ILogger<AuthController> logger,
            IApplicationUserRepository applicationUserRepository,
            IAuthService authService
        )
        {
            this.logger = logger;
            this.authService = authService;
        }

        [HttpPost("AdminRegister")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> Register([FromBody] AdminUserRegisterDto user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (string.IsNullOrEmpty(authService.Register(user).Result))
                    {
                        return Ok("User Was Created Successfully");
                    }
                    else
                        return BadRequest("this email is already registered");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString(), ex.Message);
                    return StatusCode(500, "Internal server error.");
                }
            }
            return BadRequest();
        }

        [HttpPost("Register")]
        public  async Task<IActionResult> Register([FromBody] UserRegisterDto user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (
                        string.IsNullOrEmpty(
                            await authService
                                .Register(
                                    new AdminUserRegisterDto
                                    {
                                        Name = user.Name,
                                        Password = user.Password,
                                        Email = user.Email,
                                        StreetAddress = user.StreetAddress,
                                        City = user.City,
                                        PhoneNumber = user.PhoneNumber,
                                        State = user.State,
                                        PostalCode = user.PostalCode,
                                        CompanyId = user.CompanyId,
                                        ConfirmPassword = user.ConfirmPassword,
                                    }
                                )
                                
                        )
                    )
                    {
                        return Ok("User Was Created Successfully");
                    }
                    else
                        return BadRequest("this email is already registered");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString(), ex.Message);
                    return StatusCode(500, "Internal server error.");
                }
            }
            return BadRequest();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto user)
        {
            var loginResponse = await authService.Login(user);
            if (loginResponse.User is null)
            {
                return BadRequest("Username or password is incorrect");
            }
            return Ok(loginResponse);
        }

		[HttpPost("logout")]
		public async Task<IActionResult> Logout([FromBody] UserLoginDto user)
		{
            await authService.Logout();
			return Ok(true);
		}

		[HttpPost("AssignRole")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<IActionResult> AssignRole([FromBody] AdminUserRegisterDto model)
        {
            var assignRoleSuccessful = await authService.AssignRole(
                model.Email,
                model.Role.ToUpper()
            );
            if (!assignRoleSuccessful)
            {
                return BadRequest("Error encountered");
            }
            return Ok();
        }
    }
}
