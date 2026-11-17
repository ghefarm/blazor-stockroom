using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

/// <summary>
/// A part of the product list and the size of the whole list.
/// </summary>
public sealed record ProductSlice(
    IReadOnlyList<Product> Items,
    int TotalCount);
