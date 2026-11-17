namespace Stockroom.Core.Matching;

// Constants, not settings: the test cases are calibrated
// against these values, and a change must break a test.
public static class MatchWeights
{
    public const int AmountExact = 40;
    public const int AmountWithinOnePercent = 20;
    public const int SameSupplier = 25;
    public const int ReceivedSoon = 15;
    public const int ReceivedLate = 5;
    public const int SoonDays = 14;
    public const int Reference = 20;
    public const int MaxScore = 100;
}
