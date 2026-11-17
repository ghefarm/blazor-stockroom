using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Tests.Data;

/// <summary>
/// Two writers on one database file. Only a file gives each
/// connection its own lock, so these tests cannot run in memory.
/// </summary>
public class SingleWriterTests
{
    private string folder = "";

    [SetUp]
    public void CreateFolder()
    {
        folder = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
    }

    [TearDown]
    public void DeleteFolder()
    {
        // The pool keeps the file open, and Windows cannot delete
        // an open file.
        SqliteConnection.ClearAllPools();
        Directory.Delete(folder, recursive: true);
    }

    [Test]
    public async Task SecondWriterWaitsAndThenSaves()
    {
        var options = await CreateDatabaseAsync("");
        await using var first = new StockroomDbContext(options);
        await using var second = new StockroomDbContext(options);
        await using var reader = new StockroomDbContext(options);
        var mode = await reader.Database
            .SqlQueryRaw<string>("PRAGMA journal_mode")
            .ToListAsync();

        await using var transaction =
            await first.Database.BeginTransactionAsync();
        first.Products.Add(new() { Sku = "W-1", Name = "First" });
        await first.SaveChangesAsync();

        var seenWhileOpen = await reader.Products.CountAsync();
        second.Products.Add(new() { Sku = "W-2", Name = "Second" });
        var save = Task.Run(() => second.SaveChangesAsync());

        // The weak point: a fixed delay stands for "still waiting".
        await Task.Delay(500);
        var waited = !save.IsCompleted;
        await transaction.CommitAsync();
        await save;
        var count = await reader.Products.CountAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mode, Is.EqualTo(new[] { "wal" }));
            Assert.That(seenWhileOpen, Is.Zero);
            Assert.That(waited, Is.True);
            Assert.That(count, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task SecondWriterGivesUpAfterTheTimeout()
    {
        var options = await CreateDatabaseAsync(";Default Timeout=1");
        await using var first = new StockroomDbContext(options);
        await using var second = new StockroomDbContext(options);

        await using var transaction =
            await first.Database.BeginTransactionAsync();
        first.Products.Add(new() { Sku = "W-1", Name = "First" });
        await first.SaveChangesAsync();

        second.Products.Add(new() { Sku = "W-2", Name = "Second" });
        var error = Assert.ThrowsAsync<DbUpdateException>(
            () => second.SaveChangesAsync());
        await transaction.CommitAsync();

        var inner = error!.InnerException as SqliteException;
        Assert.That(inner?.SqliteErrorCode, Is.EqualTo(5));
    }

    private async Task<DbContextOptions<StockroomDbContext>>
        CreateDatabaseAsync(string extra)
    {
        var file = Path.Combine(folder, "writers.db");
        var options =
            new DbContextOptionsBuilder<StockroomDbContext>()
                .UseSqlite($"Data Source={file}{extra}")
                .UseApplicationServiceProvider(
                    TestDatabase.IdentityOptionsProvider)
                .Options;

        // EF Core switches a database it creates to WAL mode.
        await using var db = new StockroomDbContext(options);
        await db.Database.MigrateAsync();
        return options;
    }
}
