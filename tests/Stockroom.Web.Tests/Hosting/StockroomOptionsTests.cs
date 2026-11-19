using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stockroom.Web.Hosting;

namespace Stockroom.Web.Tests.Hosting;

public class StockroomOptionsTests
{
    [Test]
    public void ValidValues_AreBound()
    {
        var options = CreateOptions("UTC", "it@example.com");

        var value = options.Value;

        Assert.That(value.BusinessTimeZone, Is.EqualTo("UTC"));
        Assert.That(value.SupportEmail, Is.EqualTo("it@example.com"));
    }

    [Test]
    public void WrongValues_AreAllReported()
    {
        var options = CreateOptions("Mars/Olympus", "it-desk");

        var ex = Assert.Throws<OptionsValidationException>(
            () => _ = options.Value);

        Assert.That(ex!.Failures, Has.Some.Contains("SupportEmail"));
        Assert.That(ex.Failures, Has.Some.Contains("time zone"));
    }

    private static IOptions<StockroomOptions> CreateOptions(
        string zone, string email)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Stockroom:BusinessTimeZone"] = zone,
                ["Stockroom:SupportEmail"] = email,
            })
            .Build();

        var services = new ServiceCollection();
        services.AddStockroomOptions(config);
        return services.BuildServiceProvider()
            .GetRequiredService<IOptions<StockroomOptions>>();
    }
}
