using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Stockroom.Core;
using Stockroom.Core.Services;
using Stockroom.Web.Identity;

namespace Stockroom.Web.Endpoints;

public static class ApiEndpoints
{
    public static void MapStockroomApi(this WebApplication app)
    {
        // Every endpoint in the group needs a signed-in user.
        var api = app.MapGroup("/api").RequireAuthorization();

        api.MapGet("/orders", (PurchaseOrderService orders,
                CancellationToken ct) => orders.ListAsync(ct))
            .RequireAuthorization(new AuthorizeAttribute
            {
                Roles = Roles.PurchasingOrAdmin,
            });

        // No parameter says whose accounts: the signed-in user
        // decides, so nobody can ask for a colleague's row.
        api.MapGet("/accounts", (ClaimsPrincipal user,
                UserAccountService accounts, CancellationToken ct) =>
            accounts.ListVisibleAsync(user.ToViewer(), ct));
    }
}
