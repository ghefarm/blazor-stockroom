using System.Globalization;

namespace Stockroom.Core;

/// <summary>
/// The one place that decides how Stockroom shows numbers and dates.
/// </summary>
public static class BusinessFormats
{
    // Never the culture of the server: that one can change with an
    // update, an installer or a different service account.
    public static CultureInfo Culture { get; } =
        CultureInfo.GetCultureInfo("en-US");

    public static string FormatDate(DateOnly date)
    {
        return date.ToString("MMM d, yyyy", Culture);
    }

    public static string FormatDateTime(DateTime time)
    {
        return time.ToString("MMM d, yyyy h:mm tt", Culture);
    }

    public static string FormatQuantity(int quantity)
    {
        return quantity.ToString("N0", Culture);
    }
}
