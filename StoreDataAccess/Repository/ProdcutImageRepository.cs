using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreDataAccess.Repository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private readonly AppDbContext _db;

        public ProductImageRepository(AppDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(ProductImage productImage)
        {
            _db.Update(productImage);
        }

        // public async Task AddRange(IEnumerable<ProductImage> productImages)
        // {
        //     await _db.AddRangeAsync(productImages);
        // }
    }
}
