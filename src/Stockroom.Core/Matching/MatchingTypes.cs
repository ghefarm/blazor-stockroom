using Stockroom.Core.Entities;

namespace Stockroom.Core.Matching;

public sealed record MatchingInput(
    IReadOnlyList<Delivery> Deliveries,
    IReadOnlyList<PurchaseOrder> Orders,
    IReadOnlyList<DeliveryMatch> EarlierMatches,
    MatchingSettings Settings);

// What the assign pass needs to know about one delivery.
public sealed record DeliveryCandidates(
    int DeliveryId,
    IReadOnlyList<PairScore> Ranked,
    PairScore? UniqueReference);

public enum DeliveryMatchState
{
    NoCandidate,
    Automatic,
    Suggested,
    Ambiguous,
}

public sealed record DeliveryOutcome(
    int DeliveryId,
    DeliveryMatchState State,
    IReadOnlyList<DeliveryMatch> Rows);
