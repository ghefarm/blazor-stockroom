using System.Text.RegularExpressions;

namespace Stockroom.Questions;

public static partial class Words
{
    // These words sit in almost every question and in many labels.
    // Counted as evidence, they would make any choice look named.
    private static readonly HashSet<string> Fillers =
    [
        "the", "a", "an", "of", "in", "on", "for", "from", "with",
        "and", "or", "all", "show", "me", "my", "order", "orders",
        "this", "last", "month", "months", "year", "years",
    ];

    public static IReadOnlyList<string> Split(string text) =>
        [.. WordPattern().Matches(text.ToLowerInvariant())
            .Select(match => match.Value)];

    public static bool IsFiller(string word) =>
        Fillers.Contains(word);

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex WordPattern();
}
