using Microsoft.Extensions.Logging;

namespace Stockroom.Core.Services;

public sealed class StockCheckStatus(ILogger<StockCheckStatus> logger)
{
    private readonly Lock writeLock = new();
    private StockCheckState current = StockCheckState.Idle;

    public event Action<StockCheckState>? Changed;

    // No lock for reading: a page reads this many times per render,
    // and the record is replaced as a whole, never changed.
    public StockCheckState Current => Volatile.Read(ref current);

    public void Update(Func<StockCheckState, StockCheckState> change)
    {
        StockCheckState next;
        lock (writeLock)
        {
            next = change(current);
            if (next == current)
            {
                return;
            }

            Volatile.Write(ref current, next);
        }

        Notify(next);
    }

    private void Notify(StockCheckState state)
    {
        Delegate[] handlers = Changed?.GetInvocationList() ?? [];
        foreach (var handler in handlers)
        {
            // A subscriber whose circuit is gone may throw. It must
            // not stop the others or reach the code that called us.
            try
            {
                ((Action<StockCheckState>)handler)(state);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex, "A stock check subscriber failed.");
            }
        }
    }
}
