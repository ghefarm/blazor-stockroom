namespace Stockroom.Core.Services;

// What a page needs from a stock check. The coordinator and the
// queued worker both offer it, so the page does not change when
// the design behind it changes.
public interface IStockCheckRunner
{
    bool TryStart();

    void Cancel();
}
