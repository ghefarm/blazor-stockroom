using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Auditing;
using Stockroom.Core.Data;

namespace Stockroom.Web.Hosting;

public static class DatabaseSetup
{
    public static void AddStockroomDatabase(
        this WebApplicationBuilder builder)
    {
        var connection = new SqliteConnectionStringBuilder(
            builder.Configuration.GetConnectionString("Stockroom")
            ?? throw new InvalidOperationException(
                "The connection string 'Stockroom' is missing."));

        // SQLite resolves a relative path against the working
        // directory, which changes with the way the app starts.
        connection.DataSource = Path.GetFullPath(
            connection.DataSource,
            builder.Environment.ContentRootPath);

        var withSamples = builder.Environment.IsDevelopment();
        builder.Services.AddDbContextFactory<StockroomDbContext>(
            (services, options) =>
            {
                options.UseSqlite(connection.ToString())
                    .AddInterceptors(services
                        .GetRequiredService<AuditInterceptor>());
                if (withSamples)
                {
                    options.UseSeeding(SampleData.Seed)
                        .UseAsyncSeeding(SampleData.SeedAsync);
                }
            });
    }

    public static async Task MigrateDatabaseAsync(
        this WebApplication app)
    {
        var factory = app.Services.GetRequiredService<
            IDbContextFactory<StockroomDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        // SQLite creates the database file, but not its folder.
        var file = new SqliteConnectionStringBuilder(
            db.Database.GetConnectionString()).DataSource;
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);

        await db.Database.MigrateAsync();
    }
}
