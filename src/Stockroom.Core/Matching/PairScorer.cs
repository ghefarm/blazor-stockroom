using Stockroom.Core.Entities;

namespace Stockroom.Core.Matching;

public static class PairScorer
{
    public static PairScore Score(
        Delivery delivery, PurchaseOrder order, int daysAfterOrder)
    {
        var orderCents = order.Lines.Sum(
            line => line.Quantity * line.UnitPriceCents);

        return new PairScore(
            delivery.Id,
            order.Id,
            AmountMatcher.Score(delivery.TotalCents, orderCents),
            SupplierMatcher.Score(
                delivery.SupplierId, order.SupplierId),
            DateMatcher.Score(
                order.OrderedOn, delivery.ReceivedOn, daysAfterOrder),
            ReferenceMatcher.Score(
                delivery.DeliveryNote, order.OrderNumber));
    }
}
