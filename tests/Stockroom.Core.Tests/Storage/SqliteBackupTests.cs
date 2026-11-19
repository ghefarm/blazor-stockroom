using Microsoft.Data.Sqlite;
using Stockroom.Core.Storage;

namespace Stockroom.Core.Tests.Storage;

public class SqliteBackupTests
{
    private string folder = "";

    [SetUp]
    public void CreateFolder()
    {
        folder = Path.Combine(AppContext.BaseDirectory,
            "backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
    }

    [TearDown]
    public void DeleteFolder()
    {
        SqliteConnection.ClearAllPools();
        Directory.Delete(folder, recursive: true);
    }

    [Test]
    public void CreateBackup_CopiesADatabaseThatIsInUse()
    {
        var file = Path.Combine(folder, "live.db");
        using var live = new SqliteConnection($"Data Source={file}");
        live.Open();
        Execute(live, "CREATE TABLE Products (Name TEXT)");
        Execute(live, "INSERT INTO Products VALUES ('Shelf')");

        var copy = SqliteBackup.CreateBackup(file, folder,
            new DateTimeOffset(2026, 3, 1, 22, 0, 0, TimeSpan.Zero));

        using var check = new SqliteConnection($"Data Source={copy}");
        check.Open();
        using var count = check.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM Products";
        Assert.Multiple(() =>
        {
            Assert.That(Path.GetFileName(copy),
                Is.EqualTo("stockroom-20260301-220000.db"));
            Assert.That(count.ExecuteScalar(), Is.EqualTo(1L));
        });
    }

    [Test]
    public void CreateBackup_FailsLoudly_WhenTheSourceIsMissing()
    {
        var missing = Path.Combine(folder, "missing.db");

        Assert.Throws<SqliteException>(() =>
            SqliteBackup.CreateBackup(missing, folder,
                DateTimeOffset.UnixEpoch));
        Assert.That(File.Exists(missing), Is.False);
    }

    private static void Execute(SqliteConnection connection,
        string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
