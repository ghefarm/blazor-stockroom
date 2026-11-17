using Stockroom.Core.Files;

namespace Stockroom.Core.Tests.Files;

public class SafePathTests
{
    private const string Root = @"C:\data";

    [Test]
    public void IsInside_AcceptsPathsBelowTheRoot()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(SafePath.IsInside(Root, @"C:\data\a.csv"));
            Assert.That(SafePath.IsInside(Root, @"C:\DATA\x\b.csv"));
            Assert.That(SafePath.IsInside(Root, "a.csv"));
            Assert.That(FileNames.MakeSafe("list.csv", "f.csv"),
                Is.EqualTo("list.csv"));
        }
    }

    [Test]
    public void IsInside_RefusesEveryWayOut()
    {
        string[] outside =
        [
            @"C:\data2\a.csv",
            @"C:\data\..\secret.txt",
            @"..\secret.txt",
            @"D:\a.csv",
            @"\server\share\a.csv",
            "a\0.csv",
            @"C:\data",
        ];

        using (Assert.EnterMultipleScope())
        {
            foreach (var path in outside)
            {
                Assert.That(SafePath.IsInside(Root, path), Is.False,
                    path);
            }

            Assert.That(FileNames.MakeSafe(@"..\..\web.config", "f"),
                Is.EqualTo("web.config"));
            Assert.That(FileNames.MakeSafe("list<1>?.csv", "f"),
                Is.EqualTo("list_1__.csv"));
            Assert.That(FileNames.MakeSafe("..", "f.csv"),
                Is.EqualTo("f.csv"));
        }
    }
}
