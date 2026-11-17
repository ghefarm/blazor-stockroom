using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.ReadOnly;
using Stockroom.Core.Services;

namespace Stockroom.Web.Hosting;

public static class ReadOnlySetup
{
    public static async Task StartReadOnlyRuleAsync(
        this WebApplication app)
    {
        var factory = app.Services.GetRequiredService<
            IDbContextFactory<StockroomDbContext>>();
        await using (var db = await factory.CreateDbContextAsync())
        {
            // ThrowIfReadOnly finds the interceptor in the options.
            // If it could not, it would pass without a word.
            if (!db.HasReadOnlyRule())
            {
                throw new InvalidOperationException(
                    "The read-only interceptor is not registered.");
            }
        }

        await using var scope = app.Services.CreateAsyncScope();
        await scope.ServiceProvider
            .GetRequiredService<SubscriptionService>().LoadAsync();

        var state =
            app.Services.GetRequiredService<SubscriptionState>();
        app.Logger.LogInformation(
            "Paid until {PaidUntil:yyyy-MM-dd}, "
            + "writes allowed: {AllowsWrites}",
            state.PaidUntil, state.AllowsWrites);
    }
}
