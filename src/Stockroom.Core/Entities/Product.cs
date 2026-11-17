namespace Stockroom.Core.Entities;

public class Product : IAuditable
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public long PriceCents { get; set; }
    public int StockQuantity { get; set; }

    // The same value as the database default, so C# and the
    // database agree on what a new product is.
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
