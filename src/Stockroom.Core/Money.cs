using System.Globalization;

namespace Stockroom.Core;

/// <summary>
/// Formats, parses and rounds money that is stored as whole cents.
/// </summary>
public static class Money
{
    // Far above any real amount in Stockroom. It keeps the cents
    // inside a long and catches a finger that stayed on a key.
    private const decimal MaxAmount = 1_000_000_000m;

    public static string Format(long cents)
    {
        var amount = cents / 100m;
        return amount.ToString("N2", BusinessFormats.Culture);
    }

    public static string FormatForInput(long cents)
    {
        // No thousands separators, so TryParse reads it back.
        var amount = cents / 100m;
        return amount.ToString("0.00", BusinessFormats.Culture);
    }

    public static bool TryParse(string? text, out long cents)
    {
        cents = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        // A dot or a comma is the decimal point. Thousands
        // separators are refused, because "1,234" is ambiguous.
        var normalized = text.Trim().Replace(',', '.');
        const NumberStyles style = NumberStyles.AllowLeadingSign
            | NumberStyles.AllowDecimalPoint;
        if (!decimal.TryParse(normalized, style,
                CultureInfo.InvariantCulture, out var amount))
        {
            return false;
        }

        if (Math.Abs(amount) > MaxAmount)
        {
            return false;
        }

        var exactCents = amount * 100m;
        if (exactCents != decimal.Truncate(exactCents))
        {
            return false;
        }

        cents = (long)exactCents;
        return true;
    }

    public static long CalculatePercent(long cents, decimal percent)
    {
        var exact = cents * percent / 100m;
        return (long)Math.Round(exact, MidpointRounding.AwayFromZero);
    }

    public static long[] Split(long cents, int parts)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cents);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(parts);

        var share = cents / parts;
        var rest = cents % parts;
        var result = new long[parts];
        for (var i = 0; i < parts; i++)
        {
            // The first parts get a cent more, so the sum is exact.
            result[i] = i < rest ? share + 1 : share;
        }

        return result;
    }
}
