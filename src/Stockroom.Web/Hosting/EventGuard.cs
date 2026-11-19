namespace Stockroom.Web.Hosting;

public static class EventGuard
{
    // An unhandled exception in an event handler ends the circuit,
    // and the user loses the page. A message keeps it alive.
    public static async Task<string?> RunAsync(
        Func<Task> action, ILogger logger, string failureMessage)
    {
        try
        {
            await action();
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Event handler failed");
            return failureMessage;
        }
    }
}
