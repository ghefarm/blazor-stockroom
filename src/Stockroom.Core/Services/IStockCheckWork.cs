namespace Stockroom.Core.Services;

// The slow part of the stock check, behind an interface, so a
// test can replace it with a fake that waits as long as the test
// wants.
public interface IStockCheckWork
{
    StockCheckResult Run(
        IProgress<StockCheckProgress> progress,
        CancellationToken cancellationToken);
}
