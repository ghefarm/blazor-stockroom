namespace Stockroom.Web.Maintenance;

public static class MaintenanceAllowList
{
    private static readonly string[] Prefixes =
    [
        MaintenanceMiddleware.PagePath,
        "/health",
        "/_framework",
        "/_content",
        "/lib",
    ];

    // Static files only. A page never ends in one of these.
    private static readonly string[] Extensions =
        [".css", ".js", ".png", ".ico", ".svg", ".woff2"];

    public static bool IsAllowed(PathString path)
    {
        foreach (var prefix in Prefixes)
        {
            if (path.StartsWithSegments(prefix))
            {
                return true;
            }
        }

        var extension = Path.GetExtension(path.Value);
        return Extensions.Contains(
            extension, StringComparer.OrdinalIgnoreCase);
    }
}
