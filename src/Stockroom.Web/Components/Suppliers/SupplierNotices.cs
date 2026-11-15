using Stockroom.Web.Components.Shared;

namespace Stockroom.Web.Components.Suppliers;

// The address carries a short code, never the text. So nobody can
// put their own words into the banner with a link.
public static class SupplierNotices
{
    public static Notice? Find(string? code) => code switch
    {
        "created" => Succeed("The supplier was added."),
        "updated" => Succeed("The supplier was saved."),
        "added" => Succeed("The new suppliers were added."),
        "deleted" => Succeed("The supplier was deleted."),
        "deactivated" => Succeed("The suppliers are inactive."),
        "missing" => Fail("This supplier does not exist anymore."),
        _ => null,
    };

    private static Notice Succeed(string text) =>
        new(NoticeKind.Success, text);

    private static Notice Fail(string text) =>
        new(NoticeKind.Error, text);
}
