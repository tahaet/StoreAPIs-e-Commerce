using Microsoft.AspNetCore.Http;

namespace StoreModels.Dtos
{
    public class ProductDto
    {
        public int? Id { get; set; } = null;
        public string Name { get; set; }
        public string? Description { get; set; }
        public int QuantityInStock { get; set; }
        public double ListPrice { get; set; }
        public double Price { get; set; }
        public double Price50 { get; set; }
        public double Price100 { get; set; }
        public int CategoryId { get; set; }
        public List<IFormFile> ProductImages { get; set; }
    }
}
