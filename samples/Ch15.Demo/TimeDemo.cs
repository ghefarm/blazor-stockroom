using Microsoft.Extensions.Time.Testing;
using Stockroom.Core;

namespace Ch15.Demo;

public static class TimeDemo
{
    public static void Run()
    {
        Console.WriteLine("--- time ---");

        var chicago = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Chicago");
        var utc = new DateTimeOffset(
            2026, 2, 1, 3, 30, 0, TimeSpan.Zero);
        var clock = new BusinessClock(
            new FakeTimeProvider(utc), chicago);

        var serverToday = DateOnly.FromDateTime(utc.UtcDateTime);
        Console.WriteLine($"UTC server: {serverToday:yyyy-MM-dd}");
        Console.WriteLine($"Business:   {clock.Today:yyyy-MM-dd}");
        Console.WriteLine($"Now: {clock.Now:yyyy-MM-dd HH:mm zzz}");
        Console.WriteLine(BusinessFormats.FormatDate(clock.Today));
    }
}
