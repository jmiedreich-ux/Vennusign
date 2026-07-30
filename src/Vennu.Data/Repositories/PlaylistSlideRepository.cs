using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class PlaylistSlideRepository(ISqlDataAccess dataAccess) : IPlaylistSlideRepository
{
    private const string ByScreenSql = """
        SELECT Id, VenueId, ScreenId, SlideType, Title, Body, MediaUrl, DwellSeconds,
               StartLocalTime, EndLocalTime, ActiveDaysMask, IsEnabled, SortOrder,
               CreatedUtc, UpdatedUtc
        FROM dbo.PlaylistSlides
        WHERE VenueId = @VenueId AND ScreenId = @ScreenId
        ORDER BY SortOrder, Id;
        """;

    private const string DeleteSql = """
        DELETE FROM dbo.PlaylistSlides
        WHERE VenueId = @VenueId AND ScreenId = @ScreenId AND Id = @SlideId;
        SELECT CONVERT(BIT, CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END) AS Removed;
        """;

    public async Task<IReadOnlyCollection<PlaylistSlide>> GetByScreenAsync(
        Guid venueId, Guid screenId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PlaylistSlide, object>(
            ByScreenSql, new { VenueId = Require(venueId), ScreenId = Require(screenId) }, cancellationToken)
            .ConfigureAwait(false)).ToArray();

    public async Task<Guid> CreateAsync(PlaylistSlide slide, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(slide);
        if (slide.Id == Guid.Empty) slide.Id = Guid.NewGuid();
        await dataAccess.InsertAsync(slide, cancellationToken).ConfigureAwait(false);
        return slide.Id;
    }

    public async Task<bool> UpdateAsync(PlaylistSlide slide, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(slide);
        return await dataAccess.UpdateAsync(slide, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<bool> DeleteAsync(
        Guid venueId, Guid screenId, Guid slideId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<RemovalResult, object>(
            DeleteSql,
            new { VenueId = Require(venueId), ScreenId = Require(screenId), SlideId = Require(slideId) },
            cancellationToken).ConfigureAwait(false)).Single().Removed;

    private static Guid Require(Guid id) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.") : id;

    private sealed class RemovalResult { public bool Removed { get; set; } }
}
