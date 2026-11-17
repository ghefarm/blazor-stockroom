using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;
using Stockroom.Web.Components.Account;
using Stockroom.Web.Identity;

namespace Stockroom.Web.Hosting;

public static class IdentitySetup
{
    public static void AddStockroomIdentity(
        this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider,
            IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme =
                    IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme =
                    IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            // Keep the built-in handlers: they do more than redirect.
            var toLogin = options.Events.OnRedirectToLogin;
            var toDenied = options.Events.OnRedirectToAccessDenied;

            options.Events.OnRedirectToLogin = context =>
                AnswerApi(context, StatusCodes.Status401Unauthorized)
                    ?? toLogin(context);
            options.Events.OnRedirectToAccessDenied = context =>
                AnswerApi(context, StatusCodes.Status403Forbidden)
                    ?? toDenied(context);
        });

        // Identity uses the scoped context that AddDbContextFactory
        // registered (chapter 10).
        services.AddIdentityCore<ApplicationUser>(AccountRules.Apply)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<StockroomDbContext>()
            .AddSignInManager<StockroomSignInManager>()
            .AddErrorDescriber<StockroomErrorDescriber>()
            .AddDefaultTokenProviders();

        services.AddScoped<UserAccountService>();
        services.AddScoped<AccountSetup>();
    }

    // A program that calls /api wants a status code, not the HTML
    // of the sign-in page.
    private static Task? AnswerApi(
        RedirectContext<CookieAuthenticationOptions> context,
        int statusCode)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            return null;
        }

        context.Response.StatusCode = statusCode;
        return Task.CompletedTask;
    }

    public static async Task EnsureAccountsAsync(
        this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var setup = scope.ServiceProvider
            .GetRequiredService<AccountSetup>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<AccountSetup>>();

        await setup.EnsureRolesAsync();

        var section = app.Configuration.GetSection("FirstAdmin");
        var userName = section["UserName"];
        var password = section["Password"];
        if (!string.IsNullOrEmpty(userName)
            && !string.IsNullOrEmpty(password)
            && await setup.EnsureFirstAdminAsync(userName, password))
        {
            logger.LogWarning(
                "Created the first administrator {UserName}.",
                userName);
        }

        if (await setup.HasAdminWithInitialPasswordAsync())
        {
            logger.LogWarning(
                "An administrator still has the initial password.");
        }
    }
}
