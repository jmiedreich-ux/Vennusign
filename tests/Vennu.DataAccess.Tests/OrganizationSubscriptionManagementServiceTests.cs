using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class OrganizationSubscriptionManagementServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 2, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task StartTrial_UsesTierPolicyBeforeAnyVenueExists()
    {
        var organizationId = Guid.NewGuid();
        var tier = new SubscriptionTier { Id = Guid.NewGuid(), IsActive = true, IsPublic = true, TrialDays = 21, MaxVenues = 2 };
        var subscriptions = new OrganizationSubscriptionRepositoryFake();
        var service = new OrganizationSubscriptionManagementService(
            subscriptions, new TierRepositoryFake(tier), new VenueRepositoryFake([]), new ProjectionFake(), new FixedTimeProvider());

        var result = await service.StartTrialAsync(organizationId, tier.Id);

        Assert.Equal(organizationId, result.OrganizationId);
        Assert.Equal(tier.Id, result.TierId);
        Assert.Equal("trialing", result.Status);
        Assert.Equal(UtcNow.UtcDateTime.AddDays(21), result.TrialEndsAt);
        Assert.Same(result, Assert.Single(subscriptions.Items));
    }

    [Fact]
    public async Task EnsureCanAddVenue_EnforcesOrganizationVenueLimit()
    {
        var organizationId = Guid.NewGuid();
        var tier = new SubscriptionTier { Id = Guid.NewGuid(), MaxVenues = 1 };
        var subscription = new OrganizationSubscription
        {
            OrganizationId = organizationId, TierId = tier.Id, Status = "active"
        };
        var service = new OrganizationSubscriptionManagementService(
            new OrganizationSubscriptionRepositoryFake(subscription),
            new TierRepositoryFake(tier),
            new VenueRepositoryFake([new Venue { Id = Guid.NewGuid(), OrganizationId = organizationId }]),
            new ProjectionFake(),
            new FixedTimeProvider());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EnsureCanAddVenueAsync(organizationId));
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => UtcNow;
    }

    private sealed class OrganizationSubscriptionRepositoryFake(params OrganizationSubscription[] values)
        : IOrganizationSubscriptionRepository
    {
        public List<OrganizationSubscription> Items { get; } = values.ToList();
        public Task<IReadOnlyCollection<OrganizationSubscription>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<OrganizationSubscription>>(Items);
        public Task<OrganizationSubscription?> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(item => item.OrganizationId == organizationId));
        public Task<OrganizationSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(item => item.StripeSubscriptionId == stripeSubscriptionId));
        public Task<bool> SaveAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default)
        {
            if (!Items.Contains(subscription)) Items.Add(subscription);
            return Task.FromResult(true);
        }
    }

    private sealed class TierRepositoryFake(SubscriptionTier tier) : ISubscriptionTierRepository
    {
        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<SubscriptionTier>>([tier]);
        public Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default) => Task.FromResult<SubscriptionTier?>(tier.Id == tierId ? tier : null);
        public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) => Task.FromResult<SubscriptionTier?>(null);
        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<TierFeature>>([]);
        public Task<bool> CreateAsync(SubscriptionTier value, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> UpdateAsync(SubscriptionTier value, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class VenueRepositoryFake(IReadOnlyCollection<Venue> values) : IVenueRepository
    {
        public Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(values);
        public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult(values.SingleOrDefault(item => item.Id == venueId));
    }

    private sealed class ProjectionFake : IOrganizationSubscriptionProjectionService
    {
        public Task SyncAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<VenueSubscription> SyncVenueAsync(Guid venueId, OrganizationSubscription subscription, CancellationToken cancellationToken = default) =>
            Task.FromResult(new VenueSubscription { VenueId = venueId, TierId = subscription.TierId, Status = subscription.Status });
    }
}
