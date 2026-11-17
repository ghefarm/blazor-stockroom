namespace Stockroom.Core;

/// <summary>
/// The current time and date in the one time zone of the business.
/// </summary>
public sealed class BusinessClock(
    TimeProvider time, TimeZoneInfo zone)
{
    public TimeZoneInfo Zone => zone;

    public DateTimeOffset UtcNow => time.GetUtcNow();

    public DateTimeOffset Now =>
        TimeZoneInfo.ConvertTime(UtcNow, zone);

    public DateOnly Today => DateOnly.FromDateTime(Now.DateTime);
}
