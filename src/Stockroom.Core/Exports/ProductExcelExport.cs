using ClosedXML.Excel;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Exports;

public static class ProductExcelExport
{
    public static byte[] Create(
        IReadOnlyList<Product> products, Uri siteAddress)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Products");
        string[] headers = ["SKU", "Name", "Price", "Stock"];
        for (var c = 0; c < headers.Length; c++)
        {
            sheet.Cell(1, c + 1).Value = headers[c];
        }

        var row = 2;
        foreach (var p in products)
        {
            // The file is opened outside the browser, so the link
            // needs the full address with the host name.
            var link = new Uri(siteAddress, $"products/{p.Id}");
            sheet.Cell(row, 1).Value = p.Sku;
            sheet.Cell(row, 1).SetHyperlink(new XLHyperlink(link));
            sheet.Cell(row, 2).Value = p.Name;
            sheet.Cell(row, 3).Value = p.PriceCents / 100m;
            sheet.Cell(row, 4).Value = p.StockQuantity;
            row++;
        }

        FormatSheet(sheet, row - 1);
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void FormatSheet(IXLWorksheet sheet, int lastRow)
    {
        sheet.Row(1).Style.Font.Bold = true;
        if (lastRow >= 2)
        {
            sheet.Range(2, 3, lastRow, 3).Style.NumberFormat.Format =
                "#,##0.00";
        }

        sheet.SheetView.FreezeRows(1);
        sheet.Columns().AdjustToContents();
    }
}
