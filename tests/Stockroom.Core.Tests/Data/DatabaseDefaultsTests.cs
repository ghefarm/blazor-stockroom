using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Tests.Data;

public class DatabaseDefaultsTests
{
    [Test]
    public async Task Product_KeepsTheFlagItWasSavedWith()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using (var db = database.CreateContext())
        {
            db.Products.Add(new Product { Sku = "A-1", Name = "On" });
            db.Products.Add(new Product
            {
                Sku = "A-2", Name = "Off", IsActive = false,
            });
            await db.SaveChangesAsync();
        }

        await using var check = database.CreateContext();
        var flags = await check.Products
            .OrderBy(p => p.Sku)
            .Select(p => p.IsActive)
            .ToListAsync();

        Assert.That(flags, Is.EqualTo(new[] { true, false }));
    }

    [Test]
    public async Task IntFlag_WithDefaultOne_NeedsASentinel()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<FlagContext>()
            .UseSqlite(connection)
            .Options;

        await using (var db = new FlagContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Shelves.Add(new Shelf { Active = 0, Visible = 0 });
            db.Shelves.Add(new Shelf());
            await db.SaveChangesAsync();
        }

        await using var check = new FlagContext(options);
        var rows = await check.Shelves.OrderBy(s => s.Id)
            .ToListAsync();

        using (Assert.EnterMultipleScope())
        {
            // The 0 was never sent, so the database default won.
            Assert.That(rows[0].Active, Is.EqualTo(1));
            Assert.That(rows[0].Visible, Is.EqualTo(0));
            Assert.That(rows[1].Visible, Is.EqualTo(1));
        }
    }
}
