namespace Domain.Entities;

public class Category
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public byte[]? Picture { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
