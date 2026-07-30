using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IEmergencyBroadcastService
{
    Task<IReadOnlyCollection<EmergencyBroadcast>> GetAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<EmergencyBroadcast?> GetActiveAsync(Guid venueId, Guid screenId, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
    Task<EmergencyBroadcast> CreateAsync(Guid venueId, Guid? screenId, string title, string message, string? mediaUrl, int durationMinutes, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
    Task<EmergencyBroadcast?> CancelAsync(Guid venueId, Guid broadcastId, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
}
