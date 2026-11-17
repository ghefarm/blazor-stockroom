using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Stockroom.Core.Auditing;
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
    internal static readonly IServiceProvider
        IdentityOptionsProvider = new ServiceCollection()
            .Configure<IdentityOptions>(AccountRules.Apply)
            .BuildServiceProvider();

    // Substitutes, so a test can choose the user and the time.
    public ICurrentUser User { get; } =
        Substitute.For<ICurrentUser>();
    public TimeProvider Time { get; } =
        Substitute.For<TimeProvider>();

    private TestDatabase(
        SqliteConnection connection, IInterceptor[] interceptors)
    {
        this.connection = connection;
        options = new DbContextOptionsBuilder<StockroomDbContext>()
            .UseSqlite(connection)
            .UseApplicationServiceProvider(IdentityOptionsProvider)
            .AddInterceptors(new AuditInterceptor(User, Time))
            .AddInterceptors(interceptors)
            .Options;
    }

    public static async Task<TestDatabase> CreateAsync(
        params IInterceptor[] interceptors)
    {
        // An in-memory database lives as long as its connection.
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var database = new TestDatabase(connection, interceptors);
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
        factory.CreateDbContext().Returns(_ => CreateContext());
        return factory;
    }

    public ProductCatalog CreateCatalog()
    {
        var factory = CreateFactory();
        return new ProductCatalog(factory, CreateSummaries(factory));
    }

    /// <summary>
    /// The summaries with a real HybridCache in memory, because every
    /// service that writes products clears them.
    /// </summary>
    public static ProductSummaryService CreateSummaries(
        IDbContextFactory<StockroomDbContext> factory)
    {
        var cache = new ServiceCollection()
            .AddHybridCache().Services
            .BuildServiceProvider()
            .GetRequiredService<HybridCache>();
        return new ProductSummaryService(factory, cache);
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
