using Stockroom.Core;

namespace Ch15.Demo;

public static class RoundingDemo
{
    public static void Run()
    {
        Console.WriteLine("--- rounding ---");

        Console.WriteLine(Math.Round(10.5m));
        Console.WriteLine(
            Math.Round(10.5m, MidpointRounding.AwayFromZero));

        long[] lines = [105, 105, 105];
        foreach (var line in lines)
        {
            var discount = Money.CalculatePercent(line, 10m);
            Console.WriteLine(
                $"{Money.Format(line),8} {Money.Format(discount),6}");
        }

        long[] discounts =
            [.. lines.Select(l => Money.CalculatePercent(l, 10m))];
        var ofTotal = Money.CalculatePercent(lines.Sum(), 10m);
        Console.WriteLine($"Rows: {Money.Format(discounts.Sum())}");
        Console.WriteLine($"Total: {Money.Format(ofTotal)}");

        Console.WriteLine(string.Join(" + ", Money.Split(1_000, 3)));
    }
}
