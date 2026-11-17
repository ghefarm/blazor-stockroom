using System.IO.Compression;
using System.Text;
using ClosedXML.Excel;
using PdfSharp.Fonts;
using Stockroom.Core.Entities;
using Stockroom.Core.Exports;

namespace Stockroom.Core.Tests.Exports;

public class ExportTests
{
    [OneTimeSetUp]
    public void UseWindowsFonts()
    {
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
    }

    [Test]
    public void Excel_KeepsTextAsTextAndLinksAbsolute()
    {
        Product evil = new()
        {
            Id = 7, Sku = "EV-1", Name = "=1+1", PriceCents = 1250,
        };
        var site = new Uri("https://stock.example/");

        var bytes = ProductExcelExport.Create([evil], site);
        using var workbook = new XLWorkbook(new MemoryStream(bytes));
        var sheet = workbook.Worksheet(1);
        var link = sheet.Cell(2, 1).GetHyperlink().ExternalAddress;
        var pdf = ProductPdfExport.Create(
            [evil], new DateOnly(2026, 7, 15));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sheet.Cell(2, 2).HasFormula, Is.False);
            Assert.That(sheet.Cell(2, 2).GetText(),
                Is.EqualTo("=1+1"));
            Assert.That(sheet.Cell(2, 3).GetValue<decimal>(),
                Is.EqualTo(12.5m));
            Assert.That(link.AbsoluteUri,
                Is.EqualTo("https://stock.example/products/7"));
            Assert.That(Encoding.ASCII.GetString(pdf, 0, 5),
                Is.EqualTo("%PDF-"));
        }
    }

    [Test]
    public void Zip_RenamesCollisionsAndMarksMissingFiles()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(folder, "a#1.csv"), "SKU");
        File.WriteAllText(Path.Combine(folder, "a%1.csv"), "SKU");
        PriceListImport[] imports =
        [
            new() { FileName = "a#1.csv" },
            new() { FileName = "a%1.csv" },
            new() { FileName = "gone.csv" },
        ];

        using var stream = new MemoryStream();
        ImportZipExport.Write(
            stream, imports, folder, TimeZoneInfo.Utc);
        Directory.Delete(folder, recursive: true);
        using var zip = new ZipArchive(stream);
        using var workbook = new XLWorkbook(
            zip.GetEntry("price-lists.xlsx")!.Open().CopyToMemory());
        var sheet = workbook.Worksheet(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(zip.Entries.Select(e => e.FullName),
                Is.EqualTo(new[] { "price-lists.xlsx",
                    "files/a_1.csv", "files/a_1 (2).csv" }));
            Assert.That(zip.Entries[0].CompressedLength,
                Is.EqualTo(zip.Entries[0].Length));
            var link = sheet.Cell(4, 2).GetHyperlink();
            Assert.That(link.ExternalAddress.OriginalString,
                Is.EqualTo("files/a_1 (2).csv"));
            Assert.That(sheet.Cell(5, 2).HasHyperlink, Is.False);
            Assert.That(sheet.Cell(5, 2).GetText(),
                Is.EqualTo("gone.csv (file missing)"));
        }
    }
}

internal static class StreamExtensions
{
    public static MemoryStream CopyToMemory(this Stream stream)
    {
        var memory = new MemoryStream();
        stream.CopyTo(memory);
        memory.Position = 0;
        return memory;
    }
}
