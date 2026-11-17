namespace Stockroom.Core.Matching;

public static class AmountMatcher
{
    public static int Score(long deliveryCents, long orderCents)
    {
        if (deliveryCents == orderCents)
        {
            return MatchWeights.AmountExact;
        }

        var difference = Math.Abs(deliveryCents - orderCents);

        // Multiply instead of divide, so no rounding is needed.
        if (orderCents > 0 && difference * 100 <= orderCents)
        {
            return MatchWeights.AmountWithinOnePercent;
        }

        return 0;
    }
}
