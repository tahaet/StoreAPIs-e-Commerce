using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreModels;

namespace StoreDataAccess.Configs
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder
                .Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasAnnotation("DisplayName", "Category Name");
            builder
                .Property(x => x.DisplayOrder)
                .IsRequired()
                .HasAnnotation("DisplayName", "Display Order");
            builder.Property(x => x.Description).IsRequired(false);
        }
    }
}
