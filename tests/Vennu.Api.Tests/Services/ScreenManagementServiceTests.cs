using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class ScreenManagementServiceTests
{
    [Fact]
    public async Task CreateAsync_AssignsVenueAndReturnsDisplayRegistrationUrl()
    {
        var venueId = Guid.NewGuid();
        var screens = new FakeScreenRepository();
        var service = CreateService(venueId, screens, new RecordingNotifier());

        var created = await service.CreateAsync(venueId, "  Patio board  ", "  East wall  ");

        Assert.Equal("Patio board", created.Name);
        Assert.Equal("East wall", created.Location);
        Assert.Equal($"/display/{created.Id}", created.RegistrationUrl);
        Assert.Equal(venueId, screens.LastCreatedScreen?.VenueId);
        Assert.Equal("3x2", screens.LastCreatedScreen?.PhotoGridDensity);
        Assert.Equal("photo_grid", screens.LastCreatedScreen?.DisplayLayout);
        Assert.Equal("40_60", screens.LastCreatedScreen?.SplitRatio);
        Assert.Matches("^sc-[a-z0-9]{6}$", screens.LastCreatedScreen?.ScreenKey);
    }

    [Fact]
    public async Task GetAsync_ReturnsDeterministicVenueScopedList()
    {
        var venueId = Guid.NewGuid();
        var screens = new FakeScreenRepository
        {
            GetByVenueIdAsyncHandler = (id, _) => Task.FromResult<IReadOnlyCollection<Screen>>(
            [
                new Screen { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), VenueId = id, Name = "Lobby", Status = "Online" },
                new Screen { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), VenueId = id, Name = "bar", Status = "Offline" }
            ])
        };
        var service = CreateService(venueId, screens, new RecordingNotifier());

        var result = await service.GetAsync(venueId);

        Assert.Equal(["bar", "Lobby"], result.Select(screen => screen.Name));
    }

    [Fact]
    public async Task UpdateAndPush_RejectCrossVenueScreen()
    {
        var venueId = Guid.NewGuid();
        var screens = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(
                new Screen { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Name = "Other venue" })
        };
        var notifier = new RecordingNotifier();
        var service = CreateService(venueId, screens, notifier);

        var updated = await service.UpdateAsync(venueId, Guid.NewGuid(), "Renamed", null, "3x2", "photo_grid");
        var pushed = await service.PushAsync(venueId, Guid.NewGuid());

        Assert.Null(updated);
        Assert.False(pushed);
        Assert.Null(screens.LastUpdatedScreen);
        Assert.Equal(0, notifier.ScreenContentCount);
    }

    [Fact]
    public async Task UpdateAsync_PersistsSupportedPhotoGridDensity()
    {
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var screens = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(
                new Screen { Id = screenId, VenueId = venueId, Name = "Bar" })
        };
        var service = CreateService(venueId, screens, new RecordingNotifier());

        var updated = await service.UpdateAsync(venueId, screenId, "Bar", null, " 4X2 ", null);

        Assert.Equal("4x2", updated?.PhotoGridDensity);
        Assert.Equal("4x2", screens.LastUpdatedScreen?.PhotoGridDensity);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(venueId, screenId, "Bar", null, "5x2", null));
    }

    [Fact]
    public async Task UpdateAsync_PersistsSupportedDisplayLayout()
    {
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var screens = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(
                new Screen { Id = screenId, VenueId = venueId, Name = "Bar" })
        };
        var service = CreateService(venueId, screens, new RecordingNotifier());

        var updated = await service.UpdateAsync(
            venueId, screenId, "Bar", null, null, " Classic-Diner ");

        Assert.Equal("classic_diner", updated?.DisplayLayout);
        Assert.Equal("classic_diner", screens.LastUpdatedScreen?.DisplayLayout);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(venueId, screenId, "Bar", null, null, "neon"));
    }

    [Fact]
    public async Task UpdateAsync_PersistsSupportedSplitLayoutRatio()
    {
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var screens = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(
                new Screen { Id = screenId, VenueId = venueId, Name = "Bar" })
        };
        var service = CreateService(venueId, screens, new RecordingNotifier());

        var updated = await service.UpdateAsync(
            venueId, screenId, "Bar", null, null, " Split Layout ", "50/50");

        Assert.Equal("split_layout", updated?.DisplayLayout);
        Assert.Equal("50_50", updated?.SplitRatio);
        Assert.Equal("50_50", screens.LastUpdatedScreen?.SplitRatio);
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(venueId, screenId, "Bar", null, null, "split_layout", "70_30"));
    }

    [Fact]
    public async Task PushAsync_NotifiesOnlyTheRequestedOwnedScreen()
    {
        var venueId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var screens = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(
                new Screen { Id = screenId, VenueId = venueId, Name = "Bar" })
        };
        var notifier = new RecordingNotifier();
        var service = CreateService(venueId, screens, notifier);

        var pushed = await service.PushAsync(venueId, screenId);

        Assert.True(pushed);
        Assert.Equal(screenId, notifier.ScreenId);
        Assert.Equal(1, notifier.ScreenContentCount);
    }

    private static ScreenManagementService CreateService(
        Guid venueId,
        FakeScreenRepository screens,
        RecordingNotifier notifier) =>
        new(
            screens,
            new FakeVenueRepository
            {
                GetByIdAsyncHandler = (id, _) => Task.FromResult<Venue?>(
                    id == venueId ? new Venue { Id = id, Name = "Cafe" } : null)
            },
            notifier,
            new FixedTimeProvider());

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 29, 23, 30, 0, TimeSpan.Zero);
    }

    private sealed class RecordingNotifier : IScreenUpdateNotifier
    {
        public Guid? ScreenId { get; private set; }
        public int ScreenContentCount { get; private set; }
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default)
        {
            ScreenId = screenId;
            ScreenContentCount++;
            return Task.CompletedTask;
        }
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
