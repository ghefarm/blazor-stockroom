using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Stockroom.Core.Entities;

namespace Stockroom.Web.Components.Account;

// Checks the user of an open circuit again every 30 minutes.
internal sealed class IdentityRevalidatingAuthenticationStateProvider(
        ILoggerFactory loggerFactory,
        IServiceScopeFactory scopeFactory,
        IOptions<IdentityOptions> options)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval =>
        TimeSpan.FromMinutes(30);

    protected override async Task<bool>
        ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState,
            CancellationToken cancellationToken)
    {
        // A new scope, so the user manager reads fresh data.
        await using var scope = scopeFactory.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var userManager = services
            .GetRequiredService<UserManager<ApplicationUser>>();
        var signInManager = services
            .GetRequiredService<SignInManager<ApplicationUser>>();

        var principal = authenticationState.User;
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return false;
        }

        // A rule like an archived account does not always change the
        // security stamp, so ask the same question as at sign-in.
        if (!await signInManager.CanSignInAsync(user))
        {
            return false;
        }

        var stampType =
            options.Value.ClaimsIdentity.SecurityStampClaimType;
        var principalStamp = principal.FindFirstValue(stampType);
        var userStamp = await userManager.GetSecurityStampAsync(user);
        return principalStamp == userStamp;
    }
}
