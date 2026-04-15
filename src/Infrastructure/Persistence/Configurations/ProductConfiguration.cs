using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(product => product.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(product => product.QuantityPerUnit)
            .HasColumnName("quantity_per_unit")
            .HasMaxLength(50);

        builder.Property(product => product.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2);

        builder.Property(product => product.UnitsInStock)
            .HasColumnName("units_in_stock");

        builder.Property(product => product.UnitsOnOrder)
            .HasColumnName("units_on_order");

        builder.Property(product => product.ReorderLevel)
            .HasColumnName("reorder_level");

        builder.Property(product => product.Discontinued)
            .HasColumnName("discontinued")
            .HasDefaultValue(false);

        builder.HasIndex(product => product.ProductName)
            .HasDatabaseName("ix_products_product_name");

        builder.HasIndex(product => product.CategoryId)
            .HasDatabaseName("ix_products_category_id");
    }
}
