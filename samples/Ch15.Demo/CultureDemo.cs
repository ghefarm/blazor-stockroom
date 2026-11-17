using System.Globalization;
using Stockroom.Core;

namespace Ch15.Demo;

public static class CultureDemo
{
    public static void Run()
    {
        Console.WriteLine("--- culture ---");

        var price = 1234.5m;
        var german = CultureInfo.GetCultureInfo("de-DE");
        var business = BusinessFormats.Culture;
        Console.WriteLine(price.ToString("N2", german));
        Console.WriteLine(price.ToString("N2", business));

        var style = NumberStyles.Number;
        var invariant = CultureInfo.InvariantCulture;
        Console.WriteLine(decimal.Parse("1,5", style, invariant));
        Console.WriteLine(decimal.Parse("1.5", style, german));

        Money.TryParse("1,5", out var cents);
        Console.WriteLine(cents);
    }
}
