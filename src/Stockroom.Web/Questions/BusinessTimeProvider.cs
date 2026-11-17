namespace Stockroom.Web.Questions;

// GetLocalNow uses LocalTimeZone. The server may run in UTC, but
// "last month" means the month of the business.
public sealed class BusinessTimeProvider(TimeZoneInfo zone)
    : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => zone;
}
