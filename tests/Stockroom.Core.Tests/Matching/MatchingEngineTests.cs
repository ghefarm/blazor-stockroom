using Stockroom.Core.Entities;
using Stockroom.Core.Matching;

namespace Stockroom.Core.Tests.Matching;

public class MatchingEngineTests
{
    [Test]
    public void Run_LinksClearBestAndHardIdentifier()
    {
        List<PurchaseOrder> orders =
        [
            TestData.CreateOrder(1041, supplierId: 7),
            TestData.CreateOrder(1042, supplierId: 8),
            TestData.CreateOrder(1043, supplierId: 9,
                totalCents: 99_000,
                orderedOn: new DateOnly(2026, 1, 5)),
            TestData.CreateOrder(1044, supplierId: 5,
                totalCents: 50_000),
        ];
        List<Delivery> deliveries =
        [
            TestData.CreateDelivery(1, supplierId: 7),
            TestData.CreateDelivery(2, supplierId: 3,
                totalCents: 10_000, note: "Goods for PO-1043."),
            TestData.CreateDelivery(3, supplierId: 5,
                totalCents: 50_400),
        ];

        var outcomes = TestData.Run(deliveries, orders);

        Assert.That(outcomes.Select(o => o.State), Is.EqualTo(
        [
            DeliveryMatchState.Automatic,
            DeliveryMatchState.Automatic,
            DeliveryMatchState.Suggested,
        ]));
        var rows = outcomes.SelectMany(o => o.Rows).ToList();
        Assert.That(rows.Select(r => r.PurchaseOrderId),
            Is.EqualTo([1041, 1043, 1044]));
        Assert.That(rows.Select(r => r.Score),
            Is.EqualTo([80, 20, 60]));
    }

    [Test]
    public void Run_WritesNothingThatIsNotClear()
    {
        List<PurchaseOrder> orders =
        [
            TestData.CreateOrder(1051, supplierId: 7),
            TestData.CreateOrder(1052, supplierId: 7,
                status: OrderStatus.Cancelled),
            TestData.CreateOrder(1053, supplierId: 8),
        ];
        List<Delivery> deliveries =
        [
            TestData.CreateDelivery(1, supplierId: 7),
            TestData.CreateDelivery(2, supplierId: 4,
                totalCents: 5_000, note: "PO-1051 and PO-1053"),
            TestData.CreateDelivery(3, supplierId: 4,
                totalCents: 100),
        ];
        List<DeliveryMatch> earlier =
        [
            new DeliveryMatch
            {
                DeliveryId = 1,
                PurchaseOrderId = 1051,
                Decision = MatchDecision.Rejected,
            },
        ];
        var invalid = new MatchingSettings { SuggestThreshold = 90 };

        var outcomes = TestData.Run(deliveries, orders, earlier);

        Assert.That(outcomes.Select(o => o.State), Is.EqualTo(
        [
            DeliveryMatchState.Suggested,
            DeliveryMatchState.NoCandidate,
            DeliveryMatchState.NoCandidate,
        ]));
        var row = outcomes.SelectMany(o => o.Rows).Single();
        Assert.That(row.PurchaseOrderId, Is.EqualTo(1053));
        Assert.Throws<InvalidOperationException>(() =>
            TestData.Run(deliveries, orders, settings: invalid));
    }
}
