namespace Stockroom.Core.Reports;

// ValueCents is null when nobody counted in that month. Zero would
// claim that the warehouse was empty.
public record MonthValue(DateOnly Month, long? ValueCents);
