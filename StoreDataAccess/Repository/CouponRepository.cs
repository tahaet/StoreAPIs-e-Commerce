using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreDataAccess.Repository
{
    public class CouponRepository : Repository<Coupon>, ICouponRepository
    {
        private readonly AppDbContext _db;

        public CouponRepository(AppDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(Coupon coupon)
        {
            _db.Update(coupon);
        }
    }
}
