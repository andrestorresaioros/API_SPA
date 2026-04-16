namespace Application.Products.Dtos;

public class ProductListItemResponse
{
    public int Id { get; init; }

    public required string ProductName { get; init; }

    public int CategoryId { get; init; }

    public required string CategoryName { get; init; }

    public decimal UnitPrice { get; init; }

    public short UnitsInStock { get; init; }

    public bool Discontinued { get; init; }
}
