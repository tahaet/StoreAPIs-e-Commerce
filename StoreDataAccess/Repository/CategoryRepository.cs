using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreDataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly AppDbContext _db;

        public CategoryRepository(AppDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            _db.Update(category);
        }
    }
}
