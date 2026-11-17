using System.Text;
using Stockroom.Core.Entities;
using Stockroom.Core.Files;

namespace Stockroom.Core.Exports;

public static class ProductCsvExport
{
    private const char Separator = ',';

    public static byte[] Create(IEnumerable<Product> products)
    {
        var csv = new StringBuilder();
        AppendRow(csv, ["SKU", "Name", "Price", "Stock"]);
        foreach (var p in products)
        {
            AppendRow(csv,
            [
                CsvFields.ProtectText(p.Sku),
                CsvFields.ProtectText(p.Name),
                Money.FormatForInput(p.PriceCents),
                p.StockQuantity.ToString(BusinessFormats.Culture),
            ]);
        }

        // With the mark, a spreadsheet application opened by double
        // click reads the file as UTF-8, accents included.
        return CsvText.EncodeWithBom(csv.ToString());
    }

    private static void AppendRow(StringBuilder csv, string[] fields)
    {
        var quoted =
            fields.Select(f => CsvFields.Quote(f, Separator));
        csv.Append(string.Join(Separator, quoted)).Append("\r\n");
    }
}
