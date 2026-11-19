using System.ComponentModel.DataAnnotations;

namespace Stockroom.Web.Hosting;

public sealed class StockroomOptions
{
    public const string SectionName = "Stockroom";

    [Required]
    public string BusinessTimeZone { get; init; } = "";

    [Required]
    [EmailAddress]
    public string SupportEmail { get; init; } = "";
}
