using Stockroom.Core.Matching;

namespace Stockroom.Core.Tests.Matching;

public class MatchExplainerTests
{
    [Test]
    public void Explain_ListsStoredPointsOfSuggestion()
    {
        var delivery = TestData.CreateDelivery(
            3, supplierId: 5, totalCents: 50_400);
        var order = TestData.CreateOrder(
            1044, supplierId: 5, totalCents: 50_000);
        var row = TestData.Run([delivery], [order])
            .Single().Rows.Single();

        var lines = MatchExplainer.Explain(row);

        Assert.That(lines, Is.EqualTo(
        [
            "+20 total within 1 percent of the order",
            "+25 same supplier as the order",
            "+15 received within 14 days",
            "Score 60 of 100",
        ]));
    }

    [Test]
    public void Explain_ShowsNoNumbersForAutomaticMatch()
    {
        var delivery = TestData.CreateDelivery(1, supplierId: 7);
        var order = TestData.CreateOrder(1041, supplierId: 7);
        var row = TestData.Run([delivery], [order])
            .Single().Rows.Single();

        Assert.That(MatchExplainer.Explain(row), Is.Empty);
    }
}
