using Stockroom.Core.Matching;

namespace Stockroom.Core.Tests.Matching;

public class MatcherTests
{
    [Test]
    public void Score_GivesFullPointsForPerfectPair()
    {
        var delivery = TestData.CreateDelivery(
            1, supplierId: 7, note: "Your order PO-1042.");
        var order = TestData.CreateOrder(1042, supplierId: 7);

        var pair = PairScorer.Score(delivery, order, 30);

        Assert.That(pair, Is.EqualTo(
            new PairScore(1, 1042, 40, 25, 15, 20)));
        Assert.That(pair.Total, Is.EqualTo(100));
        Assert.That(AmountMatcher.Score(101_000, 100_000),
            Is.EqualTo(20));
    }

    [Test]
    public void Score_GivesNothingForWeakEvidence()
    {
        var ordered = new DateOnly(2026, 3, 2);

        Assert.Multiple(() =>
        {
            Assert.That(AmountMatcher.Score(102_000, 100_000),
                Is.Zero);
            Assert.That(SupplierMatcher.Score(7, 8), Is.Zero);
            Assert.That(DateMatcher.Score(
                ordered, ordered.AddDays(-1), 30), Is.Zero);
            Assert.That(DateMatcher.Score(
                ordered, ordered.AddDays(31), 30), Is.Zero);
            Assert.That(ReferenceMatcher.Score(
                "Ref PO-10421", "PO-1042"), Is.Zero);
        });
    }
}
