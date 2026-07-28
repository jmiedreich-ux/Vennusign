using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public class StripeSubscriptionEventHandlerTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 28, 16, 0, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Subscription_created_maps_price_and_creates_venue_subscription()
    {
        var venueId = Guid.NewGuid();
        var tier = Tier("price_pro");
        var subscriptions = new SubscriptionRepositoryFake();
        var features = new FeatureResolutionFake();
        var handler = CreateHandler(tier, subscriptions, features);

        var applied = await handler.HandleAsync(new StripeSubscriptionEvent(
            "evt_created",
            "subscription.created",
            "sub_1",
            venueId,
            "price_pro",
            "trialing",
            TrialEndsAt: UtcNow.UtcDateTime.AddDays(14)));

        Assert.True(applied);
        var subscription = Assert.Single(subscriptions.Items);
        Assert.Equal(venueId, subscription.VenueId);
        Assert.Equal(tier.Id, subscription.TierId);
        Assert.Equal("sub_1", subscription.StripeSubscriptionId);
        Assert.Equal("trialing", subscription.Status);
        Assert.Equal(venueId, Assert.Single(features.InvalidatedVenueIds));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Subscription_updated_changes_tier_and_status()
    {
        var venueId = Guid.NewGuid();
        var tier = Tier("price_business");
        var existing = new VenueSubscription
        {
            VenueId = venueId,
            TierId = Guid.NewGuid(),
            StripeSubscriptionId = "sub_1",
            Status = "active"
        };
        var subscriptions = new SubscriptionRepositoryFake(existing);
        var handler = CreateHandler(tier, subscriptions, new FeatureResolutionFake());

        await handler.HandleAsync(new StripeSubscriptionEvent(
            "evt_updated",
            "customer.subscription.updated",
            "sub_1",
            venueId,
            "price_business",
            "past_due"));

        Assert.Equal(tier.Id, existing.TierId);
        Assert.Equal("past_due", existing.Status);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Invoice_paid_activates_and_extends_subscription()
    {
        var periodEnd = UtcNow.UtcDateTime.AddMonths(1);
        var existing = Subscription("sub_1", "past_due");
        var subscriptions = new SubscriptionRepositoryFake(existing);
        var handler = CreateHandler(Tier("price_pro"), subscriptions, new FeatureResolutionFake());

        await handler.HandleAsync(new StripeSubscriptionEvent(
            "evt_paid",
            "invoice.paid",
            "sub_1",
            CurrentPeriodEnd: periodEnd));

        Assert.Equal("active", existing.Status);
        Assert.Equal(periodEnd, existing.CurrentPeriodEnd);
        Assert.Null(existing.TrialEndsAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Subscription_deleted_cancels_access()
    {
        var existing = Subscription("sub_1", "active");
        var features = new FeatureResolutionFake();
        var handler = CreateHandler(
            Tier("price_pro"),
            new SubscriptionRepositoryFake(existing),
            features);

        await handler.HandleAsync(new StripeSubscriptionEvent(
            "evt_deleted",
            "customer.subscription.deleted",
            "sub_1"));

        Assert.Equal("canceled", existing.Status);
        Assert.Equal(existing.VenueId, Assert.Single(features.InvalidatedVenueIds));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Duplicate_event_is_not_applied()
    {
        var existing = Subscription("sub_1", "active");
        var idempotency = new IdempotencyFake { Execute = false };
        var handler = CreateHandler(
            Tier("price_pro"),
            new SubscriptionRepositoryFake(existing),
            new FeatureResolutionFake(),
            idempotency);

        var applied = await handler.HandleAsync(new StripeSubscriptionEvent(
            "evt_duplicate",
            "customer.subscription.deleted",
            "sub_1"));

        Assert.False(applied);
        Assert.Equal("active", existing.Status);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Unsupported_event_is_rejected_before_idempotency_claim()
    {
        var idempotency = new IdempotencyFake();
        var handler = CreateHandler(
            Tier("price_pro"),
            new SubscriptionRepositoryFake(),
            new FeatureResolutionFake(),
            idempotency);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            handler.HandleAsync(new StripeSubscriptionEvent("evt_1", "checkout.session.completed", "sub_1")));

        Assert.Empty(idempotency.EventIds);
    }

    private static StripeSubscriptionEventHandler CreateHandler(
        SubscriptionTier tier,
        SubscriptionRepositoryFake subscriptions,
        FeatureResolutionFake features,
        IdempotencyFake? idempotency = null) =>
        new(
            idempotency ?? new IdempotencyFake(),
            new BillingCatalogRepositoryFake(tier),
            subscriptions,
            features,
            new FixedTimeProvider(UtcNow));

    private static SubscriptionTier Tier(string priceId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Pro",
            Slug = "pro",
            Price = 89m,
            MaxScreens = 6,
            StripeMonthlyPriceId = priceId,
            StripeAnnualPriceId = $"{priceId}_annual"
        };

    private static VenueSubscription Subscription(string stripeSubscriptionId, string status) =>
        new()
        {
            VenueId = Guid.NewGuid(),
            TierId = Guid.NewGuid(),
            StripeSubscriptionId = stripeSubscriptionId,
            Status = status,
            TrialEndsAt = UtcNow.UtcDateTime.AddDays(1)
        };

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class IdempotencyFake : IStripeEventIdempotencyService
    {
        public bool Execute { get; set; } = true;
        public List<string> EventIds { get; } = [];

        public async Task<bool> ExecuteOnceAsync(
            string eventId,
            string eventType,
            Func<CancellationToken, Task> handler,
            CancellationToken cancellationToken = default)
        {
            EventIds.Add(eventId);
            if (!Execute)
            {
                return false;
            }

            await handler(cancellationToken);
            return true;
        }
    }

    private sealed class BillingCatalogRepositoryFake(SubscriptionTier tier) : IBillingCatalogRepository
    {
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<SubscriptionTier>>(new[] { tier });

        public Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SubscriptionTier?>(tier.Id == tierId ? tier : null);

        public Task<SubscriptionTier?> GetByStripeProductIdAsync(string productId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SubscriptionTier?>(tier.StripeProductId == productId ? tier : null);

        public Task<SubscriptionTier?> GetByStripePriceIdAsync(string priceId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SubscriptionTier?>(
                tier.StripeMonthlyPriceId == priceId || tier.StripeAnnualPriceId == priceId ? tier : null);

        public Task<bool> SaveAsync(SubscriptionTier value, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class SubscriptionRepositoryFake(params VenueSubscription[] subscriptions) : IVenueSubscriptionRepository
    {
        public List<VenueSubscription> Items { get; } = subscriptions.ToList();

        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<VenueSubscription>>(Items);

        public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(item => item.VenueId == venueId));

        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(
            string stripeSubscriptionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(item => item.StripeSubscriptionId == stripeSubscriptionId));

        public Task<bool> SaveAsync(VenueSubscription subscription, CancellationToken cancellationToken = default)
        {
            if (!Items.Contains(subscription))
            {
                Items.Add(subscription);
            }

            return Task.FromResult(true);
        }
    }

    private sealed class FeatureResolutionFake : IFeatureResolutionService
    {
        public List<Guid> InvalidatedVenueIds { get; } = [];

        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<FeatureEntitlement?>(null);

        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(new Dictionary<string, FeatureEntitlement>());

        public void Invalidate(Guid venueId) => InvalidatedVenueIds.Add(venueId);
    }
}
