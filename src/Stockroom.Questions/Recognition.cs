namespace Stockroom.Questions;

/// <summary>Decides if a question visibly names a choice.</summary>
public static class Recognition
{
    private const int MinimumPrefix = 4;

    public static bool IsNamed(
        IReadOnlyList<string> questionWords,
        Choice choice,
        IReadOnlyList<Choice> allChoices)
    {
        var own = FindKeyWords(choice.Label);
        var shared = allChoices
            .Where(other => other != choice)
            .SelectMany(other => FindKeyWords(other.Label))
            .ToHashSet();

        // A word that several labels share cannot tell them apart.
        List<string> distinctive =
            [.. own.Where(word => !shared.Contains(word))];
        var evidence = distinctive.Count > 0 ? distinctive : own;
        return evidence.Any(labelWord => questionWords
            .Where(word => !Words.IsFiller(word))
            .Any(word => SharePrefix(word, labelWord)));
    }

    private static List<string> FindKeyWords(string label) =>
        [.. Words.Split(label).Where(word => !Words.IsFiller(word))];

    private static bool SharePrefix(string a, string b)
    {
        if (a == b)
        {
            return true;
        }

        var (shorter, longer) = a.Length < b.Length ? (a, b) : (b, a);
        return shorter.Length >= MinimumPrefix
            && longer.StartsWith(shorter, StringComparison.Ordinal);
    }
}
