using Serilog;
using Stockroom.Core.Storage;

namespace Stockroom.Web.Hosting;

public static class LoggingSetup
{
    // The trace id links a log line to the number on the error page.
    public const string FileTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
        "{TraceId} {Message:lj}{NewLine}{Exception}";

    // Configuration is not loaded yet, so only the environment
    // variable can move this file.
    public static string GetStartupLogPath()
    {
        var root = Environment.GetEnvironmentVariable(
            "Stockroom__DataRoot") ?? DataFolders.GetDefaultRoot();
        var folders = new DataFolders(root);
        return Path.Combine(folders.Logs, "startup-.log");
    }

    public static void AddStockroomLogging(
        this WebApplicationBuilder builder)
    {
        var isDevelopment = builder.Environment.IsDevelopment();

        builder.Services.AddSerilog(config =>
        {
            config.ReadFrom.Configuration(builder.Configuration);

            // Under IIS, console output can end up in a stdout
            // file that is never rotated.
            if (isDevelopment)
            {
                config.WriteTo.Console();
            }
        });
    }
}
