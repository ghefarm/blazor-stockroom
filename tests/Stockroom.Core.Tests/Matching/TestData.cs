using Stockroom.Core.Entities;
using Stockroom.Core.Matching;

namespace Stockroom.Core.Tests.Matching;

internal static class TestData
{
    public static readonly DateOnly Received = new(2026, 3, 10);
    public static readonly DateOnly Ordered = new(2026, 3, 2);

    public static Delivery CreateDelivery(
        int id,
        int supplierId,
        long totalCents = 120_000,
        string note = "")
    {
        return new Delivery
        {
            Id = id,
            SupplierId = supplierId,
            ReceivedOn = Received,
            DeliveryNote = note,
            TotalCents = totalCents,
        };
    }

    public static PurchaseOrder CreateOrder(
        int id,
        int supplierId,
        long totalCents = 120_000,
        DateOnly? orderedOn = null,
        OrderStatus status = OrderStatus.Open)
    {
        return new PurchaseOrder
        {
            Id = id,
            SupplierId = supplierId,
            OrderNumber = $"PO-{id}",
            OrderedOn = orderedOn ?? Ordered,
            Status = status,
            Lines =
            [
                new PurchaseOrderLine
                {
                    PurchaseOrderId = id,
                    Quantity = 1,
                    UnitPriceCents = totalCents,
                },
            ],
        };
    }

    public static IReadOnlyList<DeliveryOutcome> Run(
        IReadOnlyList<Delivery> deliveries,
        IReadOnlyList<PurchaseOrder> orders,
        IReadOnlyList<DeliveryMatch>? earlier = null,
        MatchingSettings? settings = null)
    {
        return MatchingEngine.Run(new MatchingInput(
            deliveries,
            orders,
            earlier ?? [],
            settings ?? new MatchingSettings()));
    }
}
