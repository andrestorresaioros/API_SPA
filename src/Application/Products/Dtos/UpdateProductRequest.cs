using System.ComponentModel.DataAnnotations;

namespace Application.Products.Dtos;

public class UpdateProductRequest
{
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [MaxLength(50)]
    public string? QuantityPerUnit { get; set; }

    [Range(0, 999999999)]
    public decimal UnitPrice { get; set; }

    [Range(0, short.MaxValue)]
    public short UnitsInStock { get; set; }

    [Range(0, short.MaxValue)]
    public short UnitsOnOrder { get; set; }

    [Range(0, short.MaxValue)]
    public short ReorderLevel { get; set; }

    public bool Discontinued { get; set; }
}
