using Stockroom.Core.Entities;
using Stockroom.Core.Matching;

namespace Stockroom.Core.Tests.Matching;

// The first version. It decides while it scores, and it lives
// here only to show why the engine works in fixed passes.
internal static class GreedyMatcher
{
    public static List<DeliveryMatch> AssignWhileScoring(
        IEnumerable<Delivery> deliveries,
        IEnumerable<PurchaseOrder> orders,
        MatchingSettings settings)
    {
        List<DeliveryMatch> rows = [];
        foreach (var delivery in deliveries)
        {
            foreach (var order in orders)
            {
                var pair = PairScorer.Score(
                    delivery, order, settings.DaysAfterOrder);

                if (pair.Total >= settings.AutoThreshold)
                {
                    rows.Add(new DeliveryMatch
                    {
                        DeliveryId = delivery.Id,
                        PurchaseOrderId = order.Id,
                        Score = pair.Total,
                        Status = MatchStatus.Automatic,
                    });
                    break;
                }
            }
        }

        return rows;
    }
}
