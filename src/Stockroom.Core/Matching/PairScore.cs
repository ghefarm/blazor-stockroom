namespace Stockroom.Core.Matching;

public sealed record PairScore(
    int DeliveryId,
    int PurchaseOrderId,
    int Amount,
    int Supplier,
    int Date,
    int Reference)
{
    public int Total => Math.Min(
        MatchWeights.MaxScore, Amount + Supplier + Date + Reference);

    public bool HasReference => Reference > 0;
}
