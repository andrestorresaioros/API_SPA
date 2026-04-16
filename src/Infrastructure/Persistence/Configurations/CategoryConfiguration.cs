using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(category => category.CategoryName)
            .HasColumnName("category_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(category => category.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(category => category.Picture)
            .HasColumnName("picture");

        builder.HasMany(category => category.Products)
            .WithOne(product => product.Category)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
