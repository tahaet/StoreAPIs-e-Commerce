﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoreModels;
using StoreUtility;

namespace StoreDataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public void Initialize()
        {
            //migrations if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) { }

            //create roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager
                    .CreateAsync(new IdentityRole(SD.Role_Employee))
                    .GetAwaiter()
                    .GetResult();
                _roleManager
                    .CreateAsync(new IdentityRole(SD.Role_Company))
                    .GetAwaiter()
                    .GetResult();
                _roleManager
                    .CreateAsync(new IdentityRole(SD.Role_Customer))
                    .GetAwaiter()
                    .GetResult();

                //if roles are not created, then we will create admin user as well

                _userManager
                    .CreateAsync(
                        new ApplicationUser
                        {
                            UserName = "admin@gmail.com",
                            Email = "admin@gmail.com",
                            Name = "Admin admin",
                            PhoneNumber = "1112223333",
                            StreetAddress = "test 123 Ave",
                            State = "Egypt",
                            PostalCode = "23422",
                            City = "cairo"
                        },
                        "Admin@1234"
                    )
                    .GetAwaiter()
                    .GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u =>
                    u.Email == "admin@gmail.com"
                )!;

                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
