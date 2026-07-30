using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IPlaylistAdministrationService
{
    Task<IReadOnlyCollection<PlaylistSlide>> GetAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PlaylistSlide>> GetActiveAsync(Guid venueId, Guid screenId, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
    Task<PlaylistSlide> CreateAsync(Guid venueId, Guid screenId, PlaylistSlideWrite write, CancellationToken cancellationToken = default);
    Task<PlaylistSlide?> UpdateAsync(Guid venueId, Guid screenId, Guid slideId, PlaylistSlideWrite write, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid venueId, Guid screenId, Guid slideId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PlaylistSlide>> ReorderAsync(Guid venueId, Guid screenId, IReadOnlyCollection<Guid> orderedIds, CancellationToken cancellationToken = default);
}

public sealed record PlaylistSlideWrite(
    string SlideType, string? Title, string? Body, string? MediaUrl, int DwellSeconds,
    TimeSpan? StartLocalTime, TimeSpan? EndLocalTime, int? ActiveDaysMask, bool IsEnabled);
