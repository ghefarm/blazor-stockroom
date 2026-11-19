using Microsoft.Data.Sqlite;

namespace Stockroom.Core.Storage;

public static class SqliteBackup
{
    /// <summary>
    /// Copies a database that may be in use, with the SQLite online
    /// backup. A plain file copy of a live database can be broken.
    /// </summary>
    public static string CreateBackup(
        string databaseFile, string backupFolder, DateTimeOffset now)
    {
        var name = $"stockroom-{now:yyyyMMdd-HHmmss}.db";
        var target = Path.Combine(backupFolder, name);

        using var source =
            Open(databaseFile, SqliteOpenMode.ReadOnly);
        using var copy = Open(target, SqliteOpenMode.ReadWriteCreate);
        source.BackupDatabase(copy);
        return target;
    }

    // No pooling, so that both files are closed when we return.
    private static SqliteConnection Open(
        string file, SqliteOpenMode mode)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = file,
            Mode = mode,
            Pooling = false
        };
        var connection = new SqliteConnection(builder.ToString());
        connection.Open();
        return connection;
    }
}
