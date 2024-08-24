using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreDataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly AppDbContext _db;

        public ShoppingCartRepository(AppDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(ShoppingCart ShoppingCart)
        {
            _db.Update(ShoppingCart);
        }
    }
}
