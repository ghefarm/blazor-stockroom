using System.Text.RegularExpressions;

namespace Stockroom.Core.Matching;

public static partial class ReferenceMatcher
{
    public static int Score(string deliveryNote, string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return 0;
        }

        var wanted = orderNumber.Trim();
        var found = GetTokenRegex()
            .Matches(deliveryNote)
            .Any(token => string.Equals(
                token.Value,
                wanted,
                StringComparison.OrdinalIgnoreCase));

        return found ? MatchWeights.Reference : 0;
    }

    // Hyphens belong to the token, so "PO-1042" stays whole.
    [GeneratedRegex(@"[\p{L}\p{N}-]+")]
    private static partial Regex GetTokenRegex();
}
