using Microsoft.Extensions.Caching.Memory;
using Vennu.Core.Models;
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
    private readonly IVenueRepository? venueRepository;
    private readonly IOrganizationSubscriptionRepository? organizationSubscriptionRepository;

    public FeatureResolutionService(
        IFeatureRepository featureRepository,
        ISubscriptionTierRepository tierRepository,
        IVenueSubscriptionRepository subscriptionRepository,
        IVenueFeatureOverrideRepository overrideRepository,
        IMemoryCache cache,
        TimeProvider timeProvider,
        IVenueRepository? venueRepository = null,
        IOrganizationSubscriptionRepository? organizationSubscriptionRepository = null)
    {
        this.featureRepository = featureRepository;
        this.tierRepository = tierRepository;
        this.subscriptionRepository = subscriptionRepository;
        this.overrideRepository = overrideRepository;
        this.cache = cache;
        this.timeProvider = timeProvider;
        this.venueRepository = venueRepository;
        this.organizationSubscriptionRepository = organizationSubscriptionRepository;
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

        var organizationSubscription = await GetOrganizationSubscriptionAsync(venueId, cancellationToken).ConfigureAwait(false);
        var venueSubscription = organizationSubscription is null
            ? await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            : null;
        var tierId = organizationSubscription?.TierId ?? venueSubscription?.TierId;
        var status = organizationSubscription?.Status ?? venueSubscription?.Status;
        var trialEndsAt = organizationSubscription?.TrialEndsAt ?? venueSubscription?.TrialEndsAt;
        if (tierId is not null && IsAccessStatus(status, trialEndsAt, timeProvider.GetUtcNow().UtcDateTime))
        {
            var tierFeatures = await tierRepository.GetFeaturesAsync(tierId.Value, cancellationToken).ConfigureAwait(false);
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

    private async Task<OrganizationSubscription?> GetOrganizationSubscriptionAsync(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        if (venueRepository is null || organizationSubscriptionRepository is null) return null;
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        return venue?.OrganizationId is Guid organizationId
            ? await organizationSubscriptionRepository.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            : null;
    }

    private static bool IsAccessStatus(string? status, DateTime? trialEndsAt, DateTime utcNow) =>
        status?.Equals("active", StringComparison.OrdinalIgnoreCase) == true ||
        status?.Equals("trialing", StringComparison.OrdinalIgnoreCase) == true && trialEndsAt > utcNow;

    private static string CacheKey(Guid venueId) => $"feature-set:{venueId:N}";
}
