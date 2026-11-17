using Stockroom.Core.Entities;

namespace Stockroom.Core.Matching;

public static class MatchingEngine
{
    public static IReadOnlyList<DeliveryOutcome> Run(
        MatchingInput input)
    {
        var errors = MatchingSettingsRules.Validate(input.Settings);

        // The settings are validated before they are saved,
        // so an invalid row here is a bug, not a state to show.
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Invalid matching settings: "
                + string.Join(" ", errors));
        }

        var pairs = BuildPairs(input);
        var candidates = Aggregate(
            input.Deliveries, pairs, input.Settings);

        return candidates
            .Select(c => Assign(c, input.Settings))
            .ToList();
    }

    private static List<PairScore> BuildPairs(MatchingInput input)
    {
        var rejected = input.EarlierMatches
            .Where(m => m.Decision == MatchDecision.Rejected)
            .Select(m => (m.DeliveryId, m.PurchaseOrderId))
            .ToHashSet();

        List<PairScore> pairs = [];
        foreach (var delivery in input.Deliveries)
        {
            foreach (var order in input.Orders)
            {
                if (!CanReceiveGoods(order)
                    || rejected.Contains((delivery.Id, order.Id)))
                {
                    continue;
                }

                pairs.Add(PairScorer.Score(
                    delivery, order, input.Settings.DaysAfterOrder));
            }
        }

        return pairs;
    }

    private static bool CanReceiveGoods(PurchaseOrder order)
    {
        return order.Status is OrderStatus.Open
            or OrderStatus.PartlyDelivered;
    }

    private static List<DeliveryCandidates> Aggregate(
        IReadOnlyList<Delivery> deliveries,
        List<PairScore> pairs,
        MatchingSettings settings)
    {
        var byDelivery = pairs.ToLookup(p => p.DeliveryId);

        return deliveries
            .Select(d => d.Id)
            .Distinct()
            .Order()
            .Select(id => Summarize(id, byDelivery[id], settings))
            .ToList();
    }

    private static DeliveryCandidates Summarize(
        int deliveryId,
        IEnumerable<PairScore> pairs,
        MatchingSettings settings)
    {
        var all = pairs.ToList();
        var references = all.Where(p => p.HasReference).ToList();

        // A reference that fits several orders identifies none.
        var unique = references.Count == 1 ? references[0] : null;

        var ranked = all
            .Where(p => p.Total >= settings.SuggestThreshold)
            .OrderByDescending(p => p.Total)
            .ThenByDescending(p => p.HasReference)
            .ThenBy(p => p.PurchaseOrderId)
            .ToList();

        return new DeliveryCandidates(deliveryId, ranked, unique);
    }

    private static DeliveryOutcome Assign(
        DeliveryCandidates candidates, MatchingSettings settings)
    {
        var id = candidates.DeliveryId;

        if (candidates.UniqueReference is { } hard)
        {
            return Link(id, MatchStatus.Automatic, [hard]);
        }

        var ranked = candidates.Ranked;
        if (ranked.Count == 0)
        {
            return new DeliveryOutcome(
                id, DeliveryMatchState.NoCandidate, []);
        }

        var best = ranked[0];
        var close = ranked
            .Where(p => best.Total - p.Total < settings.MinimumLead)
            .ToList();

        if (close.Count > 1)
        {
            return Link(id, MatchStatus.Ambiguous, close);
        }

        var status = best.Total >= settings.AutoThreshold
            ? MatchStatus.Automatic
            : MatchStatus.Suggested;

        return Link(id, status, [best]);
    }

    private static DeliveryOutcome Link(
        int deliveryId,
        MatchStatus status,
        IReadOnlyList<PairScore> pairs)
    {
        var state = status switch
        {
            MatchStatus.Automatic => DeliveryMatchState.Automatic,
            MatchStatus.Suggested => DeliveryMatchState.Suggested,
            MatchStatus.Ambiguous => DeliveryMatchState.Ambiguous,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status)),
        };

        var rows = pairs.Select(p => CreateRow(p, status)).ToList();
        return new DeliveryOutcome(deliveryId, state, rows);
    }

    private static DeliveryMatch CreateRow(
        PairScore pair, MatchStatus status)
    {
        return new DeliveryMatch
        {
            DeliveryId = pair.DeliveryId,
            PurchaseOrderId = pair.PurchaseOrderId,
            Score = pair.Total,
            Status = status,
            Decision = MatchDecision.Open,
            AmountPoints = pair.Amount,
            SupplierPoints = pair.Supplier,
            DatePoints = pair.Date,
            ReferencePoints = pair.Reference,
        };
    }
}
