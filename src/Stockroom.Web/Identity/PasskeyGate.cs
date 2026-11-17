using Microsoft.AspNetCore.Diagnostics;
using Stockroom.Web.Components.Account;

namespace Stockroom.Web.Identity;

public static class PasskeyGate
{
    private static readonly string[] Paths =
    [
        "/Account/Manage/Passkeys",
        "/Account/Manage/RenamePasskey",
        "/Account/PasskeyCreationOptions",
        "/Account/PasskeyRequestOptions",
    ];

    /// <summary>
    /// While passkeys are switched off, their pages and endpoints
    /// answer 404, for GET and POST alike.
    /// </summary>
    public static void UsePasskeyGate(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path;
            if (!AccountFeatures.Passkeys
                && Paths.Any(p => path.StartsWithSegments(p)))
            {
                // A plain 404. The not-found page would run again
                // for a POST, and fail on a request without a form.
                var pages = context.Features
                    .Get<IStatusCodePagesFeature>();
                if (pages is not null)
                {
                    pages.Enabled = false;
                }

                context.Response.StatusCode =
                    StatusCodes.Status404NotFound;
                return;
            }

            await next(context);
        });
    }
}
