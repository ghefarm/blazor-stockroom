using Microsoft.Extensions.Logging.Abstractions;
using Stockroom.Web.Hosting;

namespace Stockroom.Web.Tests.Hosting;

public class EventGuardTests
{
    [Test]
    public async Task SuccessfulAction_GivesNoMessage()
    {
        var ran = false;

        var message = await EventGuard.RunAsync(
            () => { ran = true; return Task.CompletedTask; },
            NullLogger.Instance, "failed");

        Assert.That(ran, Is.True);
        Assert.That(message, Is.Null);
    }

    [Test]
    public async Task FailingAction_GivesTheMessage()
    {
        var message = await EventGuard.RunAsync(
            () => throw new InvalidOperationException("boom"),
            NullLogger.Instance, "failed");

        Assert.That(message, Is.EqualTo("failed"));
    }
}
