using Microsoft.AspNetCore.SignalR;
using Vennu.Api.Hubs;

namespace Vennu.Api.Notifications;

public sealed class SignalRScreenUpdateNotifier(IHubContext<VennuHub> hubContext) : IScreenUpdateNotifier
{
    private const string ContentUpdated = "ContentUpdated";
    private const string ThemeUpdated = "ThemeUpdated";
    private const string ItemAvailabilityChanged = "ItemAvailabilityChanged";
    private const string SyncTick = "SyncTick";

    public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) =>
        SendAsync(ScreenGroup(screenId), ContentUpdated, cancellationToken, payload);

    public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) =>
        SendAsync(VenueGroup(venueId), ContentUpdated, cancellationToken, payload);

    public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) =>
        SendAsync(ScreenGroup(screenId), ThemeUpdated, cancellationToken, theme);

    public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) =>
        SendAsync(VenueGroup(venueId), ThemeUpdated, cancellationToken, theme);

    public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) =>
        SendAsync(ScreenGroup(screenId), ItemAvailabilityChanged, cancellationToken, itemId, available);

    public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) =>
        SendAsync(VenueGroup(venueId), ItemAvailabilityChanged, cancellationToken, itemId, available);

    public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) =>
        SendAsync(ScreenGroup(screenId), SyncTick, cancellationToken, serverTimeMs);

    public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) =>
        SendAsync(VenueGroup(venueId), SyncTick, cancellationToken, serverTimeMs);

    private Task SendAsync(string groupName, string eventName, CancellationToken cancellationToken, params object?[] args) =>
        hubContext.Clients.Group(groupName).SendCoreAsync(eventName, args, cancellationToken);

    private static string ScreenGroup(Guid screenId) => $"screen:{screenId}";

    private static string VenueGroup(Guid venueId) => $"venue:{venueId}";
}
