namespace Stockroom.Core.Entities;

public class PurchaseOrder : IAuditable
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string OrderNumber { get; set; } = "";
    public DateOnly OrderedOn { get; set; }
    public OrderStatus Status { get; set; }
    public List<PurchaseOrderLine> Lines { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
