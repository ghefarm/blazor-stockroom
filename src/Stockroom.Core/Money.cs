using System.Globalization;

namespace Stockroom.Core;

public static class Money
{
    // The invariant culture keeps the output independent of the
    // language settings of the server.
    public static string Format(long cents)
    {
        var amount = cents / 100m;
        return amount.ToString("N2", CultureInfo.InvariantCulture);
    }
}
