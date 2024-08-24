using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StoreDataAccess.Repository;
using StoreModels.Dtos;
using StoreModels;
using StoreDataAccess.Repository.IRepository;

namespace StoreAPIs.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CompanyController : ControllerBase
    {
        private readonly ILogger<CompanyController> logger;
        private readonly ICompanyRepository companyRepository;

        public CompanyController(ILogger<CompanyController>logger,ICompanyRepository companyRepository)
        {
            this.logger = logger;
            this.companyRepository = companyRepository;
        }
        [HttpGet("companies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Company>>> GetAllCompanies()
        {
            try
            {
                var categories = await companyRepository.GetAll();
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

        [HttpGet("id/{id:int}", Name = "GeCompanyById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Company>> GetCompanyById(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            try
            {
                var Company = await companyRepository.Get(x => x.Id == id);
                if (Company is null)
                    return NotFound();
                return Ok(Company);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Company>> GetCompanyByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            try
            {
                var Company = await companyRepository.Get(x => string.Equals(x.Name, name));
                if (Company is null)
                    return NotFound();
                return Ok(Company);
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
        public async Task<ActionResult> Post([FromBody] Company Company)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(ModelState.IsValid)
                    {
                        await companyRepository.Add(Company);
                        await companyRepository.Save();
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

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Company Company)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    companyRepository.Update(Company);
                    await companyRepository.Save();

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
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return BadRequest();
            }
            try
            {
                var Company = await companyRepository.Get(x => x.Id == id);
                if (Company is null)
                    return NotFound();
                companyRepository.Remove(Company);
                await companyRepository.Save();
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

