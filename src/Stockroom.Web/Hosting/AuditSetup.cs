using Microsoft.AspNetCore.Components.Server.Circuits;
using Stockroom.Core.Auditing;
using Stockroom.Web.Auditing;

namespace Stockroom.Web.Hosting;

public static class AuditSetup
{
    public static void AddStockroomAuditing(
        this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddSingleton<CircuitUserAccessor>();
        services.AddScoped<CircuitHandler, CircuitUserHandler>();

        // Singletons, because the singleton context factory
        // builds its options, and so the interceptor, only once.
        services.AddSingleton<ICurrentUser, CurrentUser>();
        services.AddSingleton<AuditInterceptor>();
    }
}
