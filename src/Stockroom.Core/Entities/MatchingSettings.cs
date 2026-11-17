namespace Stockroom.Core.Entities;

public class MatchingSettings
{
    public const int SingleRowId = 1;

    public int Id { get; set; } = SingleRowId;
    public int AutoThreshold { get; set; } = 80;
    public int SuggestThreshold { get; set; } = 50;
    public int MinimumLead { get; set; } = 10;
    public int DaysAfterOrder { get; set; } = 30;
}
