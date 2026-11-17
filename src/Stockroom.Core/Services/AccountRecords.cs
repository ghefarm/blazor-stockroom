namespace Stockroom.Core.Services;

public sealed record NewAccount(
    string UserName, string DisplayName, IReadOnlyList<string> Roles);

/// <summary>
/// One row of the user list. The page never sees the Identity user.
/// </summary>
public sealed record AccountSummary(
    string Id,
    string UserName,
    string DisplayName,
    IReadOnlyList<string> Roles,
    bool MustChangePassword,
    bool IsArchived);

/// <summary>
/// The result of a change to an account. A new password is returned
/// once, here, and can never be read again.
/// </summary>
public sealed record AccountResult(
    bool Succeeded, IReadOnlyList<string> Errors, string? Password)
{
    public static AccountResult Ok(string? password = null) =>
        new(true, [], password);

    public static AccountResult Fail(params string[] errors) =>
        new(false, errors, null);
}
