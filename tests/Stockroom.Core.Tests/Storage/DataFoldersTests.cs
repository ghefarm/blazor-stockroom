using Stockroom.Core.Storage;

namespace Stockroom.Core.Tests.Storage;

public class DataFoldersTests
{
    [Test]
    public void CreateAll_CreatesEveryFolder_UnderTheAppFolder()
    {
        var name = "data-" + Guid.NewGuid().ToString("N");

        var folders = new DataFolders(name);
        folders.CreateAll();

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(folders.Root, Is.EqualTo(
                    Path.Combine(AppContext.BaseDirectory, name)));
                Assert.That(Directory.Exists(folders.Database));
                Assert.That(Directory.Exists(folders.Logs));
                Assert.That(Directory.Exists(folders.Documents));
                Assert.That(Directory.Exists(folders.Backups));
            });
        }
        finally
        {
            Directory.Delete(folders.Root, recursive: true);
        }
    }
}
