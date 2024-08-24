using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreDataAccess.Repository;
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
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly ILogger<ProductController> logger;

        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IShoppingCartRepository shoppingCartRepository;

        public ProductController(
            IProductRepository ProductRepository,
            IProductImageRepository productImageRepository,
            ILogger<ProductController> logger,
            IWebHostEnvironment webHostEnvironment,
            IShoppingCartRepository shoppingCartRepository
        )
        {
            this.productRepository = ProductRepository;
            this.productImageRepository = productImageRepository;
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
            this.shoppingCartRepository = shoppingCartRepository;
        }

        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetAllProducts()
        {
            try
            {
                var products = await productRepository.GetAll(
                    includeProperties: "Category,ProductImages"
                );
                if (products is null)
                    return NotFound();
                return Ok(products);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("id/{id:int}")]
        public async Task<ActionResult<Product>> GetProductById(int? id)
        {
            if (id == null || id == 0)
            {
                return BadRequest();
            }
            try
            {
                var Product = await productRepository.Get(
                    x => x.Id == id,
                    includeProperties: "Category,ProductImages"
                );
                if (Product is null)
                    return NotFound();
                return Ok(Product);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("name/{name}")]
        public async Task<ActionResult<Product>> GetProductByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            try
            {
                var Product = await productRepository.Get(
                    x => string.Equals(x.Name, name),
                    includeProperties: "Category,ProductImages"
                );
                if (Product is null)
                    return NotFound();
                return Ok(Product);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("details/{id:int}")]
        public async Task<ActionResult<ShoppingCart>> Details(int id)
        {
            try
            {
                var product = await productRepository.Get(
                    p => p.Id == id,
                    includeProperties: "ProductImages,Category"
                );

                if (product == null)
                {
                    return NotFound();
                }

                ShoppingCart shoppingCart = new ShoppingCart
                {
                    Product = product,
                    Count = 1,
                    ProductId = id
                };

                return Ok(shoppingCart);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("Details")]
        [Authorize]
        public async Task<IActionResult> Details(ShoppingCart shoppingCart)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                shoppingCart.ApplicationUserId = userId;
                var cartFromDb = await shoppingCartRepository.Get(s =>
                    s.ApplicationUserId == userId && s.ProductId == shoppingCart.ProductId
                );
                if (cartFromDb == null)
                {
                    shoppingCart.Id = 0;
                    await shoppingCartRepository.Add(shoppingCart);
                    await shoppingCartRepository.Save();

                    HttpContext.Session.SetInt32(
                        SD.SessionCart,
                        shoppingCartRepository
                            .GetAll(s => s.ApplicationUserId == userId)
                            .Result.Count()
                    );
                }
                else
                {
                    cartFromDb.Count += shoppingCart.Count;
                    shoppingCartRepository.Update(cartFromDb);
                    await shoppingCartRepository.Save();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("Home")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetAllProductsWithMainImage()
        {
            try
            {
                var products = await productRepository.GetAll(
                    includeProperties: "Category,ProductImages"
                );
                if (products is null)
                    return NotFound();
                return Ok(products);
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    QuantityInStock = productDto.QuantityInStock,
                    ListPrice = productDto.ListPrice,
                    Price = productDto.Price,
                    Price50 = productDto.Price50,
                    Price100 = productDto.Price100,
                    CategoryId = productDto.CategoryId,
                    ProductImages = new List<ProductImage>()
                };

                try
                {
                    await productRepository.Add(product);
                    await productRepository.Save();

                    string wwwRootPath = webHostEnvironment.WebRootPath is null
                        ? Directory
                            .CreateDirectory($"{Directory.GetCurrentDirectory()}\\wwwroot")
                            .FullName
                        : webHostEnvironment.WebRootPath;

                    if (productDto.ProductImages != null)
                    {
                        for (int i = 0; i<productDto.ProductImages.Count;i++ )
                        {
							//IFormFile file in productDto.ProductImages

							//I have added the if condition to remove the any image with same name if that exist in the folder by any change
							string fileName = product.Id + Path.GetExtension(productDto.ProductImages[i].FileName);
							string filePath = @"wwwroot\ProductImages\" + fileName;
							var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);
							var folderPath = $@"images\products\product-{product.Id}";
                            var finalPath = Path.Combine(wwwRootPath, folderPath);

							FileInfo fileInfo = new FileInfo(directoryLocation);
							if (fileInfo.Exists)
							{
								fileInfo.Delete();
							}
                            if (!Directory.Exists(finalPath))
                            {
                                Directory.CreateDirectory(finalPath);
                            }

                            using (
                                var fileStreams = new FileStream(
                                    Path.Combine(finalPath, fileName),
                                    FileMode.Create
                                )
                            )
                            {
                                productDto.ProductImages[i].CopyTo(fileStreams);
                            }
                            var productImage = new ProductImage
                            {
                                ImageUrl = $@"\{folderPath}\{fileName}",
                                ProductId = product.Id
                            };
                            product.ProductImages.Add(productImage);
                            await productImageRepository.Add(productImage);
                        }
                    }
					else
					{

                        product.ProductImages[0].ImageUrl = "https://placehold.co/600x400";
					}

					productRepository.Update(product);
                    await productRepository.Save();

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
        public async Task<ActionResult> Update([FromBody] ProductDto productDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var product = await productRepository.Get(x => x.Id == productDto.Id);
                    string wwwRootPath = webHostEnvironment.WebRootPath is null
                        ? Directory
                            .CreateDirectory($"{Directory.GetCurrentDirectory()}\\wwwroot")
                            .FullName
                        : webHostEnvironment.WebRootPath;

                    if (productDto.ProductImages != null)
                    {
                        foreach (IFormFile file in productDto.ProductImages)
                        {
                            string fileName =
                                Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var folderPath = $@"images\products\product-{product.Id}";
                            var filePath = Path.Combine(wwwRootPath, folderPath);
                            if (!Directory.Exists(filePath))
                            {
                                Directory.CreateDirectory(filePath);
                            }
                            using (
                                var fileStreams = new FileStream(
                                    Path.Combine(filePath, fileName),
                                    FileMode.Create
                                )
                            )
                            {
                                file.CopyTo(fileStreams);
                            }
                            ProductImage productImage = new ProductImage()
                            {
                                ImageUrl = $@"\{folderPath}\{fileName}",
                                ProductId = product.Id
                            };
                            if (product.ProductImages is null)
                                product.ProductImages = new List<ProductImage>();
                            product.ProductImages.Add(productImage);
                            await productImageRepository.Add(productImage);
                        }
                    }
                    // await productImageRepository.AddRange(product.ProductImages);
                    productRepository.Update(product);
                    await productRepository.Save();
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
                var Product = await productRepository.Get(x => x.Id == id);
                if (Product is null)
                    return NotFound();
                var folderPath = $@"images\products\product-{id}";
                var finalPath = Path.Combine(webHostEnvironment.WebRootPath, folderPath);
                if (Directory.Exists(finalPath))
                {
                    var files = Directory.GetFiles(finalPath);
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                    Directory.Delete(finalPath);
                }
                productRepository.Remove(Product);
                await productRepository.Save();
                return Ok();
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("image/{ImageId:int}")]
        public async Task<ActionResult<int>> DeleteImage(int? ImageId)
        {
            if (ImageId == null || ImageId == 0)
            {
                return BadRequest();
            }
            try
            {
                var imageToBeDeleted = await productImageRepository.Get(x => x.Id == ImageId);
                if (imageToBeDeleted is null)
                    return NotFound();
                else
                {
                    if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                    {
                        var oldPath = Path.Combine(
                            webHostEnvironment.WebRootPath,
                            imageToBeDeleted.ImageUrl.TrimStart('\\')
                        );
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }
                    productImageRepository.Remove(imageToBeDeleted);
                    await productImageRepository.Save();
                    return Ok(imageToBeDeleted.ProductId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString(), ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
