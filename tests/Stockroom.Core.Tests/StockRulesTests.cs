namespace Stockroom.Core.Tests;

public class StockRulesTests
{
    [Test]
    public void CountsEveryProductOnceAtItsLevel()
    {
        var counts = StockRules.CountLevels(
        [
            new() { StockQuantity = 0 },
            new() { StockQuantity = -2 },
            new() { StockQuantity = 9 },
            new() { StockQuantity = 10 },
            new() { StockQuantity = 240 },
        ]);

        Assert.That(counts, Is.EqualTo(new StockCounts(2, 1, 2)));
    }
}
