using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed record FeatureMatrixSnapshot(
    IReadOnlyCollection<SubscriptionTier> Tiers,
    IReadOnlyCollection<Feature> Features,
    IReadOnlyCollection<TierFeature> EnabledFeatures,
    IReadOnlyCollection<FeatureMatrixAuditEntry> RecentAudit);
