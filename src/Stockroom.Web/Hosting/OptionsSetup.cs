using Stockroom.Web.Maintenance;

namespace Stockroom.Web.Hosting;

public static class OptionsSetup
{
    public static IServiceCollection AddStockroomOptions(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<StockroomOptions>()
            .Bind(config.GetSection(StockroomOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(o => IsKnownTimeZone(o.BusinessTimeZone),
                "BusinessTimeZone is not a known time zone.")
            .ValidateOnStart();

        services.AddOptions<MaintenanceOptions>()
            .Bind(config.GetSection(MaintenanceOptions.SectionName));
        return services;
    }

    private static bool IsKnownTimeZone(string id) =>
        TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);
}
