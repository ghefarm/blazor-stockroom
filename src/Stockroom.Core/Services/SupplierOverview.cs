namespace Stockroom.Core.Services;

/// <summary>
/// A supplier with its order numbers and the products it supplied.
/// </summary>
public sealed record SupplierOverview(
    string Name,
    IReadOnlyList<string> OrderNumbers,
    IReadOnlyList<string> ProductNames);
