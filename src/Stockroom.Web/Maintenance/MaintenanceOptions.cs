namespace Stockroom.Web.Maintenance;

public sealed class MaintenanceOptions
{
    public const string SectionName = "Maintenance";

    // The switch decides. The message may stay in the file.
    public bool Enabled { get; init; }

    public string Message { get; init; } = "";
}
