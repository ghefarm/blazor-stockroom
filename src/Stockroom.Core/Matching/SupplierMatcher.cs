namespace Stockroom.Core.Matching;

public static class SupplierMatcher
{
    public static int Score(
        int deliverySupplierId, int orderSupplierId)
    {
        return deliverySupplierId == orderSupplierId
            ? MatchWeights.SameSupplier
            : 0;
    }
}
