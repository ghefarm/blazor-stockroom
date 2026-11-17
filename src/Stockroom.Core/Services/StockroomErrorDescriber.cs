using Microsoft.AspNetCore.Identity;

namespace Stockroom.Core.Services;

/// <summary>
/// Stockroom's own wording for Identity errors. The code of each
/// error stays, only the description changes.
/// </summary>
public class StockroomErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateUserName(string userName)
    {
        var error = base.DuplicateUserName(userName);
        error.Description =
            $"The user name '{userName}' is already in use.";
        return error;
    }

    public override IdentityError PasswordTooShort(int length)
    {
        var error = base.PasswordTooShort(length);
        error.Description =
            $"A password needs at least {length} characters.";
        return error;
    }

    public override IdentityError PasswordMismatch()
    {
        var error = base.PasswordMismatch();
        error.Description = "The current password is not correct.";
        return error;
    }
}
