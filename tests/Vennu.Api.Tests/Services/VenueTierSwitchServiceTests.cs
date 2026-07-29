using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class VenueTierSwitchServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 28, 23, 55, 0, TimeSpan.Zero);

    [Fact]
    public async Task SwitchAsync_UpdatesStripeBeforeLocalStateAndRecordsUpgrade()
    {
        var venue = new Venue { Id = Guid.NewGuid(), Name = "Harbor Cafe" };
        var current = Tier("Starter", 29m, "price_starter");
        var target = Tier("Pro", 89m, "price_pro");
        var subscription = new VenueSubscription
        {
            VenueId = venue.Id,
            TierId = current.Id,
            StripeSubscriptionId = "sub_1",
            Status = "active"
        };
        var updater = new StripeUpdaterFake();
        var subscriptions = new SubscriptionRepositoryFake(subscription);
        var events = new EventRepositoryFake();
        var features = new FeatureResolutionFake();
        var service = CreateService(venue, subscription, [current, target], updater, subscriptions, events, features);

        var result = await service.SwitchAsync(venue.Id, target.Id);

        Assert.Equal(target.Id, result.TierId);
        Assert.Equal("sub_1", updater.SubscriptionId);
        Assert.Equal("price_pro", updater.MonthlyPriceId);
        Assert.Equal(1, subscriptions.SaveCalls);
        Assert.Equal(venue.Id, Assert.Single(features.InvalidatedVenueIds));
        Assert.Equal("upgrade", Assert.Single(events.Items).EventType);
    }

    [Fact]
    public async Task SwitchAsync_DoesNotMutateLocalState_WhenStripeFails()
    {
        var venue = new Venue { Id = Guid.NewGuid() };
        var current = Tier("Starter", 29m, "price_starter");
        var target = Tier("Pro", 89m, "price_pro");
        var subscription = Subscription(venue.Id, current.Id);
        var subscriptions = new SubscriptionRepositoryFake(subscription);
        var updater = new StripeUpdaterFake { Failure = new InvalidOperationException("Stripe unavailable") };
        var service = CreateService(
            venue,
            subscription,
            [current, target],
            updater,
            subscriptions,
            new EventRepositoryFake(),
            new FeatureResolutionFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SwitchAsync(venue.Id, target.Id));

        Assert.Equal(current.Id, subscription.TierId);
        Assert.Equal(0, subscriptions.SaveCalls);
    }

    [Fact]
    public async Task SwitchAsync_RestoresStripe_WhenLocalPersistenceFails()
    {
        var venue = new Venue { Id = Guid.NewGuid() };
        var current = Tier("Starter", 29m, "price_starter");
        var target = Tier("Pro", 89m, "price_pro");
        var subscription = Subscription(venue.Id, current.Id);
        var subscriptions = new SubscriptionRepositoryFake(subscription) { SaveResult = false };
        var updater = new StripeUpdaterFake();
        var service = CreateService(
            venue,
            subscription,
            [current, target],
            updater,
            subscriptions,
            new EventRepositoryFake(),
            new FeatureResolutionFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SwitchAsync(venue.Id, target.Id));

        Assert.Equal(current.Id, subscription.TierId);
        Assert.True(updater.Restored);
    }

    [Fact]
    public async Task SwitchAsync_RestoresLocalStateAndStripe_WhenEventRecordingFails()
    {
        var venue = new Venue { Id = Guid.NewGuid() };
        var current = Tier("Starter", 29m, "price_starter");
        var target = Tier("Pro", 89m, "price_pro");
        var subscription = Subscription(venue.Id, current.Id);
        var subscriptions = new SubscriptionRepositoryFake(subscription);
        var updater = new StripeUpdaterFake();
        var service = CreateService(
            venue,
            subscription,
            [current, target],
            updater,
            subscriptions,
            new EventRepositoryFake { Failure = new InvalidOperationException("Event store unavailable") },
            new FeatureResolutionFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SwitchAsync(venue.Id, target.Id));

        Assert.Equal(current.Id, subscription.TierId);
        Assert.Equal(2, subscriptions.SaveCalls);
        Assert.True(updater.Restored);
    }

    [Fact]
    public async Task SwitchAsync_RejectsArchivedTargetBeforeStripe()
    {
        var venue = new Venue { Id = Guid.NewGuid() };
        var current = Tier("Starter", 29m, "price_starter");
        var target = Tier("Legacy", 19m, "price_legacy");
        target.IsActive = false;
        var subscription = Subscription(venue.Id, current.Id);
        var updater = new StripeUpdaterFake();
        var service = CreateService(
            venue,
            subscription,
            [current, target],
            updater,
            new SubscriptionRepositoryFake(subscription),
            new EventRepositoryFake(),
            new FeatureResolutionFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SwitchAsync(venue.Id, target.Id));

        Assert.Null(updater.SubscriptionId);
    }

    private static VenueTierSwitchService CreateService(
        Venue venue,
        VenueSubscription subscription,
        IReadOnlyCollection<SubscriptionTier> tiers,
        StripeUpdaterFake updater,
        SubscriptionRepositoryFake subscriptions,
        EventRepositoryFake events,
        FeatureResolutionFake features) =>
        new(
            new FakeVenueRepository
            {
                GetByIdAsyncHandler = (id, _) => Task.FromResult<Venue?>(id == venue.Id ? venue : null)
            },
            subscriptions,
            new TierRepositoryFake(tiers),
            updater,
            events,
            features,
            new FixedTimeProvider());

    private static SubscriptionTier Tier(string name, decimal price, string monthlyPriceId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = name.ToLowerInvariant(),
            Price = price,
            IsActive = true,
            StripeMonthlyPriceId = monthlyPriceId,
            StripeAnnualPriceId = $"{monthlyPriceId}_annual"
        };

    private static VenueSubscription Subscription(Guid venueId, Guid tierId) =>
        new()
        {
            VenueId = venueId,
            TierId = tierId,
            StripeSubscriptionId = "sub_1",
            Status = "active"
        };

    private sealed class SubscriptionRepositoryFake(VenueSubscription subscription) : IVenueSubscriptionRepository
    {
        public int SaveCalls { get; private set; }
        public bool SaveResult { get; set; } = true;
        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VenueSubscription>>([subscription]);
        public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<VenueSubscription?>(subscription.VenueId == venueId ? subscription : null);
        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
            Task.FromResult<VenueSubscription?>(subscription.StripeSubscriptionId == stripeSubscriptionId ? subscription : null);
        public Task<bool> SaveAsync(VenueSubscription value, CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(SaveResult);
        }
    }

    private sealed class TierRepositoryFake(IReadOnlyCollection<SubscriptionTier> tiers) : ISubscriptionTierRepository
    {
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(tiers);
        public Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default) =>
            Task.FromResult(tiers.FirstOrDefault(item => item.Id == tierId));
        public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(tiers.FirstOrDefault(item => item.Slug == slug));
        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
        public Task<bool> CreateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class StripeUpdaterFake : IStripeSubscriptionTierUpdater
    {
        public string? SubscriptionId { get; private set; }
        public string? MonthlyPriceId { get; private set; }
        public Exception? Failure { get; init; }
        public bool Restored { get; private set; }

        public Task<StripeSubscriptionTierChange> ChangeAsync(
            string stripeSubscriptionId,
            string monthlyPriceId,
            string? annualPriceId,
            CancellationToken cancellationToken = default)
        {
            SubscriptionId = stripeSubscriptionId;
            MonthlyPriceId = monthlyPriceId;
            if (Failure is not null) throw Failure;
            return Task.FromResult(new StripeSubscriptionTierChange("si_1", "price_old", monthlyPriceId));
        }

        public Task RestoreAsync(StripeSubscriptionTierChange change, CancellationToken cancellationToken = default)
        {
            Restored = true;
            return Task.CompletedTask;
        }
    }

    private sealed class EventRepositoryFake : IOperationalEventRepository
    {
        public List<OperationalEvent> Items { get; } = [];
        public Exception? Failure { get; init; }
        public Task AddAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default)
        {
            if (Failure is not null) throw Failure;
            Items.Add(operationalEvent);
            return Task.CompletedTask;
        }
        public Task<IReadOnlyCollection<OperationalEvent>> GetRecentAsync(int limit, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<OperationalEvent>>(Items.Take(limit).ToArray());
    }

    private sealed class FeatureResolutionFake : IFeatureResolutionService
    {
        public List<Guid> InvalidatedVenueIds { get; } = [];
        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult<FeatureEntitlement?>(null);
        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(new Dictionary<string, FeatureEntitlement>());
        public void Invalidate(Guid venueId) => InvalidatedVenueIds.Add(venueId);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}
