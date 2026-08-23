using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Controllers.BackOffice;
using Vennu.Api.Notifications;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

// #811: a venue-wide broadcast (ScreenId null) used to notify only `venue:{id}`,
// a SignalR group nothing joins - so it never reached a live display and relied
// entirely on the 60s content-poll recovery, the same masking mechanism that hid
// #769 until #763 measured it. These assert the per-screen fan-out fix directly,
// the way it's actually delivered.
[Trait("Category", "Unit")]
public sealed class BackOfficeEmergencyBroadcastsControllerTests
{
    [Fact]
    public async Task Create_VenueWideBroadcast_NotifiesEveryScreenInTheVenueDirectly()
    {
        var venueId = Guid.NewGuid();
        var screenIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var screenRepository = new FakeScreenRepository
        {
            GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>(
                [.. screenIds.Select(id => new Screen { Id = id, VenueId = venueId })])
        };
        var notifier = new RecordingNotifier();
        var controller = new BackOfficeEmergencyBroadcastsController(
            new FakeEmergencyBroadcastService(), notifier, screenRepository, TimeProvider.System);

        await controller.Create(
            venueId, new EmergencyBroadcastWriteRequest(null, "Fire alarm", "Evacuate now", null, 30),
            CancellationToken.None);

        Assert.Equal(screenIds.OrderBy(id => id), notifier.ScreenNotifiedIds.OrderBy(id => id));
        Assert.Equal(1, notifier.VenueNotifiedCount);
    }

    [Fact]
    public async Task Create_SingleScreenBroadcast_NotifiesOnlyThatScreen_NoVenueFanOut()
    {
        var venueId = Guid.NewGuid();
        var targetScreenId = Guid.NewGuid();
        var screenRepository = new FakeScreenRepository
        {
            // Should never be consulted for a single-screen broadcast.
            GetByVenueIdAsyncHandler = (_, _) => throw new InvalidOperationException(
                "GetByVenueIdAsync should not be called for a screen-targeted broadcast.")
        };
        var notifier = new RecordingNotifier();
        var controller = new BackOfficeEmergencyBroadcastsController(
            new FakeEmergencyBroadcastService(), notifier, screenRepository, TimeProvider.System);

        await controller.Create(
            venueId, new EmergencyBroadcastWriteRequest(targetScreenId, "Fire alarm", "Evacuate now", null, 30),
            CancellationToken.None);

        Assert.Equal([targetScreenId], notifier.ScreenNotifiedIds);
        Assert.Equal(0, notifier.VenueNotifiedCount);
    }

    [Fact]
    public async Task Cancel_VenueWideBroadcast_AlsoFansOutToEveryScreen()
    {
        var venueId = Guid.NewGuid();
        var screenIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var screenRepository = new FakeScreenRepository
        {
            GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>(
                [.. screenIds.Select(id => new Screen { Id = id, VenueId = venueId })])
        };
        var notifier = new RecordingNotifier();
        var broadcast = new EmergencyBroadcast { Id = Guid.NewGuid(), VenueId = venueId, ScreenId = null };
        var service = new FakeEmergencyBroadcastService { CancelResult = broadcast };
        var controller = new BackOfficeEmergencyBroadcastsController(service, notifier, screenRepository, TimeProvider.System);

        await controller.Cancel(venueId, broadcast.Id, CancellationToken.None);

        Assert.Equal(screenIds.OrderBy(id => id), notifier.ScreenNotifiedIds.OrderBy(id => id));
        Assert.Equal(1, notifier.VenueNotifiedCount);
    }

    private sealed class FakeEmergencyBroadcastService : IEmergencyBroadcastService
    {
        public EmergencyBroadcast? CancelResult { get; set; }

        public Task<IReadOnlyCollection<EmergencyBroadcast>> GetAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<EmergencyBroadcast>>([]);

        public Task<EmergencyBroadcast?> GetActiveAsync(Guid venueId, Guid screenId, DateTimeOffset utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult<EmergencyBroadcast?>(null);

        public Task<EmergencyBroadcast> CreateAsync(
            Guid venueId, Guid? screenId, string title, string message, string? mediaUrl,
            int durationMinutes, DateTimeOffset utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult(new EmergencyBroadcast
            {
                Id = Guid.NewGuid(), VenueId = venueId, ScreenId = screenId, Title = title, Message = message,
                MediaUrl = mediaUrl, StartsUtc = utcNow.UtcDateTime, ExpiresUtc = utcNow.AddMinutes(durationMinutes).UtcDateTime,
                IsActive = true
            });

        public Task<EmergencyBroadcast?> CancelAsync(Guid venueId, Guid broadcastId, DateTimeOffset utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult(CancelResult);
    }

    private sealed class RecordingNotifier : IScreenUpdateNotifier
    {
        public List<Guid> ScreenNotifiedIds { get; } = [];
        public int VenueNotifiedCount { get; private set; }

        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default)
        {
            ScreenNotifiedIds.Add(screenId);
            return Task.CompletedTask;
        }

        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default)
        {
            VenueNotifiedCount++;
            return Task.CompletedTask;
        }

        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
