using Microsoft.Extensions.DependencyInjection;

namespace Stockroom.Core.Services;

public static class StockCheckWorkerSetup
{
    // The caller registers IStockCheckWork. Each job resolves it
    // from a scope of its own, so it may also be scoped.
    public static IServiceCollection AddStockCheckWorker(
        this IServiceCollection services)
    {
        services.AddSingleton<StockCheckStatus>();
        services.AddSingleton<StockCheckWorker>();

        // One instance in three roles: the host starts and stops
        // it, and pages start and cancel jobs through it.
        services.AddSingleton<IStockCheckRunner>(provider =>
            provider.GetRequiredService<StockCheckWorker>());
        services.AddHostedService(provider =>
            provider.GetRequiredService<StockCheckWorker>());
        return services;
    }
}
