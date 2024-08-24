using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly IApplicationUserRepository applicationUserRepository;
        private readonly IAuthService authService;

        public UserController(ILogger<UserController> logger, IApplicationUserRepository applicationUserRepository,IAuthService authService)
        {
            this.logger = logger;
            this.applicationUserRepository = applicationUserRepository;
            this.authService = authService;
        }
        [HttpGet("Users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ApplicationUser>>> GetAllUsers()
        {
            try
            {
                var categories = await applicationUserRepository.GetAll();
                if (categories is null)
                    return NotFound();
                return Ok(categories);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("id/{id:int}", Name = "GeUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApplicationUser>> GetUserById(string? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            try
            {
                var User = await applicationUserRepository.Get(x => x.Id == id);
                if (User is null)
                    return NotFound();
                return Ok(User);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<ApplicationUser>> GetUserByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            try
            {
                var User = await applicationUserRepository.Get(x => string.Equals(x.Name, name));
                if (User is null)
                    return NotFound();
                return Ok(User);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post([FromBody] AdminUserRegisterDto User)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                       var result =  await authService.Register(User);
                        if(result =="")
                            return Ok();
                    }
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex.ToString(), ex.Message);
                    return StatusCode(500, "Internal server error.");
                }
            }
            return BadRequest();
        }

        [Authorize(Roles = SD.Role_Admin)]
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] ApplicationUser User)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    applicationUserRepository.Update(User);
                    await applicationUserRepository.Save();

                    return Ok();
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex.ToString(), ex.Message);
                    return StatusCode(500, "Internal server error.");
                }
            }
            return BadRequest();
        }

        [HttpDelete]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            try
            {
                var User = await applicationUserRepository.Get(x => x.Id == id);
                if (User is null)
                    return NotFound();
                applicationUserRepository.Remove(User);
                await applicationUserRepository.Save();
                return Ok();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
