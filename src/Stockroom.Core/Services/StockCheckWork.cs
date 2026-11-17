using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

public sealed class StockCheckWork(
    IDbContextFactory<StockroomDbContext> factory,
    TimeSpan timePerProduct)
    : IStockCheckWork
{
    public StockCheckResult Run(
        IProgress<StockCheckProgress> progress,
        CancellationToken cancellationToken)
    {
        var products = LoadProducts();
        var low = 0;
        var outOfStock = 0;
        for (var i = 0; i < products.Count; i++)
        {
            InspectSlowly(cancellationToken);

            var quantity = products[i].StockQuantity;
            switch (StockRules.DetermineLevel(quantity))
            {
                case StockLevel.Low:
                    low++;
                    break;
                case StockLevel.OutOfStock:
                    outOfStock++;
                    break;
            }

            progress.Report(new(i + 1, products.Count));
        }

        return new StockCheckResult(products.Count, low, outOfStock);
    }

    // A short-lived context, as in every service since chapter 10.
    private List<Product> LoadProducts()
    {
        using var db = factory.CreateDbContext();
        return [.. db.Products.AsNoTracking().OrderBy(p => p.Id)];
    }

    // Simulates slow synchronous work, such as reading a file.
    // It blocks the thread like that work, but a cancel wakes it.
    private void InspectSlowly(CancellationToken cancellationToken)
    {
        cancellationToken.WaitHandle.WaitOne(timePerProduct);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
