using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IPlaylistSlideRepository
{
    Task<IReadOnlyCollection<PlaylistSlide>> GetByScreenAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(PlaylistSlide slide, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(PlaylistSlide slide, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid venueId, Guid screenId, Guid slideId, CancellationToken cancellationToken = default);
}
