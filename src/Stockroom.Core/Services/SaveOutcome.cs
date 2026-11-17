namespace Stockroom.Core.Services;

/// <summary>
/// What happened to a change that the user sent from a form.
/// </summary>
public enum SaveOutcome
{
    Saved,
    NotFound,
    Conflict,
}
