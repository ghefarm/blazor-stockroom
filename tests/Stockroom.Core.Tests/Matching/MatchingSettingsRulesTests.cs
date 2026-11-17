using Stockroom.Core.Entities;
using Stockroom.Core.Matching;

namespace Stockroom.Core.Tests.Matching;

public class MatchingSettingsRulesTests
{
    [Test]
    public void Validate_AcceptsDefaults()
    {
        var errors = MatchingSettingsRules.Validate(
            new MatchingSettings());

        Assert.That(errors, Is.Empty);
    }

    [Test]
    public void Validate_ReportsEveryBrokenRule()
    {
        var settings = new MatchingSettings
        {
            AutoThreshold = 70,
            SuggestThreshold = 75,
            MinimumLead = -1,
            DaysAfterOrder = 7,
        };

        var errors = MatchingSettingsRules.Validate(settings);

        Assert.That(errors, Is.EqualTo(
        [
            "Minimum lead must be between 0 and 100.",
            "Days after order must be between 14 and 365.",
            "Suggest threshold is above auto threshold.",
        ]));
    }
}
