namespace Stockroom.Core.Entities;

/// <summary>
/// Who created a row and who changed it last, and when. Filled
/// on every save, never by hand.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    string CreatedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
