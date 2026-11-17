using System.Text;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Stockroom.Core.Data;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class PriceListImporterTests
{
    private string folder = "";

    [SetUp]
    public void CreateFolder()
    {
        folder = Path.Combine(
            Path.GetTempPath(), "stockroom-" + Guid.NewGuid());
    }

    [TearDown]
    public void DeleteFolder()
    {
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Test]
    public async Task ImportsArchivesAndRemembersTheFile()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var importer = CreateImporter(database.CreateFactory());

        var result = await importer.ImportAsync(@"C:\in\list.csv",
            Csv("SKU;Price\nTP-050;2,59\nBX-S;0,85\nNO-1;1,00\n"));
        var again = await importer.ImportAsync("list.csv",
            Csv("SKU;Price\nTP-050;2,69\n"));

        await using var check = database.CreateContext();
        var imports = await check.PriceListImports
            .OrderBy(i => i.Id).ToListAsync();
        var tape = await check.Products.SingleAsync(
            p => p.Sku == "TP-050");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Prices,
                Is.EqualTo(new BatchResult(1, 2)));
            Assert.That(again.Succeeded, Is.True);
            Assert.That(tape.PriceCents, Is.EqualTo(269));
            Assert.That(imports.Select(i => i.FileName),
                Is.EqualTo(new[] { "list.csv", "list (2).csv" }));
            Assert.That(imports[0].FileHash, Has.Length.EqualTo(64));
            Assert.That(imports[0].ImportedAt, Is.EqualTo(
                new DateTime(2026, 7, 15, 12, 0, 0)));
            Assert.That(imports[0].RowCount, Is.EqualTo(3));
            Assert.That(
                File.Exists(Path.Combine(folder, "list.csv")));
        }
    }

    [Test]
    public async Task RefusesBadFilesAndLeavesNoTrace()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var importer = CreateImporter(database.CreateFactory());
        const string good = "SKU;Price\nTP-050;2,59\n";

        var wrongType = await importer.ImportAsync("list.txt",
            Csv(good));
        var huge = new byte[PriceListImporter.MaxBytes + 1];
        var tooLarge = await importer.ImportAsync("big.csv",
            new MemoryStream(huge));
        var badRow = await importer.ImportAsync("bad.csv",
            Csv("SKU;Price\nTP-050;two\n"));
        await importer.ImportAsync("list.csv", Csv(good));
        var twice = await importer.ImportAsync("copy.csv", Csv(good));

        // A file where the folder should be: nothing can be written.
        var blocked = folder + ".blocked";
        await File.WriteAllTextAsync(blocked, "");
        var noArchive = await new PriceListImporter(
            database.CreateFactory(), new ImportArchive(blocked),
            CreateClock()).ImportAsync("new.csv", Csv(good));
        File.Delete(blocked);

        // The save fails after the file was archived.
        var failing = Substitute.For<
            IDbContextFactory<StockroomDbContext>>();
        failing.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => database.CreateContext(),
                _ => throw new InvalidOperationException("disk"));
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateImporter(failing).ImportAsync(
                "new.csv", Csv("SKU;Price\nBX-S;1\n")));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(noArchive.Errors,
                Has.One.Contains("not writable"));
            Assert.That(wrongType.Errors, Has.One.Contains(".csv"));
            Assert.That(tooLarge.Errors, Has.One.Contains("2 MB"));
            Assert.That(badRow.Errors, Has.One.Contains("\"two\""));
            Assert.That(twice.Errors, Is.EqualTo(new[]
                { "This file was imported before as list.csv." }));
            Assert.That(Directory.GetFiles(folder).Select(
                Path.GetFileName), Is.EqualTo(new[] { "list.csv" }));
        }
    }

    private static MemoryStream Csv(string text) =>
        new(Encoding.Latin1.GetBytes(text));

    private PriceListImporter CreateImporter(
        IDbContextFactory<StockroomDbContext> factory) =>
        new(factory, new ImportArchive(folder), CreateClock());

    private static BusinessClock CreateClock()
    {
        // Mid-year, as in chapter 15.
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(
            new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero));
        return new BusinessClock(time, TimeZoneInfo.Utc);
    }
}
