namespace Stockroom.Core.Entities;

// Stored as a number. Add new values at the end and never change
// the order, or the rows in the database change their meaning.
public enum OrderStatus
{
    Open,
    PartlyDelivered,
    Delivered,
    Cancelled,
}
