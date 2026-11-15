namespace Stockroom.Core.Entities;

public class PurchaseOrder
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateOnly OrderedOn { get; set; }
    public OrderStatus Status { get; set; }
    public List<PurchaseOrderLine> Lines { get; set; } = [];
}
