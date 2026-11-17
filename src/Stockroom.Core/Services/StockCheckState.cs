namespace Stockroom.Core.Services;

// Immutable: a reader always sees one whole state, never half of
// an update.
public sealed record StockCheckState(
    bool IsRunning,
    int Percent,
    string? ResultText,
    bool WasCancelled)
{
    public static StockCheckState Idle { get; } =
        new(false, 0, null, false);

    public static StockCheckState Started { get; } =
        new(true, 0, null, false);

    public StockCheckState Finish(StockCheckResult result) =>
        new(false, 100, result.Describe(), false);

    public StockCheckState MarkCancelled() =>
        this with
        {
            IsRunning = false,
            ResultText = "The stock check was cancelled.",
            WasCancelled = true,
        };

    public StockCheckState MarkFailed() =>
        this with
        {
            IsRunning = false,
            ResultText = "The stock check failed. See the log.",
        };
}
