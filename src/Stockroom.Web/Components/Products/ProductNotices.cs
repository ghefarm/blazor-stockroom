using Stockroom.Web.Components.Shared;

namespace Stockroom.Web.Components.Products;

// Same idea as SupplierNotices: the address carries a code only.
public static class ProductNotices
{
    public static Notice? Find(string? code) => code switch
    {
        "counted" => new(NoticeKind.Success,
            "The stock count was saved."),
        "priced" => new(NoticeKind.Success,
            "The new price was saved."),
        _ => null,
    };
}
