using Microsoft.EntityFrameworkCore;

namespace Stockroom.Core.Tests.Data;

public class ContextLifetimeTests
{
    [Test]
    public async Task LongLivedContext_KeepsShowingOldValues()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();

        // Lives as long as a circuit would.
        await using var page = database.CreateContext();
        var before = await page.Products
            .SingleAsync(p => p.Sku == "PJ-200");

        // Another user saves a new price with a context of their own.
        await using (var other = database.CreateContext())
        {
            var jack = await other.Products
                .SingleAsync(p => p.Sku == "PJ-200");
            jack.PriceCents = 39900;
            await other.SaveChangesAsync();
        }

        var reloaded = await page.Products
            .SingleAsync(p => p.Sku == "PJ-200");
        var fresh = await page.Products.AsNoTracking()
            .SingleAsync(p => p.Sku == "PJ-200");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reloaded, Is.SameAs(before));
            Assert.That(reloaded.PriceCents, Is.EqualTo(34900));
            Assert.That(fresh.PriceCents, Is.EqualTo(39900));
        }
    }
}
