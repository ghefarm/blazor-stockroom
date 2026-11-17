using System.Security.Claims;
using Stockroom.Core;
using Stockroom.Core.Services;

namespace Stockroom.Web.Identity;

public static class ClaimsPrincipalExtensions
{
    public static AccountViewer ToViewer(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException(
                "Only a signed-in user can view accounts.");
        return new AccountViewer(id, user.IsInRole(Roles.Admin));
    }
}
