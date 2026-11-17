namespace Stockroom.Questions;

public sealed record Period(DateOnly From, DateOnly To)
{
    public static Period ForMonth(DateOnly day)
    {
        var first = new DateOnly(day.Year, day.Month, 1);
        return new Period(first, first.AddMonths(1).AddDays(-1));
    }

    public static Period ForYear(int year) =>
        new(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));
}
