using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class OrderListServiceTests
{
    [Test]
    public async Task PagesFiltersAndExportsTheSameRows()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        await AddOrdersAsync(database, 22);
        var service = new OrderListService(database.CreateFactory());

        var third =
            await service.LoadAsync(OrderListFilter.Empty, 3, 10);
        var gloves = await service.LoadAsync(
            new OrderListFilter(Search: " GLOVES "), 1, 10);
        var open = new OrderListFilter(Status: OrderStatus.Open);
        var table = await service.LoadAsync(open, 1, 50);
        var export = await service.ExportAsync(open);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(third.TotalCount, Is.EqualTo(25));
            Assert.That(third.PageCount, Is.EqualTo(3));
            Assert.That(third.Rows, Has.Count.EqualTo(5));
            Assert.That(third.FirstRow, Is.EqualTo(21));
            Assert.That(third.FindPageFor(25), Is.EqualTo(1));
            Assert.That(third.StatusCounts[OrderStatus.Open],
                Is.EqualTo(23));
            Assert.That(gloves.Filter.Search, Is.EqualTo("GLOVES"));
            Assert.That(gloves.Rows.Select(r => r.OrderNumber),
                Is.EqualTo(new[] { "PO-1003" }));
            Assert.That(table.TotalCount, Is.EqualTo(23));
            Assert.That(export, Is.EqualTo(table.Rows));
        }
    }

    [Test]
    public async Task AppliesOnlyWhatTheListCanShow()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var service = new OrderListService(database.CreateFactory());

        var unknown = await service.LoadAsync(
            new OrderListFilter(SupplierId: 999, Search: "   "),
            99, 7);
        var nothing = await service.LoadAsync(
            new OrderListFilter(Search: "nothing"), 0, 10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unknown.Filter,
                Is.EqualTo(OrderListFilter.Empty));
            Assert.That(unknown.TotalCount, Is.EqualTo(3));
            Assert.That(unknown.Page, Is.EqualTo(1));
            Assert.That(unknown.PageSize, Is.EqualTo(10));
            Assert.That(nothing.Rows, Is.Empty);
            Assert.That(nothing.Page, Is.EqualTo(1));
            Assert.That(nothing.FirstRow, Is.EqualTo(0));
        }
    }

    // More orders than one page holds, all open, one supplier.
    private static async Task AddOrdersAsync(
        TestDatabase database, int count)
    {
        await using var db = database.CreateContext();
        var supplierId = await db.Suppliers
            .Where(s => s.Name == "Brightway Packaging")
            .Select(s => s.Id)
            .SingleAsync();
        var start = new DateOnly(2026, 9, 1);
        for (var i = 1; i <= count; i++)
        {
            db.PurchaseOrders.Add(new PurchaseOrder
            {
                OrderNumber = $"PO-{2000 + i}",
                SupplierId = supplierId,
                OrderedOn = start.AddDays(i),
                Status = OrderStatus.Open,
            });
        }

        await db.SaveChangesAsync();
    }
}
