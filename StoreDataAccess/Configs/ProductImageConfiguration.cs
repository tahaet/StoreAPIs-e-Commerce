using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreModels;

namespace StoreDataAccess.Configs
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(255);
            builder.Ignore(x => x.Product);
            builder.Property(x => x.IsMainImage).HasDefaultValue(false).IsRequired();
        }
    }
}
