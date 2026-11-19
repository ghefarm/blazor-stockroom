using Microsoft.AspNetCore.Http;
using Stockroom.Web.Maintenance;

namespace Stockroom.Web.Tests.Hosting;

public class MaintenanceAllowListTests
{
    [TestCase("/maintenance")]
    [TestCase("/health")]
    [TestCase("/app.4n2y1ze3sy.css")]
    [TestCase("/Stockroom.Web.styles.css")]
    [TestCase("/lib/bootstrap/dist/css/bootstrap.min.css")]
    [TestCase("/_framework/blazor.web.js")]
    public void StaticFilesAndTheMaintenancePage_PassThrough(
        string path)
    {
        Assert.That(MaintenanceAllowList.IsAllowed(path), Is.True);
    }

    [TestCase("/")]
    [TestCase("/products")]
    [TestCase("/maintenance-report")]
    [TestCase("/products/2/label")]
    public void Pages_AreRedirected(string path)
    {
        Assert.That(MaintenanceAllowList.IsAllowed(path), Is.False);
    }
}
