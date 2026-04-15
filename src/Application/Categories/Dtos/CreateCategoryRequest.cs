using System.ComponentModel.DataAnnotations;

namespace Application.Categories.Dtos;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public byte[]? Picture { get; set; }
}
