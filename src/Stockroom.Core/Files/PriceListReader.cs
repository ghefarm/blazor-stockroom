using Stockroom.Core.Services;

namespace Stockroom.Core.Files;

public sealed record PriceListContent(
    IReadOnlyList<PriceChange> Changes,
    IReadOnlyList<string> Errors);

/// <summary>
/// Reads and checks every row of a supplier price list before
/// anything is written.
/// </summary>
public static class PriceListReader
{
    public static PriceListContent Read(byte[] bytes)
    {
        var text = CsvText.Decode(bytes);

        // Supplier files use semicolons, our own exports commas.
        var firstLine = text.Split('\n', 2)[0];
        var separator = firstLine.Contains(';') ? ';' : ',';

        var rows = CsvParser.Parse(text, separator);
        if (rows.Count == 0)
        {
            return Fail("The file is empty.");
        }

        var header = rows[0].Select(h => h.Trim()).ToList();
        var skuColumn = header.FindIndex(IsNamed("SKU"));
        var priceColumn = header.FindIndex(IsNamed("Price"));
        if (skuColumn < 0 || priceColumn < 0)
        {
            return Fail("The first row needs the columns SKU "
                + "and Price.");
        }

        return ReadRows(rows, skuColumn, priceColumn);
    }

    private static PriceListContent ReadRows(
        List<string[]> rows, int skuColumn, int priceColumn)
    {
        List<PriceChange> changes = [];
        List<string> errors = [];
        HashSet<string> seen = new(StringComparer.Ordinal);

        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var number = i + 1;
            var sku = Field(row, skuColumn).Trim();
            var price = Field(row, priceColumn);

            if (sku.Length == 0)
            {
                errors.Add($"Row {number}: the SKU is missing.");
            }
            else if (!seen.Add(sku))
            {
                errors.Add($"Row {number}: {sku} appears twice.");
            }
            else if (!Money.TryParse(price, out var cents)
                || cents <= 0)
            {
                errors.Add(
                    $"Row {number}: \"{price}\" is not a price.");
            }
            else
            {
                changes.Add(new PriceChange(sku, cents));
            }
        }

        return new PriceListContent(changes, errors);
    }

    private static string Field(string[] row, int column) =>
        column < row.Length ? row[column] : "";

    private static Predicate<string> IsNamed(string name) =>
        h => h.Equals(name, StringComparison.OrdinalIgnoreCase);

    private static PriceListContent Fail(string error) =>
        new([], [error]);
}
