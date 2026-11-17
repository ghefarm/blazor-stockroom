namespace Stockroom.Core.Entities;

public class Delivery
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public DateOnly ReceivedOn { get; set; }
    public string DeliveryNote { get; set; } = "";
    public long TotalCents { get; set; }
}
