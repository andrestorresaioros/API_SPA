namespace Application.Products.Dtos;

public class ProductCategoryResponse
{
    public int Id { get; init; }

    public required string CategoryName { get; init; }

    public string? Description { get; init; }

    public byte[]? Picture { get; init; }
}
