using Stockroom.Core.Entities;

namespace Stockroom.Core.Matching;

public sealed record SettingRange(string Label, int Min, int Max)
{
    public bool Contains(int value) => value >= Min && value <= Max;

    public string? Check(int value) => Contains(value)
        ? null
        : $"{Label} must be between {Min} and {Max}.";
}

// The one place for valid ranges. The service and the form
// both read them, so there is no second truth in markup.
public static class MatchingSettingsRules
{
    public static readonly SettingRange AutoThreshold =
        new("Auto threshold", 1, MatchWeights.MaxScore);
    public static readonly SettingRange SuggestThreshold =
        new("Suggest threshold", 1, MatchWeights.MaxScore);
    public static readonly SettingRange MinimumLead =
        new("Minimum lead", 0, MatchWeights.MaxScore);
    public static readonly SettingRange DaysAfterOrder =
        new("Days after order", MatchWeights.SoonDays, 365);

    public static IReadOnlyList<string> Validate(
        MatchingSettings settings)
    {
        string?[] checks =
        [
            AutoThreshold.Check(settings.AutoThreshold),
            SuggestThreshold.Check(settings.SuggestThreshold),
            MinimumLead.Check(settings.MinimumLead),
            DaysAfterOrder.Check(settings.DaysAfterOrder),
            settings.SuggestThreshold > settings.AutoThreshold
                ? "Suggest threshold is above auto threshold."
                : null,
        ];

        return checks.OfType<string>().ToList();
    }
}
