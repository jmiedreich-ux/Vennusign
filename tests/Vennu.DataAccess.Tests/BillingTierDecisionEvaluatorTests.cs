using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class BillingTierDecisionEvaluatorTests
{
    [Fact]
    public void Evaluate_BlocksDowngradeWhenUsageExceedsTargetLimits()
    {
        var current = Tier("Current", 99m, maxScreens: 12, maxVenues: 5);
        var target = Tier("Starter", 49m, maxScreens: 2, maxVenues: 1);

        var result = BillingTierDecisionEvaluator.Evaluate(
            current,
            target,
            activeScreenCount: 4,
            organizationVenueCount: 3,
            ["Advanced themes", "Scheduling"]);

        Assert.Equal("downgrade", result.Direction);
        Assert.False(result.CanSelect);
        Assert.Contains("Archive 2 active screen(s) before selecting this plan.", result.BlockingReasons);
        Assert.Contains("Remove 2 venue(s) before selecting this plan.", result.BlockingReasons);
        Assert.Equal(["Advanced themes", "Scheduling"], result.LostFeatures);
    }

    [Fact]
    public void Evaluate_AllowsUpgradeWithinTargetLimits()
    {
        var current = Tier("Starter", 49m, maxScreens: 2, maxVenues: 1);
        var target = Tier("Growth", 99m, maxScreens: 10, maxVenues: 3);

        var result = BillingTierDecisionEvaluator.Evaluate(current, target, 2, 1);

        Assert.Equal("upgrade", result.Direction);
        Assert.True(result.CanSelect);
        Assert.Empty(result.BlockingReasons);
        Assert.Empty(result.LostFeatures);
    }

    [Fact]
    public void Evaluate_PreventsSelectingCurrentTier()
    {
        var tier = Tier("Growth", 99m, maxScreens: 10, maxVenues: 3);

        var result = BillingTierDecisionEvaluator.Evaluate(tier, tier, 1, 1);

        Assert.Equal("current", result.Direction);
        Assert.False(result.CanSelect);
        Assert.Equal(["This is the current plan."], result.BlockingReasons);
    }

    private static SubscriptionTier Tier(string name, decimal price, int maxScreens, int maxVenues) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Price = price,
        MaxScreens = maxScreens,
        MaxVenues = maxVenues,
        IsActive = true,
        IsPublic = true
    };
}
