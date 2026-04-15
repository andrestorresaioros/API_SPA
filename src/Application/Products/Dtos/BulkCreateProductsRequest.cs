using System.ComponentModel.DataAnnotations;

namespace Application.Products.Dtos;

public class BulkCreateProductsRequest
{
    [Range(1, 100000)]
    public int Quantity { get; set; }

    // Optional. When omitted, the service prioritizes SERVIDORES/CLOUD categories if available.
    public IReadOnlyCollection<int>? CategoryIds { get; set; }

    [Range(100, 5000)]
    public int BatchSize { get; set; } = 1000;

    public bool UseRoundRobin { get; set; }
}
