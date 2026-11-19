using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Stockroom.Deploy;

public static class FolderRights
{
    private const InheritanceFlags ToAll =
        InheritanceFlags.ContainerInherit
        | InheritanceFlags.ObjectInherit;

    /// <summary>
    /// Rights like a folder under Program Files, with inheritance
    /// from the parent folder switched off.
    /// </summary>
    public static void ProtectProgramFolder(string path)
    {
        var security = new DirectorySecurity();
        security.SetAccessRuleProtection(true, false);
        security.AddAccessRule(CreateRule(
            WellKnownSidType.BuiltinAdministratorsSid,
            FileSystemRights.FullControl));
        security.AddAccessRule(CreateRule(
            WellKnownSidType.LocalSystemSid,
            FileSystemRights.FullControl));
        security.AddAccessRule(CreateRule(
            WellKnownSidType.BuiltinUsersSid,
            FileSystemRights.ReadAndExecute));
        new DirectoryInfo(path).SetAccessControl(security);
    }

    // A grant that is already there would still rewrite the right
    // into every file below, which costs seconds on a big folder.
    public static bool HasModify(string path, SecurityIdentifier sid)
    {
        var rules = new DirectoryInfo(path).GetAccessControl()
            .GetAccessRules(true, true, typeof(SecurityIdentifier));
        foreach (FileSystemAccessRule rule in rules)
        {
            if (rule.AccessControlType == AccessControlType.Allow
                && rule.IdentityReference == sid
                && rule.InheritanceFlags == ToAll
                && rule.FileSystemRights.HasFlag(
                    FileSystemRights.Modify))
            {
                return true;
            }
        }

        return false;
    }

    // A new application pool identity can take some seconds
    // before Windows resolves its name.
    public static SecurityIdentifier ResolveAccount(
        string account, TimeSpan waitFor)
    {
        var clock = Stopwatch.StartNew();
        while (true)
        {
            try
            {
                return (SecurityIdentifier)new NTAccount(account)
                    .Translate(typeof(SecurityIdentifier));
            }
            catch (IdentityNotMappedException)
                when (clock.Elapsed < waitFor)
            {
                Thread.Sleep(1000);
            }
        }
    }

    private static FileSystemAccessRule CreateRule(
        WellKnownSidType type, FileSystemRights rights) =>
        new(new SecurityIdentifier(type, null), rights, ToAll,
            PropagationFlags.None, AccessControlType.Allow);
}
