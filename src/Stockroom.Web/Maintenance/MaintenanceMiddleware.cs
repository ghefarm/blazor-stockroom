using Microsoft.Extensions.Options;

namespace Stockroom.Web.Maintenance;

public sealed class MaintenanceMiddleware(
    RequestDelegate next, IOptions<MaintenanceOptions> options)
{
    public const string PagePath = "/maintenance";

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (options.Value.Enabled
            && !MaintenanceAllowList.IsAllowed(path))
        {
            context.Response.Redirect(PagePath);
            return;
        }

        await next(context);
    }
}
