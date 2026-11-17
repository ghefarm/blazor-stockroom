namespace Stockroom.Core.Entities;

public class Supplier : IAuditable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}
