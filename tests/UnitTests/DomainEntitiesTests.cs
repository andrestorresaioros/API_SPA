using Domain.Entities;

namespace UnitTests;

public class DomainEntitiesTests
{
    [Fact]
    public void Category_ShouldInitialize_WithEmptyProductCollection()
    {
        var category = new Category();

        Assert.NotNull(category.Products);
        Assert.Empty(category.Products);
    }

    [Fact]
    public void Product_ShouldKeepAssignedValues()
    {
        var product = new Product
        {
            ProductName = "Chai",
            CategoryId = 1,
            UnitPrice = 10.50m,
            UnitsInStock = 5,
            Discontinued = false
        };

        Assert.Equal("Chai", product.ProductName);
        Assert.Equal(1, product.CategoryId);
        Assert.Equal(10.50m, product.UnitPrice);
        Assert.Equal((short)5, product.UnitsInStock);
        Assert.False(product.Discontinued);
    }
}
