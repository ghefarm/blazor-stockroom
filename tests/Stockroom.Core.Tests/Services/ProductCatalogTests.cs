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
        var catalog = new ProductCatalog(database.CreateFactory());
        var shelf = (await catalog.GetAllAsync())
            .Single(p => p.Sku == "SH-120");

        var found = await catalog.ChangePriceAsync(shelf.Id, 9500);
        var result = await catalog.ChangePricesAsync(
            [new("TP-050", 260), new("BX-L", 150)]);

        await using var check = database.CreateContext();
        var prices = await check.Products
            .Where(p => p.Sku == "SH-120" || p.Sku == "TP-050"
                || p.Sku == "BX-L")
            .OrderBy(p => p.Sku)
            .Select(p => p.PriceCents)
            .ToListAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(found, Is.True);
            Assert.That(result, Is.EqualTo(new BatchResult(2, 0)));
            Assert.That(prices, Is.EqualTo(new[] { 150, 9500, 260 }));
        }
    }

    [Test]
    public async Task LeavesUnknownAndUnchangedRowsAlone()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var catalog = new ProductCatalog(database.CreateFactory());

        var found = await catalog.ChangePriceAsync(999, 100);
        var result = await catalog.ChangePricesAsync(
            [new("NO-SUCH", 100), new("TP-050", 249)]);
        var missing = await catalog.FindByIdAsync(999);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(found, Is.False);
            Assert.That(result, Is.EqualTo(new BatchResult(0, 2)));
            Assert.That(missing, Is.Null);
        }
    }
}
