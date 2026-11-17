using Microsoft.AspNetCore.Identity;

namespace Stockroom.Core.Services;

/// <summary>
/// The Identity options of Stockroom, in one place for the app and
/// the tests.
/// </summary>
public static class AccountRules
{
    public static void Apply(IdentityOptions options)
    {
        // Identity schema version 3 adds the table for passkeys.
        options.Stores.SchemaVersion =
            IdentitySchemaVersions.Version3;

        // Stockroom sends no email, so nobody could confirm one.
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        // Long enough against guessing, short enough that a typing
        // mistake needs no administrator.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    }
}
