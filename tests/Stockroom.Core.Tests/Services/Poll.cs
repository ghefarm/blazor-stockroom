using System.Diagnostics;

namespace Stockroom.Core.Tests.Services;

public static class Poll
{
    private static readonly TimeSpan Deadline =
        TimeSpan.FromSeconds(5);

    // Polls instead of sleeping: fast when the condition is met
    // early, and a clear failure when it never is.
    public static async Task UntilAsync(Func<bool> condition)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            if (watch.Elapsed > Deadline)
            {
                Assert.Fail($"Condition not met within {Deadline}.");
            }

            await Task.Delay(10);
        }
    }
}
