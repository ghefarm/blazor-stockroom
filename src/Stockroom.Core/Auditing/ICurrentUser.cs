namespace Stockroom.Core.Auditing;

/// <summary>
/// The name of the user for whom the current code runs.
/// </summary>
public interface ICurrentUser
{
    string GetName();
}
