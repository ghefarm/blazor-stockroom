namespace Stockroom.Core.Reports;

// Invented counts: Stockroom stores the stock of today, not its
// value over time. July 2025 has no count, and 2026 ends in
// September.
public static class SampleStockCounts
{
    public static IReadOnlyList<int> Years { get; } = [2025, 2026];

    public static IReadOnlyList<StockCount> All { get; } =
    [
        new(new(2025, 1, 31), 4_120_000),
        new(new(2025, 2, 28), 4_310_000),
        new(new(2025, 3, 31), 4_050_000),
        new(new(2025, 4, 30), 4_480_000),
        new(new(2025, 5, 31), 4_620_000),
        new(new(2025, 6, 30), 4_390_000),
        new(new(2025, 8, 29), 4_870_000),
        new(new(2025, 9, 30), 5_010_000),
        new(new(2025, 10, 31), 5_240_000),
        new(new(2025, 11, 28), 5_560_000),
        new(new(2025, 12, 31), 4_930_000),
        new(new(2026, 1, 30), 4_710_000),
        new(new(2026, 2, 27), 4_860_000),
        new(new(2026, 3, 13), 5_150_000),
        new(new(2026, 3, 31), 5_020_000),
        new(new(2026, 4, 30), 5_330_000),
        new(new(2026, 5, 29), 5_470_000),
        new(new(2026, 6, 30), 5_210_000),
        new(new(2026, 7, 31), 5_690_000),
        new(new(2026, 8, 31), 5_840_000),
        new(new(2026, 9, 30), 6_020_000),
    ];
}
