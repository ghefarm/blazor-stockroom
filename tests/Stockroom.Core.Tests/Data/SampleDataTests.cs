using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;

namespace Stockroom.Core.Tests.Data;

public class SampleDataTests
{
    [Test]
    public async Task SeedAsync_FillsAnEmptyDatabase()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using (var db = database.CreateContext())
        {
            await SampleData.SeedAsync(db, true, default);
        }

        await using var check = database.CreateContext();
        var inactive = await check.Products
            .Where(p => !p.IsActive)
            .Select(p => p.Sku)
            .ToListAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(await check.Products.CountAsync(),
                Is.EqualTo(8));
            Assert.That(await check.Suppliers.CountAsync(),
                Is.EqualTo(4));
            Assert.That(inactive, Is.EqualTo(new[] { "LP-01" }));
        }
    }

    [Test]
    public async Task SeedAsync_NeverAddsTwiceOrOverwrites()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using (var db = database.CreateContext())
        {
            await SampleData.SeedAsync(db, true, default);
            var shelf = await db.Products
                .SingleAsync(p => p.Sku == "SH-120");
            shelf.PriceCents = 9900;
            await db.SaveChangesAsync();
        }

        await using (var db = database.CreateContext())
        {
            await SampleData.SeedAsync(db, false, default);
        }

        await using var check = database.CreateContext();
        var price = await check.Products
            .Where(p => p.Sku == "SH-120")
            .Select(p => p.PriceCents)
            .SingleAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(await check.Products.CountAsync(),
                Is.EqualTo(8));
            Assert.That(await check.Suppliers.CountAsync(),
                Is.EqualTo(4));
            Assert.That(price, Is.EqualTo(9900));
        }
    }
}
