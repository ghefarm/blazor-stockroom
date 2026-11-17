using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class OrderListQueryTests
{
    [Test]
    public void ReadsWritesAndNamesAFilter()
    {
        var valid = OrderListQuery.TryRead(
            "2", "PartlyDelivered", "2026-10-01", "2026-10-31",
            " tape ", out var filter);
        var written = OrderListQuery.Write(filter);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(valid, Is.True);
            Assert.That(filter, Is.EqualTo(new OrderListFilter(
                2, OrderStatus.PartlyDelivered,
                new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31),
                "tape")));
            Assert.That(written[OrderListQuery.StatusName],
                Is.EqualTo("PartlyDelivered"));
            Assert.That(written[OrderListQuery.FromName],
                Is.EqualTo("2026-10-01"));
            Assert.That(OrderExport.CreateFileName(filter),
                Is.EqualTo("orders_2026-10-01_2026-10-31_tape.csv"));
        }
    }

    [Test]
    public void RejectsValuesThatMakeNoSense()
    {
        var badValues = OrderListQuery.TryRead(
            "two", "7", "2026-13-01", null, null, out var bad);
        var number = OrderListQuery.TryRead(
            null, "7", null, null, null, out _);
        var reversed = OrderListQuery.TryRead(
            null, "all", "2026-11-01", "2026-10-01", null,
            out var noRange);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(badValues, Is.False);
            Assert.That(bad, Is.EqualTo(OrderListFilter.Empty));
            Assert.That(number, Is.False);
            Assert.That(reversed, Is.False);
            Assert.That(noRange, Is.EqualTo(OrderListFilter.Empty));
            Assert.That(OrderExport.CreateFileName(
                new OrderListFilter(Search: "a/b:c")),
                Is.EqualTo("orders_a-b-c.csv"));
        }
    }
}
