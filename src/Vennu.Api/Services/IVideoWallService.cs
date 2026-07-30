using Vennu.Api.Contracts.Admin;

namespace Vennu.Api.Services;

public interface IVideoWallService
{
    Task<VideoWallSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<VideoWallGroup> SaveAsync(Guid venueId, string name, string layout, IReadOnlyCollection<Guid> screenIds, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(Guid venueId, string name, CancellationToken cancellationToken = default);
}
