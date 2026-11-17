using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class OrderQueryTests
{
    [Test]
    public async Task LoadsRowsAndRelatedDataInAFixedNumberOfQueries()
    {
        var counter = new CommandCounter();
        await using var database =
            await TestDatabase.CreateAsync(counter);
        await database.SeedAsync();
        await AddOrdersAsync(database, 20);
        var service =
            new PurchaseOrderService(database.CreateFactory());

        counter.Clear();
        var rows = await service.ListWithProductsAsync();
        var listQueries = counter.Commands.Count;

        var first = rows.Single(
            r => r.Order.OrderNumber == "PO-1001");
        var supplierId = await FindSupplierIdAsync(
            database, "Brightway Packaging");
        counter.Clear();
        var overview =
            await service.GetSupplierOverviewAsync(supplierId);
        var overviewQueries = counter.Commands.Count;
        var kestrel = await FindSupplierIdAsync(
            database, "Kestrel Lifting Gear");
        var hasOpen = await service.HasOpenOrdersAsync(kestrel);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rows, Has.Count.EqualTo(23));
            Assert.That(listQueries, Is.EqualTo(2));
            Assert.That(first.ProductNames, Is.EqualTo(new[]
            {
                "Cardboard box, small", "Packing tape 50 m",
            }));
            Assert.That(overviewQueries, Is.EqualTo(3));
            Assert.That(overview!.OrderNumbers,
                Has.Count.EqualTo(21));
            Assert.That(overview.ProductNames, Is.EqualTo(new[]
            {
                "Cardboard box, small", "Packing tape 50 m",
                "Stretch film 500 mm",
            }));
            Assert.That(hasOpen, Is.True);
        }
    }

    [Test]
    public async Task FindsNothingWhereNothingMatches()
    {
        await using var database = await TestDatabase.CreateAsync();
        var service =
            new PurchaseOrderService(database.CreateFactory());
        var empty = await service.ListWithProductsAsync();

        await database.SeedAsync();
        var brightway = await FindSupplierIdAsync(
            database, "Brightway Packaging");
        var onlyDelivered =
            await service.HasOpenOrdersAsync(brightway);
        var unknown = await service.HasOpenOrdersAsync(999);
        var missing = await service.GetSupplierOverviewAsync(999);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(empty, Is.Empty);
            Assert.That(onlyDelivered, Is.False);
            Assert.That(unknown, Is.False);
            Assert.That(missing, Is.Null);
        }
    }

    private static async Task<int> FindSupplierIdAsync(
        TestDatabase database, string name)
    {
        await using var db = database.CreateContext();
        return await db.Suppliers
            .Where(s => s.Name == name)
            .Select(s => s.Id)
            .SingleAsync();
    }

    private static async Task AddOrdersAsync(
        TestDatabase database, int count)
    {
        var supplierId = await FindSupplierIdAsync(
            database, "Brightway Packaging");
        await using var db = database.CreateContext();
        var filmId = await db.Products
            .Where(p => p.Sku == "SF-500")
            .Select(p => p.Id)
            .SingleAsync();
        for (var i = 1; i <= count; i++)
        {
            db.PurchaseOrders.Add(new PurchaseOrder
            {
                OrderNumber = $"PO-{2000 + i}",
                SupplierId = supplierId,
                OrderedOn = new DateOnly(2026, 9, 1),
                Status = OrderStatus.Delivered,
                Lines = [new() { ProductId = filmId, Quantity = i,
                    UnitPriceCents = 1100 }],
            });
        }

        await db.SaveChangesAsync();
    }
}
