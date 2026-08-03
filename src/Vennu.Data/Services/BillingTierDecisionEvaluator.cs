using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed record BillingTierDecision(
    string Direction,
    bool CanSelect,
    IReadOnlyCollection<string> BlockingReasons,
    IReadOnlyCollection<string> LostFeatures);

public static class BillingTierDecisionEvaluator
{
    public static BillingTierDecision Evaluate(
        SubscriptionTier? currentTier,
        SubscriptionTier targetTier,
        int activeScreenCount,
        int organizationVenueCount,
        IReadOnlyCollection<string>? lostFeatures = null)
    {
        ArgumentNullException.ThrowIfNull(targetTier);
        if (activeScreenCount < 0) throw new ArgumentOutOfRangeException(nameof(activeScreenCount));
        if (organizationVenueCount < 0) throw new ArgumentOutOfRangeException(nameof(organizationVenueCount));

        var direction = currentTier is null
            ? "start"
            : currentTier.Id == targetTier.Id
                ? "current"
                : targetTier.Price >= currentTier.Price
                    ? "upgrade"
                    : "downgrade";
        var reasons = new List<string>();
        if (direction == "current") reasons.Add("This is the current plan.");
        if (targetTier.MaxScreens >= 0 && activeScreenCount > targetTier.MaxScreens)
            reasons.Add($"Archive {activeScreenCount - targetTier.MaxScreens} active screen(s) before selecting this plan.");
        if (targetTier.MaxVenues >= 0 && organizationVenueCount > targetTier.MaxVenues)
            reasons.Add($"Remove {organizationVenueCount - targetTier.MaxVenues} venue(s) before selecting this plan.");

        return new BillingTierDecision(
            direction,
            reasons.Count == 0,
            reasons,
            (lostFeatures ?? []).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray());
    }
}
