using Microsoft.Extensions.Time.Testing;

namespace Stockroom.Questions.Tests;

public class PeriodReaderTests
{
    // Mid-year, so "last month" and "this year" stay stable.
    private readonly PeriodReader reader = new(TestTime.Create());

    [TestCase("open orders in march", "2026-03-01", "2026-03-31")]
    [TestCase("orders from September", "2025-09-01", "2025-09-30")]
    [TestCase("march last year", "2025-03-01", "2025-03-31")]
    [TestCase("May 2024", "2024-05-01", "2024-05-31")]
    [TestCase("orders from last month", "2026-06-01", "2026-06-30")]
    [TestCase("this year", "2026-01-01", "2026-12-31")]
    [TestCase("deliveries in 2025", "2025-01-01", "2025-12-31")]
    public void Read_KnownPhrase_ReturnsPeriod(
        string question, string from, string to)
    {
        var period = reader.Read(question);

        Assert.That(period, Is.EqualTo(new Period(
            DateOnly.Parse(from), DateOnly.Parse(to))));
    }

    [TestCase("open orders from northfield")]
    [TestCase("orders in march or april")]
    [TestCase("orders of the last 30 days")]
    [TestCase("orders from 1999")]
    public void Read_NoClearPeriod_ReturnsNull(string question)
    {
        Assert.That(reader.Read(question), Is.Null);
    }
}

public static class TestTime
{
    public static FakeTimeProvider Create()
    {
        FakeTimeProvider time = new(
            new DateTimeOffset(2026, 7, 15, 9, 30, 0, TimeSpan.Zero));
        time.SetLocalTimeZone(TimeZoneInfo.Utc);
        return time;
    }
}
