using Stockroom.Core.Entities;
using Stockroom.Core.Matching;

namespace Stockroom.Core.Tests.Matching;

public class InputOrderTests
{
    private static readonly Delivery Delivery =
        TestData.CreateDelivery(1, supplierId: 7);
    private static readonly PurchaseOrder First =
        TestData.CreateOrder(1041, supplierId: 7);
    private static readonly PurchaseOrder Second =
        TestData.CreateOrder(1042, supplierId: 7);

    [Test]
    public void GreedyMatcher_DependsOnInputOrder()
    {
        var settings = new MatchingSettings();

        var forward = GreedyMatcher.AssignWhileScoring(
            [Delivery], [First, Second], settings);
        var backward = GreedyMatcher.AssignWhileScoring(
            [Delivery], [Second, First], settings);

        Assert.That(forward.Single().PurchaseOrderId,
            Is.EqualTo(1041));
        Assert.That(backward.Single().PurchaseOrderId,
            Is.EqualTo(1042));
    }

    [Test]
    public void Engine_GivesSameResultForAnyInputOrder()
    {
        var forward = TestData.Run([Delivery], [First, Second]);
        var backward = TestData.Run([Delivery], [Second, First]);

        Assert.That(forward.Single().State,
            Is.EqualTo(DeliveryMatchState.Ambiguous));
        Assert.That(Summarize(backward),
            Is.EqualTo(Summarize(forward)));
    }

    private static List<(int, MatchStatus, int)> Summarize(
        IReadOnlyList<DeliveryOutcome> outcomes)
    {
        return outcomes
            .SelectMany(o => o.Rows)
            .Select(r => (r.PurchaseOrderId, r.Status, r.Score))
            .ToList();
    }
}
