namespace Stockroom.Core;

/// <summary>
/// The role names. Pages, the menu and endpoints all use these
/// constants, so a typing mistake fails at build time.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Purchasing = "Purchasing";
    public const string Warehouse = "Warehouse";
    public const string Reports = "Reports";

    // For [Authorize(Roles = ...)], AuthorizeView and endpoints. A
    // comma means "any of these". The administrator is always in.
    public const string PurchasingOrAdmin = Purchasing + "," + Admin;

    public const string StockAreasOrAdmin =
        Warehouse + "," + Purchasing + "," + Admin;

    public const string ReportsOrAdmin = Reports + "," + Admin;

    public static IReadOnlyList<string> All { get; } =
        [Admin, Purchasing, Warehouse, Reports];
}
