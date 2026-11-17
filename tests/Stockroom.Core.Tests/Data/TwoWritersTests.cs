using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Tests.Data;

public class TwoWritersTests
{
    private string folder = "";

    [SetUp]
    public void CreateFolder()
    {
        folder = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            "two-writers",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
    }

    [TearDown]
    public void DeleteFolder()
    {
        // The pool keeps the file open, and Windows cannot delete
        // a folder with an open file in it.
        SqliteConnection.ClearAllPools();
        Directory.Delete(folder, recursive: true);
    }

    [Test]
    public async Task SecondWriterWaitsForTheFirst()
    {
        await using (var setup = CreateContext())
        {
            await setup.Database.MigrateAsync();
        }

        var mode = await ReadJournalModeAsync();

        await using var first = CreateContext();
        await using var transaction =
            await first.Database.BeginTransactionAsync();
        first.Suppliers.Add(CreateSupplier("North"));
        await first.SaveChangesAsync();

        await using var reader = CreateContext();
        var seen = await reader.Suppliers.CountAsync();

        // A writer that gives up after one second cannot get in,
        // however slow or fast the machine is.
        await using var hasty = CreateContext(timeoutSeconds: 1);
        hasty.Suppliers.Add(CreateSupplier("East"));
        var refused = Assert.ThrowsAsync<DbUpdateException>(
            () => hasty.SaveChangesAsync());
        var busy = refused?.InnerException as SqliteException;

        var second = Task.Run(async () =>
        {
            await using var db = CreateContext();
            db.Suppliers.Add(CreateSupplier("South"));
            await db.SaveChangesAsync();
        });
        await transaction.CommitAsync();
        await second;

        var names = await ReadNamesAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mode, Is.EqualTo("wal"));
            Assert.That(seen, Is.Zero);
            // 5 is SQLITE_BUSY: the file is locked by a writer.
            Assert.That(busy?.SqliteErrorCode, Is.EqualTo(5));
            Assert.That(names,
                Is.EqualTo(new[] { "North", "South" }));
        }
    }

    private StockroomDbContext CreateContext(int timeoutSeconds = 30)
    {
        var connection = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(folder, "stockroom.db"),
            DefaultTimeout = timeoutSeconds,
        };
        var options =
            new DbContextOptionsBuilder<StockroomDbContext>()
                .UseSqlite(connection.ToString())
                .UseApplicationServiceProvider(
                    TestDatabase.IdentityOptionsProvider)
                .Options;
        return new StockroomDbContext(options);
    }

    private async Task<string?> ReadJournalModeAsync()
    {
        await using var db = CreateContext();
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode;";
        return (string?)await command.ExecuteScalarAsync();
    }

    private async Task<List<string>> ReadNamesAsync()
    {
        await using var db = CreateContext();
        return await db.Suppliers
            .OrderBy(s => s.Name)
            .Select(s => s.Name)
            .ToListAsync();
    }

    private static Supplier CreateSupplier(string name) => new()
    {
        Name = name,
        Email = $"{name.ToLowerInvariant()}@example.test",
    };
}
