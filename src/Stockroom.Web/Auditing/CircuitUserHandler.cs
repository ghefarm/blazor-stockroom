using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Stockroom.Web.Auditing;

/// <summary>
/// Hands the user of the circuit to code outside the circuit's
/// scope, such as the singleton that names the current user.
/// </summary>
public sealed class CircuitUserHandler(
    AuthenticationStateProvider authentication,
    CircuitUserAccessor accessor) : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task>
        CreateInboundActivityHandler(
            Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            var state =
                await authentication.GetAuthenticationStateAsync();
            accessor.User = state.User;
            await next(context);
            accessor.User = null;
        };
}
