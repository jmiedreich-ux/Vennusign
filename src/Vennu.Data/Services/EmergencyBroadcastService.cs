using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class EmergencyBroadcastService(
    IEmergencyBroadcastRepository repository,
    IScreenRepository screens) : IEmergencyBroadcastService
{
    public Task<IReadOnlyCollection<EmergencyBroadcast>> GetAsync(
        Guid venueId, CancellationToken cancellationToken = default) =>
        repository.GetByVenueAsync(Require(venueId), cancellationToken);

    public async Task<EmergencyBroadcast?> GetActiveAsync(
        Guid venueId, Guid screenId, DateTimeOffset utcNow, CancellationToken cancellationToken = default)
    {
        Require(screenId);
        var rows = await repository.GetByVenueAsync(Require(venueId), cancellationToken).ConfigureAwait(false);
        return EmergencyBroadcastSelection.Select(rows, screenId, utcNow);
    }

    public async Task<EmergencyBroadcast> CreateAsync(
        Guid venueId, Guid? screenId, string title, string message, string? mediaUrl,
        int durationMinutes, DateTimeOffset utcNow, CancellationToken cancellationToken = default)
    {
        Require(venueId);
        if (screenId.HasValue)
        {
            var screen = await screens.GetByIdAsync(Require(screenId.Value), cancellationToken).ConfigureAwait(false);
            if (screen?.VenueId != venueId) throw new KeyNotFoundException("Screen does not exist for this venue.");
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (title.Trim().Length > 200) throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));
        if (message.Trim().Length > 2000) throw new ArgumentException("Message cannot exceed 2000 characters.", nameof(message));
        if (durationMinutes is < 1 or > 1440) throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        var now = utcNow.UtcDateTime;
        var broadcast = new EmergencyBroadcast
        {
            Id = Guid.NewGuid(), VenueId = venueId, ScreenId = screenId,
            Title = title.Trim(), Message = message.Trim(), MediaUrl = Normalize(mediaUrl, 1000),
            StartsUtc = now, ExpiresUtc = utcNow.AddMinutes(durationMinutes).UtcDateTime,
            IsActive = true, CreatedUtc = now, UpdatedUtc = now
        };
        await repository.CreateAsync(broadcast, cancellationToken).ConfigureAwait(false);
        return broadcast;
    }

    public async Task<EmergencyBroadcast?> CancelAsync(
        Guid venueId, Guid broadcastId, DateTimeOffset utcNow, CancellationToken cancellationToken = default)
    {
        var broadcast = (await repository.GetByVenueAsync(Require(venueId), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(item => item.Id == Require(broadcastId));
        if (broadcast is null) return null;
        broadcast.IsActive = false;
        broadcast.UpdatedUtc = utcNow.UtcDateTime;
        await repository.UpdateAsync(broadcast, cancellationToken).ConfigureAwait(false);
        return broadcast;
    }

    private static string? Normalize(string? value, int max) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= max ? value.Trim() : throw new ArgumentException($"Value cannot exceed {max} characters.");
    private static Guid Require(Guid id) => id == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.") : id;
}

public static class EmergencyBroadcastSelection
{
    public static EmergencyBroadcast? Select(
        IEnumerable<EmergencyBroadcast> broadcasts, Guid screenId, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(broadcasts);
        var now = utcNow.UtcDateTime;
        return broadcasts
            .Where(item => item.IsActive && item.StartsUtc <= now && now < item.ExpiresUtc
                && (!item.ScreenId.HasValue || item.ScreenId == screenId))
            .OrderByDescending(item => item.ScreenId.HasValue)
            .ThenByDescending(item => item.StartsUtc)
            .ThenByDescending(item => item.CreatedUtc)
            .ThenBy(item => item.Id)
            .FirstOrDefault();
    }
}
