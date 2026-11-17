using Microsoft.AspNetCore.Identity;

namespace Stockroom.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = "";

    // Set for a new account and after a reset by an administrator.
    public bool MustChangePassword { get; set; }

    // Accounts are archived, not deleted, so their history stays.
    public bool IsArchived { get; set; }
}
