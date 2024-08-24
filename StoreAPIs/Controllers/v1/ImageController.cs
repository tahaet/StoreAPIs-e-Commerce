using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Repository;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreAPIs.Controllers.v1
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ImageController : ControllerBase
    {
        private readonly IProductImageRepository productImageRepository;
        private readonly ILogger<ImageController> logger;

        public ImageController(IProductImageRepository productImageRepository,ILogger<ImageController> logger)
        {
            this.productImageRepository = productImageRepository;
            this.logger = logger;
        }

        [HttpGet("images")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductImage>>> GetAllImages()
        {
            try
            {
                var images = await productImageRepository.GetAll();
                if (images is null)
                    return NotFound();
                return Ok(images);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
