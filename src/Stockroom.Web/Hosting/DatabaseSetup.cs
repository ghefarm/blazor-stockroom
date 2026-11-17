using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Auditing;
using Stockroom.Core.Data;
using Stockroom.Core.ReadOnly;

namespace Stockroom.Web.Hosting;

public static class DatabaseSetup
{
    public static void AddStockroomDatabase(
        this WebApplicationBuilder builder)
    {
        var connection = CreateConnectionString(
            builder.Configuration, builder.Environment);
        var withSamples = builder.Environment.IsDevelopment();

        builder.Services.AddSingleton<SubscriptionState>();
        builder.Services.AddSingleton<ReadOnlyInterceptor>();
        builder.Services.AddDbContextFactory<StockroomDbContext>(
            (services, options) =>
            {
                Configure(options, connection, withSamples);

                // Every context from the factory gets both rules,
                // also the scoped one that Identity uses. The
                // read-only rule comes first, so a refused save never
                // reaches the audit code.
                options.AddInterceptors(
                    services.GetRequiredService<
                        ReadOnlyInterceptor>(),
                    services.GetRequiredService<AuditInterceptor>());
            });
    }

    public static async Task MigrateDatabaseAsync(
        this WebApplication app)
    {
        var connection = CreateConnectionString(
            app.Configuration, app.Environment);
        var options =
            new DbContextOptionsBuilder<StockroomDbContext>();
        var withSamples = app.Environment.IsDevelopment();
        Configure(options, connection, withSamples);

        // Not from the factory: migrations and sample data are not
        // user writes, so they run without the read-only rule. The
        // sample rows still get their audit fields, and Identity its
        // options for the schema version.
        options.UseApplicationServiceProvider(app.Services)
            .AddInterceptors(app.Services
                .GetRequiredService<AuditInterceptor>());
        await using var db = new StockroomDbContext(options.Options);

        // SQLite creates the database file, but not its folder.
        var file = new SqliteConnectionStringBuilder(
            db.Database.GetConnectionString()).DataSource;
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        await db.Database.MigrateAsync();
    }

    private static void Configure(
        DbContextOptionsBuilder options,
        string connection,
        bool withSamples)
    {
        options.UseSqlite(connection);
        if (withSamples)
        {
            options.UseSeeding(SampleData.Seed)
                .UseAsyncSeeding(SampleData.SeedAsync);
        }
    }

    private static string CreateConnectionString(
        IConfiguration configuration, IHostEnvironment environment)
    {
        var connection = new SqliteConnectionStringBuilder(
            configuration.GetConnectionString("Stockroom")
            ?? throw new InvalidOperationException(
                "The connection string 'Stockroom' is missing."));

        // SQLite resolves a relative path against the working
        // directory, which changes with the way the app starts.
        connection.DataSource = Path.GetFullPath(
            connection.DataSource, environment.ContentRootPath);
        return connection.ToString();
    }
}
