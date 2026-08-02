using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class OrganizationSubscriptionProjectionServiceTests
{
    [Fact]
    public async Task Sync_KeepsLegacyStripeIdOnOnlyOneVenueProjection()
    {
        var organizationId = Guid.NewGuid();
        var first = new Venue { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), OrganizationId = organizationId };
        var second = new Venue { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), OrganizationId = organizationId };
        var venueSubscriptions = new VenueSubscriptionRepositoryFake();
        var service = new OrganizationSubscriptionProjectionService(
            new VenueRepositoryFake([second, first]), venueSubscriptions, new FeatureResolutionFake());

        await service.SyncAsync(new OrganizationSubscription
        {
            OrganizationId = organizationId,
            TierId = Guid.NewGuid(),
            StripeSubscriptionId = "sub_org",
            Status = "active"
        });

        Assert.Equal("sub_org", venueSubscriptions.Items.Single(item => item.VenueId == first.Id).StripeSubscriptionId);
        Assert.Null(venueSubscriptions.Items.Single(item => item.VenueId == second.Id).StripeSubscriptionId);
    }

    private sealed class VenueRepositoryFake(IReadOnlyCollection<Venue> values) : IVenueRepository
    {
        public Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(values);
        public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult(values.SingleOrDefault(item => item.Id == venueId));
    }

    private sealed class VenueSubscriptionRepositoryFake : IVenueSubscriptionRepository
    {
        public List<VenueSubscription> Items { get; } = [];
        public Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<VenueSubscription>>(Items);
        public Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult(Items.SingleOrDefault(item => item.VenueId == venueId));
        public Task<VenueSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default) => Task.FromResult(Items.SingleOrDefault(item => item.StripeSubscriptionId == stripeSubscriptionId));
        public Task<bool> SaveAsync(VenueSubscription value, CancellationToken cancellationToken = default)
        {
            if (!Items.Contains(value)) Items.Add(value);
            return Task.FromResult(true);
        }
    }

    private sealed class FeatureResolutionFake : IFeatureResolutionService
    {
        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) => Task.FromResult<FeatureEntitlement?>(null);
        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyDictionary<string, FeatureEntitlement>>(new Dictionary<string, FeatureEntitlement>());
        public void Invalidate(Guid venueId) { }
    }
}
