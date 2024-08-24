using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Repository.IRepository;
using StoreModels;
using StoreModels.Dtos;
using StoreUtility;

namespace StoreAPIs.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly ILogger<CategoryController> logger;

        public CategoryController(
            ICategoryRepository categoryRepository,
            ILogger<CategoryController> logger
        )
        {
            this.categoryRepository = categoryRepository;
            this.logger = logger;
        }

        [HttpGet("categories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Category>>> GetAllCategories()
        {
            try
            {
                var categories = await categoryRepository.GetAll();
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

        [HttpGet("id/{id:int}", Name = "GetCategoryById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Category>> GetCategoryById(int? id)
        {
            if (id == null || id < 1)
            {
                return BadRequest();
            }
            try
            {
                var category = await categoryRepository.Get(x => x.Id == id);
                if (category is null)
                    return NotFound();
                return Ok(category);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Category>> GetCategoryByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            try
            {
                var category = await categoryRepository.Get(x => string.Equals(x.Name, name));
                if (category is null)
                    return NotFound();
                return Ok(category);
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
        public async Task<ActionResult> Post([FromBody] CategoryDto categoryDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Category category = new Category
                    {
                        Name = categoryDto.Name,
                        Description = categoryDto.Description,
                        DisplayOrder = categoryDto.DisplayOrder,
                    };
                    await categoryRepository.Add(category);
                    await categoryRepository.Save();
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

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    categoryRepository.Update(category);
                    await categoryRepository.Save();

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

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return BadRequest();
            }
            try
            {
                var category = await categoryRepository.Get(x => x.Id == id);
                if (category is null)
                    return NotFound();
                categoryRepository.Remove(category);
                await categoryRepository.Save();
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
