using Vennu.Api.Contracts.Admin;

namespace Vennu.Api.Services;

public interface IScreenManagementService
{
    Task<IReadOnlyCollection<ScreenManagementItem>> GetAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<ScreenManagementItem> CreateAsync(Guid venueId, string name, string? location, CancellationToken cancellationToken = default);
    Task<ScreenManagementItem?> UpdateAsync(Guid venueId, Guid screenId, string name, string? location, string? photoGridDensity, CancellationToken cancellationToken = default);
    Task<bool> PushAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);
}
