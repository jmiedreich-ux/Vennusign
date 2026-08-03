using Vennu.Api.Contracts.PlatformOperations;

namespace Vennu.Api.Services;

public interface IScreenManagementService
{
    Task<IReadOnlyCollection<ScreenManagementItem>> GetAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<ScreenManagementItem> CreateAsync(Guid venueId, string name, string? location, CancellationToken cancellationToken = default);
    Task<ScreenManagementItem?> UpdateAsync(
        Guid venueId,
        Guid screenId,
        string name,
        string? location,
        string? photoGridDensity,
        string? displayLayout,
        string? splitRatio = null,
        int? heroDwellSeconds = null,
        CancellationToken cancellationToken = default);
    Task<bool> PushAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);
    Task<ScreenManagementItem?> SetArchivedAsync(Guid venueId, Guid screenId, bool archived, CancellationToken cancellationToken = default);
    Task<ScreenManagementItem?> ResetAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);
    Task<bool> UnpairAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);
}
