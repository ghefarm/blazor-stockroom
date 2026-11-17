namespace Stockroom.Core.Entities;

public class DeliveryMatch
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }
    public int PurchaseOrderId { get; set; }
    public int Score { get; set; }
    public MatchStatus Status { get; set; }
    public MatchDecision Decision { get; set; }

    // The explanation is built from these, never from a second run.
    public int AmountPoints { get; set; }
    public int SupplierPoints { get; set; }
    public int DatePoints { get; set; }
    public int ReferencePoints { get; set; }
}

public enum MatchStatus
{
    Automatic,
    Suggested,
    Ambiguous,
}

public enum MatchDecision
{
    Open,
    Confirmed,
    Rejected,
}
