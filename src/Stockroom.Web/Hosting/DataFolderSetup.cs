using Stockroom.Core.Storage;

namespace Stockroom.Web.Hosting;

public static class DataFolderSetup
{
    public static DataFolders AddDataFolders(
        this WebApplicationBuilder builder)
    {
        var root = builder.Configuration["Stockroom:DataRoot"]
            ?? DataFolders.GetDefaultRoot();
        var folders = new DataFolders(root);
        folders.CreateAll();

        // Added last, so this file wins over every other source,
        // the command line included. No update ever ships it.
        builder.Configuration.AddJsonFile(
            folders.SettingsFile,
            optional: true,
            reloadOnChange: false);

        builder.Services.AddSingleton(folders);
        return folders;
    }
}
