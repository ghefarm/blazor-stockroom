using Microsoft.Extensions.Time.Testing;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class OrderActivityTests
{
    private static readonly TimeZoneInfo Chicago =
        TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

    [Test]
    public async Task CountsTheOrdersOfTheCurrentMonth()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var time = new FakeTimeProvider(new DateTimeOffset(
            2026, 10, 20, 12, 0, 0, TimeSpan.Zero));
        var activity = CreateActivity(database, time);

        var october = await activity.CountOrderedThisMonthAsync();
        time.Advance(TimeSpan.FromDays(26));
        var november = await activity.CountOrderedThisMonthAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(october, Is.EqualTo(2));
            Assert.That(november, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task UsesTheBusinessDayAtTheEdges()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        // 03:30 UTC on December 1 is still November 30 in Chicago.
        var time = new FakeTimeProvider(new DateTimeOffset(
            2026, 12, 1, 3, 30, 0, TimeSpan.Zero));
        var activity = CreateActivity(database, time);

        var lastDay = await activity.CountOrderedThisMonthAsync();
        time.Advance(TimeSpan.FromDays(1));
        var december = await activity.CountOrderedThisMonthAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lastDay, Is.EqualTo(1));
            Assert.That(december, Is.Zero);
        }
    }

    private static OrderActivity CreateActivity(
        TestDatabase database, FakeTimeProvider time) =>
        new(database.CreateFactory(),
            new BusinessClock(time, Chicago));
}
