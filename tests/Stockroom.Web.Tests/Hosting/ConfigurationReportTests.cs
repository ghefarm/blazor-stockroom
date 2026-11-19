using Microsoft.Extensions.Configuration;
using Stockroom.Web.Hosting;

namespace Stockroom.Web.Tests.Hosting;

public class ConfigurationReportTests
{
    private const string Key = "Stockroom:BusinessTimeZone";

    [Test]
    public void LastSourceWithTheKey_IsReported()
    {
        var root = new ConfigurationBuilder()
            .AddInMemoryCollection(Values("America/Chicago"))
            .AddCommandLine(["--Stockroom:BusinessTimeZone=UTC"])
            .AddInMemoryCollection(Values(null))
            .Build();

        var source = ConfigurationReport.FindSource(root, Key);

        Assert.That(root[Key], Is.EqualTo("UTC"));
        Assert.That(source, Does.Contain("CommandLine"));
    }

    [Test]
    public void MissingKey_HasNoSource()
    {
        var root = new ConfigurationBuilder().Build();

        var source = ConfigurationReport.FindSource(root, Key);

        Assert.That(source, Is.EqualTo("no source"));
    }

    private static Dictionary<string, string?> Values(string? zone) =>
        zone is null ? [] : new() { [Key] = zone };
}
