using Microsoft.Extensions.Configuration.Memory;
using Stockroom.Core.Storage;

namespace Stockroom.Web.Hosting;

public static class ConfigurationDefaults
{
    // Defaults that belong to the code, such as a computed path.
    // A default that people should read goes into appsettings.json.
    public static void AddStockroomDefaults(
        this WebApplicationBuilder builder, DataFolders folders)
    {
        var defaults = new MemoryConfigurationSource
        {
            InitialData =
            [
                new("Serilog:WriteTo:File:Args:path",
                    Path.Combine(folders.Logs, "stockroom-.log")),
                new("Serilog:WriteTo:File:Args:outputTemplate",
                    LoggingSetup.FileTemplate),
            ],
        };

        // Position 0 is the weakest source: every other one wins.
        builder.Configuration.Sources.Insert(0, defaults);
    }
}
