using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class VenueSupportDetailServiceTests
{
    [Fact]
    public async Task GetAsync_ComposesExistingSupportContext()
    {
        var venueId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var venue = new Venue { Id = venueId, Name = "North Star", Type = "Restaurant" };
        var subscription = new VenueSubscription { VenueId = venueId, TierId = tierId, Status = "active" };
        var tier = new SubscriptionTier { Id = tierId, Name = "Pro", Slug = "pro", MaxScreens = 8 };
        var screen = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Lobby", Status = "Online" };
        var feature = new FeatureEntitlement("scheduling", true, null, "tier");
        var featureOverride = new VenueFeatureOverride { VenueId = venueId, FeatureId = Guid.NewGuid(), Enabled = true, Reason = "Support" };
        var service = new VenueSupportDetailService(
            new FakeVenueRepository { GetByIdAsyncHandler = (id, _) => Task.FromResult(id == venueId ? venue : null) },
            new FakeVenueSubscriptionRepository { Items = [subscription] },
            new FakeSubscriptionTierRepository { Items = [tier] },
            new FakeScreenRepository { GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>([screen]) },
            new FeatureResolutionFake(new Dictionary<string, FeatureEntitlement> { [feature.Key] = feature }),
            new FakeVenueFeatureOverrideRepository { Items = [featureOverride] },
            TimeProvider.System);

        var detail = await service.GetAsync(venueId);

        Assert.NotNull(detail);
        Assert.Same(venue, detail.Venue);
        Assert.Same(subscription, detail.Subscription);
        Assert.Same(tier, detail.Tier);
        Assert.Same(screen, Assert.Single(detail.Screens));
        Assert.Equal(feature, detail.Features["scheduling"]);
        Assert.Same(featureOverride, Assert.Single(detail.ActiveOverrides));
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWithoutLoadingRelatedContext_WhenVenueDoesNotExist()
    {
        var service = new VenueSupportDetailService(
            new FakeVenueRepository { GetByIdAsyncHandler = (_, _) => Task.FromResult<Venue?>(null) },
            new FakeVenueSubscriptionRepository(),
            new FakeSubscriptionTierRepository(),
            new FakeScreenRepository(),
            new FeatureResolutionFake(new Dictionary<string, FeatureEntitlement>()),
            new FakeVenueFeatureOverrideRepository(),
            TimeProvider.System);

        Assert.Null(await service.GetAsync(Guid.NewGuid()));
    }

    private sealed class FeatureResolutionFake(IReadOnlyDictionary<string, FeatureEntitlement> features)
        : IFeatureResolutionService
    {
        public Task<bool> HasFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(features.GetValueOrDefault(featureKey)?.Enabled == true);

        public Task<FeatureEntitlement?> GetFeatureAsync(Guid venueId, string featureKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(features.GetValueOrDefault(featureKey));

        public Task<IReadOnlyDictionary<string, FeatureEntitlement>> GetFeatureSetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(features);

        public void Invalidate(Guid venueId)
        {
        }
    }
}
