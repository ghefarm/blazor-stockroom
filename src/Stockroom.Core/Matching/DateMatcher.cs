namespace Stockroom.Core.Matching;

public static class DateMatcher
{
    public static int Score(
        DateOnly orderedOn, DateOnly receivedOn, int daysAfterOrder)
    {
        var days = receivedOn.DayNumber - orderedOn.DayNumber;

        if (days < 0)
        {
            return 0;
        }

        if (days <= MatchWeights.SoonDays)
        {
            return MatchWeights.ReceivedSoon;
        }

        if (days <= daysAfterOrder)
        {
            return MatchWeights.ReceivedLate;
        }

        return 0;
    }
}
