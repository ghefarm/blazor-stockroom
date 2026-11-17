using Stockroom.Core.Entities;
using Stockroom.Core.Reports;

namespace Stockroom.Core.Tests.Reports;

public class StockReportCalculatorTests
{
    [Test]
    public void Report_TakesLastCountPerMonthAndCountsLevels()
    {
        StockCount[] counts =
        [
            new(new(2026, 3, 13), 5_150_000),
            new(new(2026, 3, 31), 5_020_000),
            new(new(2026, 4, 30), 5_330_000),
        ];
        Product[] products =
        [
            new() { StockQuantity = 50, IsActive = true },
            new() { StockQuantity = 3, IsActive = true },
            new() { StockQuantity = 0, IsActive = false },
        ];

        var months = StockReportCalculator.CalculateMonthlyValues(
            counts, 2026);
        var levels = StockReportCalculator.CountByLevel(
            products, includeInactive: true);

        Assert.That(months, Has.Count.EqualTo(12));
        Assert.That(months[2].ValueCents, Is.EqualTo(5_020_000));
        Assert.That(months[3].ValueCents, Is.EqualTo(5_330_000));
        Assert.That(
            levels.Select(l => l.Count), Is.EqualTo([1, 1, 1]));
    }

    [Test]
    public void Report_MissingMonthIsNull_EmptyLevelIsZero()
    {
        StockCount[] counts = [new(new(2025, 12, 31), 4_930_000)];
        Product[] products =
        [
            new() { StockQuantity = 50, IsActive = true },
            new() { StockQuantity = 0, IsActive = false },
        ];

        var months = StockReportCalculator.CalculateMonthlyValues(
            counts, 2026);
        var levels = StockReportCalculator.CountByLevel(
            products, includeInactive: false);

        Assert.That(months.Select(m => m.ValueCents), Is.All.Null);
        Assert.That(
            levels.Select(l => l.Count), Is.EqualTo([1, 0, 0]));
    }
}
