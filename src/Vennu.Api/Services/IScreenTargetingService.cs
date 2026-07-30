using Vennu.Api.Contracts.Admin;

namespace Vennu.Api.Services;

public interface IScreenTargetingService
{
    Task<int> PushAllAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<ScreenOverflowPreview> GetOverflowAsync(Guid venueId, int capacity, CancellationToken cancellationToken = default);
}
