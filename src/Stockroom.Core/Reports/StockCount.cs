namespace Stockroom.Core.Reports;

// The value of all stock on the day somebody counted it.
public record StockCount(DateOnly CountedOn, long ValueCents);
