using System.Globalization;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

/// <summary>
/// The order filter as it appears in the address. The page and the
/// export read it with the same rules.
/// </summary>
public static class OrderListQuery
{
    public const string SupplierName = "supplier";
    public const string StatusName = "status";
    public const string FromName = "from";
    public const string ToName = "to";
    public const string SearchName = "q";
    public const string PageName = "page";
    public const string SizeName = "size";

    private const string All = "all";
    private const string DateFormat = "yyyy-MM-dd";

    // Returns false when a value is there but makes no sense. The
    // filter then leaves that value out.
    public static bool TryRead(
        string? supplier, string? status, string? from,
        string? to, string? search, out OrderListFilter filter)
    {
        var valid = TryReadId(supplier, out var supplierId);
        valid &= TryReadStatus(status, out var orderStatus);
        valid &= TryReadDate(from, out var fromDate);
        valid &= TryReadDate(to, out var toDate);
        if (fromDate > toDate)
        {
            valid = false;
            fromDate = null;
            toDate = null;
        }

        filter = new OrderListFilter(
            supplierId, orderStatus, fromDate, toDate, search)
            .Normalize();
        return valid;
    }

    public static Dictionary<string, object?> Write(
        OrderListFilter filter) => new()
    {
        [SupplierName] = filter.SupplierId,
        [StatusName] = filter.Status?.ToString() ?? All,
        [FromName] = FormatDate(filter.From),
        [ToName] = FormatDate(filter.To),
        [SearchName] = filter.Search,
    };

    public static string? FormatDate(DateOnly? date) =>
        date?.ToString(DateFormat, CultureInfo.InvariantCulture);

    private static bool TryReadId(string? text, out int? id)
    {
        id = null;
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        if (!int.TryParse(text, NumberStyles.None,
            CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        id = value;
        return true;
    }

    private static bool TryReadStatus(
        string? text, out OrderStatus? status)
    {
        status = null;
        if (string.IsNullOrEmpty(text) || text == All)
        {
            return true;
        }

        // TryParse also takes numbers, even "7", which is no status.
        if (!Enum.TryParse<OrderStatus>(text, out var value)
            || !Enum.IsDefined(value))
        {
            return false;
        }

        status = value;
        return true;
    }

    private static bool TryReadDate(string? text, out DateOnly? date)
    {
        date = null;
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        if (!DateOnly.TryParseExact(text, DateFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None,
            out var value))
        {
            return false;
        }

        date = value;
        return true;
    }
}
