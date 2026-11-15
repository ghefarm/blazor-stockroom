using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class PurchaseOrderServiceTests
{
    [Test]
    public async Task ListsCountsAndCancelsAnOpenOrder()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var service =
            new PurchaseOrderService(database.CreateFactory());

        var orders = await service.ListAsync();
        var openBefore = await service.CountOpenAsync();
        var open = orders.Single(o => o.OrderNumber == "PO-1002");
        var cancelled = await service.CancelAsync(open.Id);
        var openAfter = await service.CountOpenAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(orders.Select(o => o.OrderNumber), Is.EqualTo(
                new[] { "PO-1003", "PO-1002", "PO-1001" }));
            Assert.That(orders[2].SupplierName,
                Is.EqualTo("Brightway Packaging"));
            Assert.That(orders[2].TotalCents, Is.EqualTo(48000));
            Assert.That(openBefore, Is.EqualTo(2));
            Assert.That(cancelled, Is.True);
            Assert.That(openAfter, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task CancelChangesNothingUnlessTheOrderIsOpen()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var service =
            new PurchaseOrderService(database.CreateFactory());
        var orders = await service.ListAsync();
        var open = orders.Single(o => o.OrderNumber == "PO-1002");
        var delivered =
            orders.Single(o => o.OrderNumber == "PO-1001");

        var first = await service.CancelAsync(open.Id);
        var second = await service.CancelAsync(open.Id);
        var done = await service.CancelAsync(delivered.Id);
        var unknown = await service.CancelAsync(999);

        await using var check = database.CreateContext();
        var status = await check.PurchaseOrders
            .Where(o => o.Id == delivered.Id)
            .Select(o => o.Status)
            .SingleAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(done, Is.False);
            Assert.That(unknown, Is.False);
            Assert.That(status, Is.EqualTo(OrderStatus.Delivered));
        }
    }
}
