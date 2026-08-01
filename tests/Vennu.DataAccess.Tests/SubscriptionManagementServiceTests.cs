using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public class SubscriptionManagementServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 28, 5, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Start_trial_uses_tier_defined_duration_and_invalidates_cache()
    {
        var venueId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var repository = new SubscriptionRepositoryFake();
        var featureResolution = new FeatureResolutionFake();
        var service = CreateService(repository, featureResolution);

        var subscription = await service.StartTrialAsync(venueId, tierId);

        Assert.Equal("trialing", subscription.Status);
        Assert.Equal(UtcNow.UtcDateTime.AddDays(21), subscription.TrialEndsAt);
        Assert.Equal(tierId, subscription.TierId);
        Assert.Equal(venueId, Assert.Single(featureResolution.InvalidatedVenueIds));
    }

    [Fact]
    public async Task Change_tier_preserves_lifecycle_status()
    {
        var venueId = Guid.NewGuid();
        var originalTierId = Guid.NewGuid();
        var newTierId = Guid.NewGuid();
        var repository = new SubscriptionRepositoryFake(new VenueSubscription
        {
            VenueId = venueId,
            TierId = originalTierId,
            Status = "past_due"
        });
        var featureResolution = new FeatureResolutionFake();
        var service = CreateService(repository, featureResolution);

        var subscription = await service.ChangeTierAsync(venueId, newTierId);

        Assert.Equal(newTierId, subscription.TierId);
        Assert.Equal("past_due", subscription.Status);
        Assert.Equal(venueId, Assert.Single(featureResolution.InvalidatedVenueIds));
    }

    [Fact]
    public async Task Unsupported_status_is_rejected()
    {
        var service = CreateService(new SubscriptionRepositoryFake(), new FeatureResolutionFake());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.SetStatusAsync(Guid.NewGuid(), "paused"));
    }

    [Fact]
    public async Task Expire_trials_cancels_only_elapsed_trials()
    {
        var expiredVenueId = Guid.NewGuid();
        var futureVenueId = Guid.NewGuid();
        var repository = new SubscriptionRepositoryFake(
            new VenueSubscription
            {
                VenueId = expiredVenueId,
                TierId = Guid.NewGuid(),
                Status = "trialing",
                TrialEndsAt = UtcNow.UtcDateTime.AddMinutes(-1)
            },
            new VenueSubscription
            {
                VenueId = futureVenueId,
                TierId = Guid.NewGuid(),
                Status = "trialing",
                TrialEndsAt = UtcNow.UtcDateTime.AddMinutes(1)
            });
        var featureResolution = new FeatureResolutionFake();
        var service = CreateService(repository, featureResolution);

        var count = await service.ExpireTrialsAsync();

        Assert.Equal(1, count);
        Assert.Equal("canceled", repository.Items.Single(item => item.VenueId == expiredVenueId).Status);
        Assert.Equal("trialing", repository.Items.Single(item => item.VenueId == futureVenueId).Status);
        Assert.Equal(expiredVenueId, Assert.Single(featureResolution.InvalidatedVenueIds));
    }

    private static SubscriptionManagementService CreateService(
        SubscriptionRepositoryFake repository,
        FeatureResolutionFake featureResolution) =>
        new(repository, featureResolution, new TierRepositoryFake(), new FixedTimeProvider(UtcNow));

    private sealed class TierRepositoryFake : ISubscriptionTierRepository
    {
        public Task<SubscriptionTier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<SubscriptionTier?>(new() { Id=id, IsActive=true, IsPublic=true, TrialDays=21 });
        public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.FromResult<SubscriptionTier?>(null);
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<SubscriptionTier>>([]);
        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
        public Task<bool> CreateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> UpdateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class SubscriptionRepositoryFake(params VenueSubscription[] subscriptions) : IVenueSubscriptionRepository
    {
        public List<VenueSubscription> Items { get; } = subscriptions.ToList();

        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VenueSubscription>>(Items.ToArray());

        public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(item => item.VenueId == venueId));

        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(
            string stripeSubscriptionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(item => item.StripeSubscriptionId == stripeSubscriptionId));

        public Task<bool> SaveAsync(VenueSubscription subscription, CancellationToken cancellationToken = default)
        {
            var existing = Items.FindIndex(item => item.VenueId == subscription.VenueId);
            if (existing >= 0)
            {
                Items[existing] = subscription;
            }
            else
            {
                Items.Add(subscription);
            }

            return Task.FromResult(true);
        }
    }

    private sealed class FeatureResolutionFake : IFeatureResolutionService
    {
        public List<Guid> InvalidatedVenueIds { get; } = new();

        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<FeatureEntitlement?>(null);

        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(new Dictionary<string, FeatureEntitlement>());

        public void Invalidate(Guid venueId) => InvalidatedVenueIds.Add(venueId);
    }
}
