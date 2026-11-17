using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;

namespace Stockroom.Core.Tests.Data;

public class MigrationTests
{
    // Every migration that has left this machine. A shipped
    // migration that gets a new id runs again on old databases.
    private static readonly string[] Shipped =
    [
        "20261115102754_InitialCreate",
    ];

    [Test]
    public async Task AppliesEveryMigrationToAnEmptyDatabase()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var db = database.CreateContext();

        var all = db.Database.GetMigrations();
        var applied = await db.Database.GetAppliedMigrationsAsync();

        Assert.That(applied, Is.EqualTo(all));
    }

    [Test]
    public void FindsNoChangeWithoutAMigration()
    {
        // No database needed: both checks read the compiled model,
        // the snapshot and the migrations in the assembly.
        var options =
            new DbContextOptionsBuilder<StockroomDbContext>()
                .UseSqlite("Data Source=:memory:")
                .UseApplicationServiceProvider(
                    TestDatabase.IdentityOptionsProvider)
                .Options;
        using var db = new StockroomDbContext(options);

        var all = db.Database.GetMigrations().ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(db.Database.HasPendingModelChanges(),
                Is.False);
            Assert.That(all.Take(Shipped.Length),
                Is.EqualTo(Shipped));
        }
    }
}
