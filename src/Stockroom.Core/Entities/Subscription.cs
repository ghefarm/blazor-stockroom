namespace Stockroom.Core.Entities;

/// <summary>
/// The paid period of this installation. The table has one row.
/// </summary>
public class Subscription
{
    public const int SingleId = 1;

    public int Id { get; set; } = SingleId;
    public DateOnly PaidUntil { get; set; }
}
