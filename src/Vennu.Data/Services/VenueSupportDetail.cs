using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed record VenueSupportDetail(
    Venue Venue,
    VenueSubscription? Subscription,
    SubscriptionTier? Tier,
    IReadOnlyCollection<Screen> Screens,
    IReadOnlyDictionary<string, FeatureEntitlement> Features,
    IReadOnlyCollection<VenueFeatureOverride> ActiveOverrides);
