namespace Ch15.Demo;

public static class DoubleDemo
{
    public static void Run()
    {
        Console.WriteLine("--- double ---");

        double a = 4285.71;
        double b = 2678.57;
        Console.WriteLine(a + b);
        Console.WriteLine(0.1 + 0.2);
        Console.WriteLine(4.35 * 100);
        Console.WriteLine((long)(4.35 * 100));

        CountAmountsThatAreNotWholeCents();
    }

    private static void CountAmountsThatAreNotWholeCents()
    {
        // Every amount from 0.01 to 10,000.00, stored as a double.
        var notWhole = 0;
        for (var cents = 1; cents <= 1_000_000; cents++)
        {
            double amount = cents / 100.0;
            double times100 = amount * 100;
            if (times100 != Math.Round(times100))
            {
                notWhole++;
            }
        }

        Console.WriteLine($"{notWhole} of 1000000 are not whole");
    }
}
