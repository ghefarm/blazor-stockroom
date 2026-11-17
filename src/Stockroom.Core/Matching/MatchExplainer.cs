using Stockroom.Core.Entities;

namespace Stockroom.Core.Matching;

public static class MatchExplainer
{
    public static IReadOnlyList<string> Explain(DeliveryMatch match)
    {
        // Nobody has to act on an automatic match: no numbers.
        if (match.Status == MatchStatus.Automatic)
        {
            return [];
        }

        (int Points, string Text)[] parts =
        [
            (match.AmountPoints, DescribeAmount(match.AmountPoints)),
            (match.SupplierPoints, "same supplier as the order"),
            (match.DatePoints, DescribeDate(match.DatePoints)),
            (match.ReferencePoints, "delivery note names the order"),
        ];

        List<string> lines =
        [
            .. parts
                .Where(p => p.Points > 0)
                .Select(p => $"+{p.Points} {p.Text}"),
            $"Score {match.Score} of {MatchWeights.MaxScore}",
        ];

        if (match.Status == MatchStatus.Ambiguous)
        {
            lines.Add("Another order scores almost as high.");
        }

        return lines;
    }

    private static string DescribeAmount(int points)
    {
        return points == MatchWeights.AmountExact
            ? "total equals the order total"
            : "total within 1 percent of the order";
    }

    private static string DescribeDate(int points)
    {
        return points == MatchWeights.ReceivedSoon
            ? $"received within {MatchWeights.SoonDays} days"
            : "received late, but in the date window";
    }
}
