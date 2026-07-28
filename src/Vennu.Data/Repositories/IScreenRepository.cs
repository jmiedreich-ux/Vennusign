using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IScreenRepository
{
    Task<Guid> CreateAsync(Screen screen, CancellationToken cancellationToken = default);
    Task<bool> AssignVenueAsync(Guid screenId, Guid venueId, CancellationToken cancellationToken = default);
    Task<Screen?> GetByIdAsync(Guid screenId, CancellationToken cancellationToken = default);
    Task<Screen?> GetByScreenKeyAsync(string screenKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Screen>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Screen>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<bool> UpdateHeartbeatAsync(Guid screenId, DateTime lastSeenUtc, string status, CancellationToken cancellationToken = default);
    Task<int> MarkStaleOnlineScreensOfflineAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}
