namespace Stockroom.Web.Components.Account;

public static class AccountFeatures
{
    // Not const: the compiler folds a constant and then warns about
    // the code it cannot reach.
    public static readonly bool Passkeys = true;
}
