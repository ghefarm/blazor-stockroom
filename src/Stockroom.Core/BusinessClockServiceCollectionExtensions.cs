using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Stockroom.Core;

public static class BusinessClockServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessClock(
        this IServiceCollection services, string timeZoneId)
    {
        // Looked up now, so a wrong id stops the app at startup
        // and not on the first page that asks for the date.
        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(sp => new BusinessClock(
            sp.GetRequiredService<TimeProvider>(), zone));
        return services;
    }
}
