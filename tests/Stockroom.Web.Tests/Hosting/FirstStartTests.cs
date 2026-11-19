using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stockroom.Core.Data;

namespace Stockroom.Web.Tests.Hosting;

public class FirstStartTests
{
    private string root = "";

    [SetUp]
    public void PrepareEmptyDataRoot()
    {
        root = Path.Combine(AppContext.BaseDirectory,
            "first-start-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "settings.json"),
            """{ "Stockroom": { "Probe": "settings file" } }""");

        Environment.SetEnvironmentVariable(
            "Stockroom__DataRoot", root);
        Environment.SetEnvironmentVariable(
            "Stockroom__Probe", "environment");
    }

    [TearDown]
    public void DeleteDataRoot()
    {
        Environment.SetEnvironmentVariable(
            "Stockroom__DataRoot", null);
        Environment.SetEnvironmentVariable(
            "Stockroom__Probe", null);
        SqliteConnection.ClearAllPools();
        Directory.Delete(root, recursive: true);
    }

    [Test]
    public async Task FirstStart_CreatesFolders_Migrates_IsHealthy()
    {
        HttpStatusCode status;
        string? probe;
        long migrations;
        await using (var app = new WebApplicationFactory<Program>())
        {
            using var client = app.CreateClient();
            var response = await client.GetAsync("/health");
            status = response.StatusCode;
            probe = app.Services.GetRequiredService<IConfiguration>()
                ["Stockroom:Probe"];

            // Every chapter with a schema change adds one.
            var factory = app.Services.GetRequiredService<
                IDbContextFactory<StockroomDbContext>>();
            await using var context = factory.CreateDbContext();
            migrations = context.Database.GetMigrations().Count();
        }

        var file = Path.Combine(root, "db", "stockroom.db");
        await using var db =
            new SqliteConnection($"Data Source={file}");
        await db.OpenAsync();
        await using var query = db.CreateCommand();
        query.CommandText =
            "SELECT COUNT(*) FROM __EFMigrationsHistory";
        var applied = await query.ExecuteScalarAsync();

        Assert.Multiple(() =>
        {
            Assert.That(status, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(applied, Is.EqualTo(migrations));
            Assert.That(probe, Is.EqualTo("settings file"));
            Assert.That(Directory.Exists(Path.Combine(root, "logs")));
        });
    }
}
