using System.Security.AccessControl;
using System.Security.Principal;

namespace Stockroom.Deploy.Tests;

// Works on a folder of its own, created for each test and deleted
// after it.
public class FolderRightsTests
{
    private string folder = "";

    [SetUp]
    public void CreateFolder()
    {
        folder = Path.Combine(AppContext.BaseDirectory,
            "rights-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
    }

    [TearDown]
    public void DeleteFolder()
    {
        // Back to inherited rights, so the folder can be deleted.
        var inherited = new DirectorySecurity();
        inherited.SetAccessRuleProtection(false, false);
        new DirectoryInfo(folder).SetAccessControl(inherited);
        Directory.Delete(folder);
    }

    [Test]
    public void CurrentUser_IsResolved_AndHasModify()
    {
        var me = WindowsIdentity.GetCurrent();

        var sid = FolderRights.ResolveAccount(me.Name, TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(sid, Is.EqualTo(me.User));
            Assert.That(FolderRights.HasModify(folder, sid));
        });
    }

    [Test]
    public void UnknownAccount_Throws_AndLocalServiceHasNoModify()
    {
        var localService = new SecurityIdentifier(
            WellKnownSidType.LocalServiceSid, null);
        var unknown = Environment.MachineName + @"\no-such-user-ch30";

        Assert.Multiple(() =>
        {
            Assert.That(FolderRights.HasModify(folder, localService),
                Is.False);
            Assert.Throws<IdentityNotMappedException>(() =>
                FolderRights.ResolveAccount(unknown,
                    TimeSpan.FromSeconds(1)));
        });
    }

    [Test]
    public void ProtectProgramFolder_SetsThreeRules_NoInheritance()
    {
        FolderRights.ProtectProgramFolder(folder);

        var security = new DirectoryInfo(folder).GetAccessControl();
        var rights = security
            .GetAccessRules(true, true, typeof(SecurityIdentifier))
            .Cast<FileSystemAccessRule>()
            .ToDictionary(
                rule => rule.IdentityReference.Value,
                rule => rule.FileSystemRights);
        var admins = Sid(WellKnownSidType.BuiltinAdministratorsSid);

        Assert.Multiple(() =>
        {
            Assert.That(security.AreAccessRulesProtected);
            Assert.That(rights, Has.Count.EqualTo(3));
            Assert.That(
                rights[admins],
                Is.EqualTo(FileSystemRights.FullControl));
            Assert.That(
                rights[Sid(WellKnownSidType.LocalSystemSid)],
                Is.EqualTo(FileSystemRights.FullControl));

            var users = rights[Sid(WellKnownSidType.BuiltinUsersSid)];
            Assert.That(
                users.HasFlag(FileSystemRights.ReadAndExecute));
            Assert.That(
                users.HasFlag(FileSystemRights.Write), Is.False);
        });
    }

    private static string Sid(WellKnownSidType type) =>
        new SecurityIdentifier(type, null).Value;
}
