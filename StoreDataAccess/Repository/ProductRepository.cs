using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreDataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly AppDbContext _db;

        public ProductRepository(AppDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            _db.Update(product);
        }

        public async Task<List<Product>> GetAllProductsWithMainImage()
        {
            return await _db
                .Products.FromSql(
                    $"Select * from Products inner join ProductImages on Products.Id = ProductImages.ProductId inner join Categories on Categories.Id= Products.CategoryId where ProductImages.IsMainImage = 1"
                )
                .ToListAsync();
        }
    }
}
