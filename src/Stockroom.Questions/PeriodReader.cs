namespace Stockroom.Questions;

/// <summary>Reads a period from a question by fixed rules.</summary>
public sealed class PeriodReader(TimeProvider time)
{
    private static readonly string[] MonthNames =
    [
        "january", "february", "march", "april", "may", "june",
        "july", "august", "september", "october", "november",
        "december",
    ];

    public static bool IsPeriodWord(string word) =>
        MonthOf(word) is not null || YearOf(word) is not null;

    public Period? Read(string question)
    {
        var words = Words.Split(question);
        List<int> months = [.. words.Select(MonthOf).OfType<int>()
            .Distinct()];
        List<int> years = [.. words.Select(YearOf).OfType<int>()
            .Distinct()];

        // Two months or two years ask for a range. We read one value
        // per field, so we read nothing rather than half of it.
        if (months.Count > 1 || years.Count > 1)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(
            time.GetLocalNow().DateTime);
        if (months.Count == 1)
        {
            return ReadMonth(months[0], years, words, today);
        }

        if (years.Count == 1)
        {
            return Period.ForYear(years[0]);
        }

        return ReadRelative(words, today);
    }

    private static Period ReadMonth(
        int month,
        List<int> years,
        IReadOnlyList<string> words,
        DateOnly today)
    {
        var year = years.Count == 1 ? years[0]
            : HasPair(words, "last", "year") ? today.Year - 1
            : month <= today.Month ? today.Year
            : today.Year - 1;
        return Period.ForMonth(new DateOnly(year, month, 1));
    }

    private static Period? ReadRelative(
        IReadOnlyList<string> words, DateOnly today)
    {
        (string First, string Second, Period Period)[] phrases =
        [
            ("this", "month", Period.ForMonth(today)),
            ("last", "month", Period.ForMonth(today.AddMonths(-1))),
            ("this", "year", Period.ForYear(today.Year)),
            ("last", "year", Period.ForYear(today.Year - 1)),
        ];
        foreach (var phrase in phrases)
        {
            if (HasPair(words, phrase.First, phrase.Second))
            {
                return phrase.Period;
            }
        }

        return null;
    }

    private static bool HasPair(
        IReadOnlyList<string> words, string first, string second)
    {
        for (var i = 0; i + 1 < words.Count; i++)
        {
            if (words[i] == first && words[i + 1] == second)
            {
                return true;
            }
        }

        return false;
    }

    private static int? MonthOf(string word)
    {
        var index = Array.IndexOf(MonthNames, word);
        return index < 0 ? null : index + 1;
    }

    private static int? YearOf(string word) =>
        word.Length == 4
        && int.TryParse(word, out var year)
        && year is >= 2000 and <= 2099
            ? year
            : null;
}
