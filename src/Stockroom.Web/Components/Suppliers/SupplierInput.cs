using System.ComponentModel.DataAnnotations;
using Stockroom.Core.Entities;

namespace Stockroom.Web.Components.Suppliers;

// What the supplier form posts. It has no Id, so a hand-made POST
// cannot choose which record it changes.
public class SupplierInput
{
    [Required, StringLength(80)]
    public string? Name { get; set; }

    [Required, EmailAddress, StringLength(120)]
    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public static SupplierInput FromSupplier(Supplier supplier) =>
        new()
        {
            Name = supplier.Name,
            Email = supplier.Email,
            IsActive = supplier.IsActive,
        };

    public void CopyTo(Supplier supplier)
    {
        supplier.Name = Name?.Trim() ?? "";
        supplier.Email = Email?.Trim() ?? "";
        supplier.IsActive = IsActive;
    }
}
