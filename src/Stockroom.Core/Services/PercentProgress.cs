namespace Stockroom.Core.Services;

// Not Progress<T>: on a thread without a synchronization context,
// it posts every report to the thread pool, so reports could run
// out of order or arrive after the result.
public sealed class PercentProgress(StockCheckStatus status)
    : IProgress<StockCheckProgress>
{
    private int lastPercent = -1;

    public void Report(StockCheckProgress value)
    {
        var percent = value.Total == 0
            ? 100
            : (int)(value.Done * 100L / value.Total);

        // At most one update per whole percent. Only the last
        // product reaches 100, so the last report always passes.
        if (percent == lastPercent)
        {
            return;
        }

        lastPercent = percent;
        status.Update(state => state with { Percent = percent });
    }
}
