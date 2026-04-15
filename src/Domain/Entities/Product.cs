namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string? QuantityPerUnit { get; set; }

    public decimal UnitPrice { get; set; }

    public short UnitsInStock { get; set; }

    public short UnitsOnOrder { get; set; }

    public short ReorderLevel { get; set; }

    public bool Discontinued { get; set; }

    public Category? Category { get; set; }
}
