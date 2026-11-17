using Stockroom.Core.Entities;

namespace Stockroom.Core.Reports;

public static class StockReportCalculator
{
    public static IReadOnlyList<MonthValue> CalculateMonthlyValues(
        IEnumerable<StockCount> counts, int year)
    {
        // One pass over all counts. After that, a month is a lookup.
        var lastPerMonth = counts
            .Where(c => c.CountedOn.Year == year)
            .GroupBy(c => c.CountedOn.Month)
            .ToDictionary(
                g => g.Key,
                g => g.MaxBy(c => c.CountedOn)!.ValueCents);

        List<MonthValue> months = [];

        for (var month = 1; month <= 12; month++)
        {
            long? value = lastPerMonth.TryGetValue(month, out var v)
                ? v
                : null;
            months.Add(new(new DateOnly(year, month, 1), value));
        }

        return months;
    }

    public static IReadOnlyList<LevelCount> CountByLevel(
        IEnumerable<Product> products, bool includeInactive)
    {
        var counts = products
            .Where(p => includeInactive || p.IsActive)
            .GroupBy(p => StockRules.DetermineLevel(p.StockQuantity))
            .ToDictionary(g => g.Key, g => g.Count());

        // Here zero is a real answer: no product has this level.
        return
        [
            .. Enum.GetValues<StockLevel>().Select(
                level => new LevelCount(
                    level, counts.GetValueOrDefault(level))),
        ];
    }
}
