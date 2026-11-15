namespace Stockroom.Core.Services;

public sealed record PriceChange(string Sku, long PriceCents);

/// <summary>
/// How many rows a batch changed, and how many it left alone.
/// </summary>
public sealed record BatchResult(int Changed, int Skipped);
