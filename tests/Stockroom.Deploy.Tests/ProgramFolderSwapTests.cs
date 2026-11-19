namespace Stockroom.Deploy.Tests;

public class ProgramFolderSwapTests
{
    private string root = "";
    private string app = "";
    private string next = "";
    private string old = "";

    [SetUp]
    public void CreateTwoVersions()
    {
        root = Path.Combine(AppContext.BaseDirectory,
            "swap-" + Guid.NewGuid().ToString("N"));
        app = Path.Combine(root, "app");
        next = Path.Combine(root, "app.new");
        old = Path.Combine(root, "app.old");
        Directory.CreateDirectory(app);
        Directory.CreateDirectory(next);
        File.WriteAllText(Path.Combine(app, "version.txt"), "1");
        File.WriteAllText(Path.Combine(next, "version.txt"), "2");
    }

    [TearDown]
    public void DeleteFolders()
    {
        Directory.Delete(root, recursive: true);
    }

    [Test]
    public void Swap_PutsTheNewVersionInPlace_AndKeepsTheOld()
    {
        ProgramFolderSwap.Default.Swap(app, next, old);

        Assert.Multiple(() =>
        {
            Assert.That(ReadVersion(app), Is.EqualTo("2"));
            Assert.That(ReadVersion(old), Is.EqualTo("1"));
            Assert.That(Directory.Exists(next), Is.False);
        });
    }

    [Test]
    public void Swap_PutsTheOldVersionBack_WhenTheNewOneIsLocked()
    {
        var swap = new ProgramFolderSwap(TimeSpan.FromSeconds(1));

        // An open file blocks the rename of its folder, as antivirus
        // software does with fresh files.
        using (File.OpenRead(Path.Combine(next, "version.txt")))
        {
            Assert.Catch<IOException>(
                () => swap.Swap(app, next, old));
        }

        Assert.Multiple(() =>
        {
            Assert.That(ReadVersion(app), Is.EqualTo("1"));
            Assert.That(Directory.Exists(old), Is.False);
            Assert.That(ReadVersion(next), Is.EqualTo("2"));
        });
    }

    private static string ReadVersion(string folder) =>
        File.ReadAllText(Path.Combine(folder, "version.txt"));
}
