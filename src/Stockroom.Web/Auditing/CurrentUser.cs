using Stockroom.Core.Auditing;

namespace Stockroom.Web.Auditing;

/// <summary>
/// The user of the current circuit activity, else the user of the
/// current HTTP request, else "system".
/// </summary>
public sealed class CurrentUser(
    CircuitUserAccessor circuit,
    IHttpContextAccessor http) : ICurrentUser
{
    // A static page or an endpoint has no circuit, but a request
    // with a valid HttpContext, so the accessor is safe there.
    public string GetName() =>
        AuditUser.GetName(circuit.User ?? http.HttpContext?.User);
}
