namespace Application.Products.Dtos;

public class ProductDetailResponse
{
    public int Id { get; init; }

    public required string ProductName { get; init; }

    public int CategoryId { get; init; }

    public string? QuantityPerUnit { get; init; }

    public decimal UnitPrice { get; init; }

    public short UnitsInStock { get; init; }

    public short UnitsOnOrder { get; init; }

    public short ReorderLevel { get; init; }

    public bool Discontinued { get; init; }

    public required ProductCategoryResponse Category { get; init; }
}
