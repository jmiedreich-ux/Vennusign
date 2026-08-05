using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class VideoWallServiceTests
{
    [Fact]
    public async Task SaveAsync_AssignsDeterministicPositionsAndClearsDisplacedGroups()
    {
        var venueId = Guid.NewGuid();
        var first = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Left", WallGroup = "Old", WallPosition = 1 };
        var second = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Right" };
        var displaced = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Old partner", WallGroup = "Old", WallPosition = 2 };
        var replaced = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Previous main", WallGroup = "Main", WallPosition = 1 };
        var screens = new FakeScreenRepository
        {
            GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>([first, second, displaced, replaced])
        };
        var notifier = new RecordingNotifier();
        var service = CreateService(venueId, screens, notifier);

        var group = await service.SaveAsync(venueId, " Main ", "2x1", [first.Id, second.Id]);

        Assert.Equal([1, 2], group.Screens.Select(screen => screen.Position));
        Assert.Equal("Main", first.WallGroup);
        Assert.Equal(1, first.WallPosition);
        Assert.Equal("Main", second.WallGroup);
        Assert.Equal(2, second.WallPosition);
        Assert.Null(displaced.WallGroup);
        Assert.Null(replaced.WallGroup);
        Assert.Equal(1, notifier.VenueContentCount);
    }

    [Fact]
    public async Task SaveAsync_RequiresSupportedLayoutAndMatchingUniqueOwnedScreens()
    {
        var venueId = Guid.NewGuid();
        var owned = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "One" };
        var screens = new FakeScreenRepository
        {
            GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>([owned])
        };
        var service = CreateService(venueId, screens, new RecordingNotifier());

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(venueId, "Wall", "4x4", [owned.Id]));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(venueId, "Wall", "2x1", [owned.Id, owned.Id]));
        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(venueId, "Wall", "2x1", [owned.Id, Guid.NewGuid()]));
    }

    [Fact]
    [Fact]
    public async Task SaveAsync_RejectsArchivedScreen()
    {
        var venueId = Guid.NewGuid();
        var active = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Active" };
        var archived = new Screen { Id = Guid.NewGuid(), VenueId = venueId, Name = "Old", Status = "Archived" };
        var screens = new FakeScreenRepository
        {
            GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>([active, archived])
        };
        var service = CreateService(venueId, screens, new RecordingNotifier());

        await Assert.ThrowsAsync<ArgumentException>(() => service.SaveAsync(venueId, "Wall", "2x1", [active.Id, archived.Id]));
    }

    private static VideoWallService CreateService(
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
            notifier);

    private sealed class RecordingNotifier : IScreenUpdateNotifier
    {
        public int VenueContentCount { get; private set; }
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) { VenueContentCount++; return Task.CompletedTask; }
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
