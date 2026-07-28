using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class VenueDirectoryServiceTests
{
    [Fact]
    public async Task SearchAsync_AggregatesVenueSupportContext()
    {
        var venueId = Guid.NewGuid();
        var tierId = Guid.NewGuid();
        var lastSeen = new DateTime(2026, 7, 28, 18, 0, 0, DateTimeKind.Utc);
        var service = CreateService(
            [new Venue { Id = venueId, Name = "North Star", Type = "Restaurant" }],
            [new VenueSubscription { VenueId = venueId, TierId = tierId, Status = "active" }],
            [new SubscriptionTier { Id = tierId, Name = "Pro", Slug = "pro" }],
            new Dictionary<Guid, IReadOnlyCollection<Screen>>
            {
                [venueId] =
                [
                    new Screen { VenueId = venueId, Status = "Online", LastSeen = lastSeen },
                    new Screen { VenueId = venueId, Status = "Offline", LastSeen = lastSeen.AddMinutes(-2) }
                ]
            },
            [new VenueFeatureOverride { VenueId = venueId, FeatureId = Guid.NewGuid(), Enabled = true }]);

        var item = Assert.Single(await service.SearchAsync(new VenueDirectoryQuery()));

        Assert.Equal("North Star", item.Name);
        Assert.Equal("Pro", item.TierName);
        Assert.Equal("active", item.SubscriptionStatus);
        Assert.Equal(2, item.ScreenCount);
        Assert.Equal(lastSeen, item.LastActiveUtc);
        Assert.Equal(1, item.OverrideCount);
        Assert.Equal("degraded", item.Health);
    }

    [Fact]
    public async Task SearchAsync_ComposesFiltersAndOrdersByName()
    {
        var proId = Guid.NewGuid();
        var alphaId = Guid.NewGuid();
        var betaId = Guid.NewGuid();
        var service = CreateService(
            [
                new Venue { Id = betaId, Name = "Beta Bar", Type = "Bar" },
                new Venue { Id = alphaId, Name = "Alpha Bar", Type = "Bar" }
            ],
            [
                new VenueSubscription { VenueId = alphaId, TierId = proId, Status = "active" },
                new VenueSubscription { VenueId = betaId, TierId = proId, Status = "canceled" }
            ],
            [new SubscriptionTier { Id = proId, Name = "Pro", Slug = "pro" }],
            new Dictionary<Guid, IReadOnlyCollection<Screen>>
            {
                [alphaId] = [new Screen { Status = "Online" }],
                [betaId] = [new Screen { Status = "Offline" }]
            },
            []);

        var results = await service.SearchAsync(new VenueDirectoryQuery("bar", "pro", "active", "online"));

        var item = Assert.Single(results);
        Assert.Equal("Alpha Bar", item.Name);
    }

    private static VenueDirectoryService CreateService(
        IReadOnlyCollection<Venue> venues,
        IReadOnlyCollection<VenueSubscription> subscriptions,
        IReadOnlyCollection<SubscriptionTier> tiers,
        IReadOnlyDictionary<Guid, IReadOnlyCollection<Screen>> screens,
        IReadOnlyCollection<VenueFeatureOverride> overrides)
    {
        return new VenueDirectoryService(
            new FakeVenueRepository { GetAllAsyncHandler = _ => Task.FromResult(venues) },
            new FakeVenueSubscriptionRepository { Items = subscriptions },
            new FakeSubscriptionTierRepository { Items = tiers },
            new FakeScreenRepository
            {
                GetByVenueIdAsyncHandler = (venueId, _) =>
                    Task.FromResult(screens.GetValueOrDefault(venueId, []))
            },
            new FakeVenueFeatureOverrideRepository { Items = overrides },
            TimeProvider.System);
    }
}
