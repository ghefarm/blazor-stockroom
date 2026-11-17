using System.Globalization;
using System.Text;

namespace Stockroom.Core.Services;

/// <summary>
/// Turns the rows of the order list into a CSV file.
/// </summary>
public static class OrderExport
{
    public static string CreateCsv(IEnumerable<OrderSummary> rows)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Number,Supplier,Ordered on,Status,Total");
        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(",",
                Quote(row.OrderNumber),
                Quote(row.SupplierName),
                OrderListQuery.FormatDate(row.OrderedOn),
                row.Status,
                Quote(Money.Format(row.TotalCents))));
        }

        return csv.ToString();
    }

    // Named after the filters people use every day, so two exports
    // of different periods do not get the same name.
    public static string CreateFileName(OrderListFilter filter)
    {
        List<string?> parts =
        [
            "orders",
            OrderListQuery.FormatDate(filter.From),
            OrderListQuery.FormatDate(filter.To),
            filter.Normalize().Search,
        ];
        var name = string.Join("_", parts.Where(p => p is not null));
        return Clean(name) + ".csv";
    }

    // The browser saves the file on the user's machine, whose rules
    // the server does not know. Only safe characters survive.
    private static string Clean(string name) => new(
    [
        .. name.Select(c =>
            char.IsAsciiLetterOrDigit(c) || c is '-' or '_'
                ? c
                : '-'),
    ]);

    private static string Quote(string value) =>
        "\"" + value.Replace("\"", "\"\"") + "\"";
}
