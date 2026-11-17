using System.Security.Claims;

namespace Stockroom.Core.Auditing;

public static class AuditUser
{
    /// <summary>
    /// The name written when no user is signed in: startup, sample
    /// data, background work.
    /// </summary>
    public const string System = "system";

    public static string GetName(ClaimsPrincipal? user)
    {
        var name = user?.Identity?.IsAuthenticated == true
            ? user.Identity.Name
            : null;
        return string.IsNullOrEmpty(name) ? System : name;
    }
}
