using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class OperationalDashboardServiceTests
{
    [Fact]
    public async Task GetAsync_AggregatesLifecycleAndScreenHealth()
    {
        var venueId = Guid.NewGuid();
        var venues = new FakeVenueRepository
        {
            GetAllAsyncHandler = _ => Task.FromResult<IReadOnlyCollection<Venue>>(
                [new Venue { Id = venueId, Name = "North Bar" }, new Venue { Id = Guid.NewGuid(), Name = "South Cafe" }])
        };
        var subscriptions = new FakeVenueSubscriptionRepository
        {
            Items =
            [
                new VenueSubscription { VenueId = venueId, Status = "active", UpdatedUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc) },
                new VenueSubscription { VenueId = Guid.NewGuid(), Status = "trialing", UpdatedUtc = new DateTime(2026, 7, 28, 0, 0, 0, DateTimeKind.Utc) },
                new VenueSubscription { VenueId = Guid.NewGuid(), Status = "canceled", UpdatedUtc = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc) },
                new VenueSubscription { VenueId = Guid.NewGuid(), Status = "canceled", UpdatedUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc) }
            ]
        };
        var screens = new FakeScreenRepository
        {
            GetAllAsyncHandler = _ => Task.FromResult<IReadOnlyCollection<Screen>>(
            [
                new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Main", Status = "Online", Platform = "tizen", AppVersion = "1.0", DesiredAppVersion = "2.0" },
                new Screen { Id = Guid.NewGuid(), Name = "Spare", Status = "Unknown" }
            ])
        };
        var service = new OperationalDashboardService(venues, subscriptions, screens, new FixedTimeProvider());

        var dashboard = await service.GetAsync();

        Assert.Equal(2, dashboard.TotalVenues);
        Assert.Equal(1, dashboard.ActiveVenues);
        Assert.Equal(1, dashboard.TrialingVenues);
        Assert.Equal(1, dashboard.CanceledLast30Days);
        Assert.Equal(1, dashboard.OnlineScreens);
        Assert.Equal(1, dashboard.OfflineScreens);
        Assert.Equal(1, dashboard.OutdatedScreens);
        Assert.Contains(dashboard.Screens, screen => screen.VenueName == "North Bar" && screen.Status == "online");
        Assert.Contains(dashboard.Screens, screen => screen.VenueName == "Unassigned" && screen.Status == "offline");
    }

    [Fact]
    public async Task GetAsync_ReturnsEmptyDashboard_WhenNoDataExists()
    {
        var service = new OperationalDashboardService(
            new FakeVenueRepository(),
            new FakeVenueSubscriptionRepository(),
            new FakeScreenRepository(),
            new FixedTimeProvider());

        var dashboard = await service.GetAsync();

        Assert.Equal(0, dashboard.TotalVenues);
        Assert.Empty(dashboard.Screens);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 28, 22, 30, 0, TimeSpan.Zero);
    }
}
