using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreModels;

namespace StoreDataAccess.Configs
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.City).HasMaxLength(50);
            builder.Property(x => x.State).HasMaxLength(50);
            builder.Property(x => x.PostalCode).HasMaxLength(100);
            builder.HasOne(x => x.Company).WithOne();

            builder.Ignore(x => x.Role);
        }
    }
}
