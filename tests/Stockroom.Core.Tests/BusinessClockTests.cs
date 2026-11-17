using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Stockroom.Core.Tests;

public class BusinessClockTests
{
    private static readonly TimeZoneInfo Chicago =
        TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

    [Test]
    public void Today_IsTheDateInTheBusinessTimeZone()
    {
        // Mid-year, so "last month" and "this year" never move.
        var time = new FakeTimeProvider(
            new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero));
        var clock = new BusinessClock(time, Chicago);

        var lastMonth = clock.Today.AddMonths(-1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(clock.Now.Offset,
                Is.EqualTo(TimeSpan.FromHours(-5)));
            Assert.That(clock.Today,
                Is.EqualTo(new DateOnly(2026, 7, 15)));
            Assert.That(lastMonth.Month, Is.EqualTo(6));
            Assert.That(lastMonth.Year, Is.EqualTo(2026));
        }
    }

    [Test]
    public void Today_DoesNotFollowUtc_AndWrongZoneFailsEarly()
    {
        // 03:30 UTC on February 1 is still January 31 in Chicago.
        var time = new FakeTimeProvider(
            new DateTimeOffset(2026, 2, 1, 3, 30, 0, TimeSpan.Zero));
        var clock = new BusinessClock(time, Chicago);
        var services = new ServiceCollection();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(clock.Today,
                Is.EqualTo(new DateOnly(2026, 1, 31)));
            Assert.That(() => services.AddBusinessClock("Mars/Base"),
                Throws.TypeOf<TimeZoneNotFoundException>());
        }
    }
}
