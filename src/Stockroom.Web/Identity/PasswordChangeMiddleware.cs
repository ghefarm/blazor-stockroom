using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Identity;
using Stockroom.Core.Entities;

namespace Stockroom.Web.Identity;

/// <summary>
/// Sends a user who must change the password to the page for it,
/// from every other page.
/// </summary>
public sealed class PasswordChangeMiddleware(RequestDelegate next)
{
    public const string PagePath = "/Account/PasswordChangeRequired";

    public async Task InvokeAsync(
        HttpContext context, UserManager<ApplicationUser> users)
    {
        if (IsOtherPage(context)
            && context.User.Identity?.IsAuthenticated == true)
        {
            // One read per page request: the flag can change at any
            // time, and the cookie would not know it.
            var user = await users.GetUserAsync(context.User);
            if (user is { MustChangePassword: true })
            {
                context.Response.Redirect(PagePath);
                return;
            }
        }

        await next(context);
    }

    private static bool IsOtherPage(HttpContext context)
    {
        // Only pages. Files, scripts and the posts of the account
        // endpoints pass, so the page keeps its styles.
        var page = context.GetEndpoint()?.Metadata
            .GetMetadata<ComponentTypeMetadata>();
        return page is not null
            && !context.Request.Path.StartsWithSegments(PagePath);
    }
}
