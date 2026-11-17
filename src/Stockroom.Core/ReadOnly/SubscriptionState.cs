namespace Stockroom.Core.ReadOnly;

/// <summary>
/// The paid period in memory, so every save can check it without a
/// query. Loaded at startup and changed when a subscription is saved.
/// </summary>
public sealed class SubscriptionState(BusinessClock clock)
{
    private readonly Lock gate = new();
    private DateOnly? paidUntil;

    public DateOnly? PaidUntil
    {
        get
        {
            lock (gate)
            {
                return paidUntil;
            }
        }
    }

    // Null until the state is loaded: then nobody writes.
    public bool AllowsWrites =>
        PaidUntil is { } until && clock.Today <= until;

    public void Set(DateOnly? until)
    {
        lock (gate)
        {
            paidUntil = until;
        }
    }
}
