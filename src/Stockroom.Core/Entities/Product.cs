namespace Stockroom.Core.Entities;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public long PriceCents { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}
