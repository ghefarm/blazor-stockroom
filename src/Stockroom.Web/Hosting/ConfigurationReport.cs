namespace Stockroom.Web.Hosting;

public static class ConfigurationReport
{
    // Never add a password or an API key to this list.
    private static readonly string[] LoggedKeys =
    [
        "Stockroom:BusinessTimeZone",
        "Maintenance:Enabled",
    ];

    public static string FindSource(
        IConfigurationRoot root, string key)
    {
        // The last provider that knows the key is the one that won.
        foreach (var provider in root.Providers.Reverse())
        {
            if (provider.TryGet(key, out _))
            {
                return provider.ToString() ?? provider.GetType().Name;
            }
        }

        return "no source";
    }

    public static void LogConfigurationSources(
        this WebApplication app)
    {
        var root = (IConfigurationRoot)app.Configuration;
        app.Logger.LogInformation(
            "Configuration sources, weakest first: {Sources}",
            root.Providers.Select(p => p.ToString()));

        foreach (var key in LoggedKeys)
        {
            app.Logger.LogInformation(
                "{Key} = {Value} from {Source}",
                key, root[key], FindSource(root, key));
        }
    }
}
