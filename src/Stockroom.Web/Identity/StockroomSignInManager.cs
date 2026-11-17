using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Stockroom.Core.Entities;

namespace Stockroom.Web.Identity;

/// <summary>
/// The one place that decides who may sign in. Identity asks it
/// before every sign-in, the revalidation of circuits asks it too.
/// </summary>
public class StockroomSignInManager(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor contextAccessor,
    IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
    IOptions<IdentityOptions> optionsAccessor,
    ILogger<SignInManager<ApplicationUser>> logger,
    IAuthenticationSchemeProvider schemes,
    IUserConfirmation<ApplicationUser> confirmation)
    : SignInManager<ApplicationUser>(userManager, contextAccessor,
        claimsFactory, optionsAccessor, logger, schemes, confirmation)
{
    public override async Task<bool> CanSignInAsync(
        ApplicationUser user)
    {
        if (user.IsArchived)
        {
            return false;
        }

        return await base.CanSignInAsync(user);
    }
}
