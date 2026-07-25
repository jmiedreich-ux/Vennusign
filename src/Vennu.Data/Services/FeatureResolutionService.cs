using Microsoft.Extensions.Caching.Memory;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class FeatureResolutionService : IFeatureResolutionService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);
    private readonly IFeatureRepository featureRepository;
    private readonly ISubscriptionTierRepository tierRepository;
    private readonly IVenueSubscriptionRepository subscriptionRepository;
    private readonly IVenueFeatureOverrideRepository overrideRepository;
    private readonly IMemoryCache cache;
    private readonly TimeProvider timeProvider;

    public FeatureResolutionService(
        IFeatureRepository featureRepository,
        ISubscriptionTierRepository tierRepository,
        IVenueSubscriptionRepository subscriptionRepository,
        IVenueFeatureOverrideRepository overrideRepository,
        IMemoryCache cache,
        TimeProvider timeProvider)
    {
        this.featureRepository = featureRepository;
        this.tierRepository = tierRepository;
        this.subscriptionRepository = subscriptionRepository;
        this.overrideRepository = overrideRepository;
        this.cache = cache;
        this.timeProvider = timeProvider;
    }

    public async Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default)
    {
        var entitlement = await GetFeatureAsync(venueId, featureKey, cancellationToken).ConfigureAwait(false);
        return entitlement?.Enabled == true;
    }

    public async Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureKey);
        var featureSet = await GetFeatureSetAsync(venueId, cancellationToken).ConfigureAwait(false);
        return featureSet.GetValueOrDefault(featureKey.Trim().ToLowerInvariant());
    }

    public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        return cache.GetOrCreateAsync(CacheKey(venueId), async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);
            return await ResolveAsync(venueId, cancellationToken).ConfigureAwait(false);
        })!;
    }

    public void Invalidate(Guid venueId) => cache.Remove(CacheKey(venueId));

    private async Task<IReadOnlyDictionary<string, FeatureEntitlement>> ResolveAsync(Guid venueId, CancellationToken cancellationToken)
    {
        var features = await featureRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var result = features.ToDictionary(
            feature => feature.Key,
            feature => new FeatureEntitlement(feature.Key, false, null, feature.IsActive ? "none" : "master-switch"),
            StringComparer.OrdinalIgnoreCase);

        var subscription = await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (subscription is not null && IsAccessStatus(subscription.Status))
        {
            var tierFeatures = await tierRepository.GetFeaturesAsync(subscription.TierId, cancellationToken).ConfigureAwait(false);
            foreach (var tierFeature in tierFeatures)
            {
                var feature = features.FirstOrDefault(item => item.Id == tierFeature.FeatureId);
                if (feature?.IsActive == true)
                {
                    result[feature.Key] = new FeatureEntitlement(feature.Key, true, tierFeature.LimitValue, "tier");
                }
            }
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var overrides = await overrideRepository.GetActiveByVenueAsync(venueId, utcNow, cancellationToken).ConfigureAwait(false);
        foreach (var featureOverride in overrides)
        {
            var feature = features.FirstOrDefault(item => item.Id == featureOverride.FeatureId);
            if (feature?.IsActive == true)
            {
                result[feature.Key] = new FeatureEntitlement(feature.Key, featureOverride.Enabled, null, "override");
            }
        }

        return result;
    }

    private static bool IsAccessStatus(string status) =>
        status.Equals("active", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("trialing", StringComparison.OrdinalIgnoreCase);

    private static string CacheKey(Guid venueId) => $"feature-set:{venueId:N}";
}
