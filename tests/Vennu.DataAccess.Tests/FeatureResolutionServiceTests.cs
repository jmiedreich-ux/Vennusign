using Microsoft.Extensions.Caching.Memory;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public class FeatureResolutionServiceTests
{
    [Fact]
    public async Task Override_disables_tier_feature()
    {
        var venueId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var service = CreateService(
            venueId,
            new Feature { Id = featureId, Key = "happy_hour", IsActive = true },
            new VenueSubscription { VenueId = venueId, TierId = tierId, Status = "active" },
            new TierFeature { TierId = tierId, FeatureId = featureId },
            new VenueFeatureOverride { VenueId = venueId, FeatureId = featureId, Enabled = false, Reason = "Custom contract" });

        var entitlement = await service.GetFeatureAsync(venueId, "happy_hour");

        Assert.NotNull(entitlement);
        Assert.False(entitlement.Enabled);
        Assert.Equal("override", entitlement.Source);
    }

    [Fact]
    public async Task Inactive_master_feature_cannot_be_enabled_by_override()
    {
        var venueId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var service = CreateService(
            venueId,
            new Feature { Id = featureId, Key = "pos_integration", IsActive = false },
            null,
            null,
            new VenueFeatureOverride { VenueId = venueId, FeatureId = featureId, Enabled = true, Reason = "Beta" });

        Assert.False(await service.HasFeatureAsync(venueId, "pos_integration"));
    }

    [Fact]
    public async Task Tier_limit_is_returned_for_active_subscription()
    {
        var venueId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var featureId = Guid.NewGuid();
        var service = CreateService(
            venueId,
            new Feature { Id = featureId, Key = "ai_translation", IsActive = true },
            new VenueSubscription { VenueId = venueId, TierId = tierId, Status = "trialing" },
            new TierFeature { TierId = tierId, FeatureId = featureId, LimitValue = "1" },
            null);

        var entitlement = await service.GetFeatureAsync(venueId, "ai_translation");

        Assert.True(entitlement!.Enabled);
        Assert.Equal("1", entitlement.LimitValue);
        Assert.Equal("tier", entitlement.Source);
    }

    private static FeatureResolutionService CreateService(
        Guid venueId,
        Feature feature,
        VenueSubscription? subscription,
        TierFeature? tierFeature,
        VenueFeatureOverride? featureOverride)
    {
        return new FeatureResolutionService(
            new FeatureRepositoryFake(feature),
            new TierRepositoryFake(tierFeature),
            new SubscriptionRepositoryFake(subscription),
            new OverrideRepositoryFake(featureOverride),
            new MemoryCache(new MemoryCacheOptions()),
            TimeProvider.System);
    }

    private sealed class FeatureRepositoryFake(Feature feature) : IFeatureRepository
    {
        public Task<IReadOnlyCollection<Feature>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Feature>>(new[] { feature });
        public Task<Feature?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<Feature?>(feature.Key == key ? feature : null);
    }

    private sealed class TierRepositoryFake(TierFeature? tierFeature) : ISubscriptionTierRepository
    {
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<SubscriptionTier>>(Array.Empty<SubscriptionTier>());
        public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.FromResult<SubscriptionTier?>(null);
        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TierFeature>>(tierFeature is null ? Array.Empty<TierFeature>() : new[] { tierFeature });
    }

    private sealed class SubscriptionRepositoryFake(VenueSubscription? subscription) : IVenueSubscriptionRepository
    {
        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VenueSubscription>>(subscription is null ? Array.Empty<VenueSubscription>() : new[] { subscription });

        public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult(subscription);
        public Task<bool> SaveAsync(VenueSubscription value, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class OverrideRepositoryFake(VenueFeatureOverride? featureOverride) : IVenueFeatureOverrideRepository
    {
        public Task<IReadOnlyCollection<VenueFeatureOverride>> GetActiveByVenueAsync(Guid venueId, DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<VenueFeatureOverride>>(featureOverride is null ? Array.Empty<VenueFeatureOverride>() : new[] { featureOverride });
        public Task UpsertAsync(VenueFeatureOverride value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
