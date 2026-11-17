using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests;

/// <summary>
/// An in-memory SQLite database with the real migrations applied.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection connection;
    private readonly DbContextOptions<StockroomDbContext> options;

    // The Identity tables depend on the schema version in the
    // Identity options. Without them, the model has version 1 and
    // no longer matches the migrations.
    private static readonly IServiceProvider IdentityOptionsProvider =
        new ServiceCollection()
            .Configure<IdentityOptions>(AccountRules.Apply)
            .BuildServiceProvider();

    private TestDatabase(SqliteConnection connection)
    {
        this.connection = connection;
        options = new DbContextOptionsBuilder<StockroomDbContext>()
            .UseSqlite(connection)
            .UseApplicationServiceProvider(IdentityOptionsProvider)
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

    /// <summary>
    /// Identity as the app registers it, on this database. Dispose
    /// the scope at the end of the test.
    /// </summary>
    public AsyncServiceScope CreateIdentityScope()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddDataProtection().Services
            .AddScoped(_ => CreateContext())
            .AddSingleton(CreateFactory());
        services.AddIdentityCore<ApplicationUser>(AccountRules.Apply)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<StockroomDbContext>()
            .AddErrorDescriber<StockroomErrorDescriber>()
            .AddDefaultTokenProviders();
        services.AddScoped<UserAccountService>();
        services.AddScoped<AccountSetup>();
        return services.BuildServiceProvider().CreateAsyncScope();
    }

    public async Task SeedAsync()
    {
        await using var db = CreateContext();
        await SampleData.SeedAsync(db, true, default);
    }

    public ValueTask DisposeAsync() => connection.DisposeAsync();
}
