namespace Stockroom.Core.Storage;

/// <summary>
/// The folders outside the program folder. They survive every update.
/// </summary>
public sealed class DataFolders
{
    public DataFolders(string root)
    {
        // A relative path would be resolved against the working
        // directory. For a Windows service that is system32.
        Root = Path.GetFullPath(root, AppContext.BaseDirectory);
        Database = Path.Combine(Root, "db");
        Logs = Path.Combine(Root, "logs");
        Documents = Path.Combine(Root, "documents");
        Backups = Path.Combine(Root, "backups");
    }

    public string Root { get; }
    public string Database { get; }
    public string Logs { get; }
    public string Documents { get; }
    public string Backups { get; }

    public string DatabaseFile =>
        Path.Combine(Database, "stockroom.db");

    public string SettingsFile =>
        Path.Combine(Root, "settings.json");

    public static string GetDefaultRoot() =>
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData),
            "Stockroom");

    // SQLite creates the database file, but not its folder.
    public void CreateAll()
    {
        string[] all = [Root, Database, Logs, Documents, Backups];
        foreach (var folder in all)
        {
            Directory.CreateDirectory(folder);
        }
    }
}
