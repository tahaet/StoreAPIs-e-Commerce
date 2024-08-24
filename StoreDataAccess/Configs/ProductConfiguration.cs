using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreModels;

namespace StoreDataAccess.Configs
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasMany(x => x.ProductImages).WithOne().HasForeignKey(x => x.ProductId);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(255);
            builder.Property(x => x.QuantityInStock).IsRequired();
            builder
                .Property(x => x.ListPrice)
                .IsRequired()
                .HasAnnotation("DisplayName", "List Price");
            builder
                .Property(x => x.Price)
                .IsRequired()
                .HasAnnotation("DisplayName", "Price for 1-50");
            builder
                .Property(x => x.Price50)
                .IsRequired()
                .HasAnnotation("DisplayName", "Price for 50-100");
            builder
                .Property(x => x.Price100)
                .IsRequired()
                .HasAnnotation("DisplayName", "Price for 100+");
            //builder.Ignore(x => x.Category);
            //builder.Ignore(x => x.ProductImages);
            builder.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);

            builder.HasMany(x => x.ProductImages).WithOne().HasForeignKey(x => x.ProductId);
        }
    }
}
