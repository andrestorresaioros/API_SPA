namespace Application.Products.Dtos;

public class BulkCreateProductsResponse
{
    public int RequestedCount { get; init; }

    public int CreatedCount { get; init; }

    public int BatchSize { get; init; }

    public long ElapsedMilliseconds { get; init; }

    public required IReadOnlyCollection<BulkCategoryUsageResponse> CategoriesUsed { get; init; }
}
