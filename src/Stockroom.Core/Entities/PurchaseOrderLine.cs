namespace Stockroom.Core.Entities;

public class PurchaseOrderLine
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public long UnitPriceCents { get; set; }
}
