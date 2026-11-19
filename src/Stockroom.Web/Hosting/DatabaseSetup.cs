using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Auditing;
using Stockroom.Core.Data;
using Stockroom.Core.ReadOnly;
using Stockroom.Core.Storage;

namespace Stockroom.Web.Hosting;

public static class DatabaseSetup
{
    public static void AddStockroomDatabase(
        this WebApplicationBuilder builder, DataFolders folders)
    {
        var connection = CreateConnectionString(folders);
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
        var folders = app.Services.GetRequiredService<DataFolders>();
        var clock = app.Services.GetRequiredService<TimeProvider>();
        var connection = CreateConnectionString(folders);
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
        var pending = await db.Database.GetPendingMigrationsAsync();

        // An old version may not work with a migrated database,
        // so a rollback needs this copy.
        if (pending.Any() && File.Exists(folders.DatabaseFile))
        {
            var copy = SqliteBackup.CreateBackup(folders.DatabaseFile,
                folders.Backups, clock.GetLocalNow());
            app.Logger.LogInformation(
                "Backup before migration: {File}", copy);
        }

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

    // Always an absolute path. SQLite would resolve a relative one
    // against the working directory.
    private static string CreateConnectionString(
        DataFolders folders) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = folders.DatabaseFile
        }.ToString();
}
