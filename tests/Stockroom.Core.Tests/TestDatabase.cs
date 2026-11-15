using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Stockroom.Core.Data;

namespace Stockroom.Core.Tests;

/// <summary>
/// An in-memory SQLite database with the real migrations applied.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;
    private readonly DbContextOptions<StockroomDbContext> options;

    private TestDatabase(SqliteConnection connection)
    {
        this.connection = connection;
        options = new DbContextOptionsBuilder<StockroomDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    public static async Task<TestDatabase> CreateAsync()
    {
        // An in-memory database lives as long as its connection.
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var database = new TestDatabase(connection);
        await using var db = database.CreateContext();

        // The migrations, not EnsureCreated, so the tests see the
        // same schema as a real database.
        await db.Database.MigrateAsync();
        return database;
    }

    public StockroomDbContext CreateContext() => new(options);

    public IDbContextFactory<StockroomDbContext> CreateFactory()
    {
        // A new context on every call, as the real factory does.
        var factory =
            Substitute.For<IDbContextFactory<StockroomDbContext>>();
        factory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => CreateContext());
        return factory;
    }

    public async Task SeedAsync()
    {
        await using var db = CreateContext();
        await SampleData.SeedAsync(db, true, default);
    }

    public ValueTask DisposeAsync() => connection.DisposeAsync();
}
