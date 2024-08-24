using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StoreDataAccess.Repository.IRepository;
using StoreModels;

namespace StoreDataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly AppDbContext _db;

        public ApplicationUserRepository(AppDbContext db)
            : base(db)
        {
            _db = db;
        }

        public void Update(ApplicationUser applicationUser)
        {
            _db.ApplicationUsers.Update(applicationUser);
        }
    }
}
