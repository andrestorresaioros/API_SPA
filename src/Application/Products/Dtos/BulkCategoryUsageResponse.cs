namespace Application.Products.Dtos;

public class BulkCategoryUsageResponse
{
    public int CategoryId { get; init; }

    public required string CategoryName { get; init; }

    public int CreatedCount { get; init; }
}
