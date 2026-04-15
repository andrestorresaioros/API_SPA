using System.ComponentModel.DataAnnotations;

namespace Application.Products.Dtos;

public class ProductsQueryRequest
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 20;

    [MaxLength(200)]
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    [Range(0, 999999999)]
    public decimal? MinPrice { get; set; }

    [Range(0, 999999999)]
    public decimal? MaxPrice { get; set; }
}
