using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreModels;

namespace StoreDataAccess.Configs
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
        public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductId).IsRequired();

            builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);

            builder.Property(x => x.Count).IsRequired().HasAnnotation("Range", new[] { 1, 1000 });

            builder.Property(x => x.ApplicationUserId).IsRequired();

            builder
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId);

            builder.Ignore(x => x.Price);
        }
    }
}
