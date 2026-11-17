using System.IO.Compression;
using ClosedXML.Excel;
using Stockroom.Core.Entities;
using Stockroom.Core.Files;

namespace Stockroom.Core.Exports;

/// <summary>
/// The archived price lists plus an overview sheet that links to
/// them, for a reader without access to Stockroom.
/// </summary>
public static class ImportZipExport
{
    private sealed record Row(
        PriceListImport Import, string Source, string? Entry);

    public static void Write(
        Stream target,
        IReadOnlyList<PriceListImport> imports,
        string archiveFolder,
        TimeZoneInfo zone)
    {
        HashSet<string> used = new(StringComparer.OrdinalIgnoreCase);
        List<Row> rows = [];
        foreach (var import in imports)
        {
            var source = Path.Combine(archiveFolder, import.FileName);
            var exists = SafePath.IsInside(archiveFolder, source)
                && File.Exists(source);
            var entry = exists
                ? "files/" + ChooseName(import.FileName, used)
                : null;
            rows.Add(new Row(import, source, entry));
        }

        using var zip = new ZipArchive(
            target, ZipArchiveMode.Create, leaveOpen: true);
        AddOverview(zip, rows, zone);
        foreach (var row in rows.Where(r => r.Entry is not null))
        {
            // Plain text shrinks well.
            zip.CreateEntryFromFile(
                row.Source, row.Entry!, CompressionLevel.Optimal);
        }
    }

    private static string ChooseName(
        string fileName, HashSet<string> used)
    {
        // In a link, # starts a fragment and % starts an escape.
        var name = fileName.Replace('#', '_').Replace('%', '_');
        var stem = Path.GetFileNameWithoutExtension(name);
        var extension = Path.GetExtension(name);
        for (var n = 2; !used.Add(name); n++)
        {
            name = $"{stem} ({n}){extension}";
        }

        return name;
    }

    private static void AddOverview(
        ZipArchive zip, List<Row> rows, TimeZoneInfo zone)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Price lists");
        sheet.Cell(1, 1).Value =
            "Unpack the ZIP file first. Inside it, no link works.";
        sheet.Cell(2, 1).Value = "Imported";
        sheet.Cell(2, 2).Value = "File";
        sheet.Cell(2, 3).Value = "Rows";
        sheet.Row(2).Style.Font.Bold = true;

        var r = 3;
        foreach (var row in rows)
        {
            // Stored as UTC, shown in the time of the business.
            sheet.Cell(r, 1).Value = TimeZoneInfo.ConvertTimeFromUtc(
                row.Import.ImportedAt, zone);
            sheet.Cell(r, 3).Value = row.Import.RowCount;
            AddFileCell(sheet.Cell(r, 2), row);
            r++;
        }

        sheet.Column(1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
        sheet.Columns(1, 3).AdjustToContents(2, r);

        // An .xlsx file is a ZIP already: compressing it again
        // costs time and saves nothing.
        var entry = zip.CreateEntry(
            "price-lists.xlsx", CompressionLevel.NoCompression);
        using var stream = entry.Open();
        workbook.SaveAs(stream);
    }

    private static void AddFileCell(IXLCell cell, Row row)
    {
        if (row.Entry is null)
        {
            // A note, never a link that leads nowhere.
            cell.Value = $"{row.Import.FileName} (file missing)";
            return;
        }

        cell.Value = row.Import.FileName;
        cell.SetHyperlink(
            new XLHyperlink(new Uri(row.Entry, UriKind.Relative)));
    }
}
