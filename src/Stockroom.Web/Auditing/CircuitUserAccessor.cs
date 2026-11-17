using System.Security.Claims;

namespace Stockroom.Web.Auditing;

/// <summary>
/// The signed-in user of the circuit activity that is running now.
/// </summary>
public sealed class CircuitUserAccessor
{
    // AsyncLocal: each activity sees its own value, also after an
    // await, and two circuits never see each other's user.
    private static readonly AsyncLocal<ClaimsPrincipal?> current =
        new();

    public ClaimsPrincipal? User
    {
        get => current.Value;
        set => current.Value = value;
    }
}
