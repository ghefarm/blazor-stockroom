namespace Stockroom.Core.Tests.Lifetimes;

// Scoped: one signed-in user per circuit.
public sealed class SignedInUser
{
    public string Name { get; set; } = "";
}

// Wrong on purpose: a singleton that holds a scoped service.
public sealed class ChangeLog(SignedInUser user)
{
    public string Describe(string change)
    {
        return $"{change} by {user.Name}";
    }
}
