using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class ProductCatalogTests
{
    [Test]
    public async Task ChangesOnePriceAndABatch()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var catalog = database.CreateCatalog();
        var shelf = (await catalog.GetAllAsync())
            .Single(p => p.Sku == "SH-120");

        var saved = await catalog.ChangePriceAsync(
            shelf.Id, 9500, shelf.Version);
        var result = await catalog.ChangePricesAsync(
            [new("TP-050", 260), new("BX-L", 150)]);
        var slice = await catalog.GetSliceAsync(2, 3);

        await using var check = database.CreateContext();
        var prices = await check.Products
            .Where(p => p.Sku == "SH-120" || p.Sku == "TP-050"
                || p.Sku == "BX-L")
            .OrderBy(p => p.Sku)
            .Select(p => p.PriceCents)
            .ToListAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(saved, Is.EqualTo(SaveOutcome.Saved));
            Assert.That(result, Is.EqualTo(new BatchResult(2, 0)));
            Assert.That(prices, Is.EqualTo(new[] { 150, 9500, 260 }));
            Assert.That(slice.Items.Select(p => p.Sku), Is.EqualTo(
                new[] { "TP-050", "BX-S", "BX-L" }));
            Assert.That(slice.TotalCount, Is.EqualTo(8));
        }
    }

    [Test]
    public async Task LeavesUnknownAndUnchangedRowsAlone()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var catalog = database.CreateCatalog();

        var found = await catalog.ChangePriceAsync(999, 100, 0);
        var result = await catalog.ChangePricesAsync(
            [new("NO-SUCH", 100), new("TP-050", 249)]);
        var missing = await catalog.FindByIdAsync(999);
        var pastTheEnd = await catalog.GetSliceAsync(50, 10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(found, Is.EqualTo(SaveOutcome.NotFound));
            Assert.That(result, Is.EqualTo(new BatchResult(0, 2)));
            Assert.That(missing, Is.Null);
            Assert.That(pastTheEnd.Items, Is.Empty);
            Assert.That(pastTheEnd.TotalCount, Is.EqualTo(8));
        }
    }
}
