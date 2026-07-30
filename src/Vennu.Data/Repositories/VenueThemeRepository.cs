using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class VenueThemeRepository(ISqlDataAccess dataAccess) : IVenueThemeRepository
{
    public Task<VenueTheme?> GetByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        GetAsync(venueId, cancellationToken);

    public async Task UpsertAsync(VenueTheme theme, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(theme);
        await dataAccess.MergeAllAsync(
            new[] { theme },
            "dbo.VenueThemes",
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<VenueTheme?> GetAsync(Guid venueId, CancellationToken cancellationToken) =>
        (await dataAccess.QueryAsync<VenueTheme, object>(
            "dbo.VenueThemes",
            new { VenueId = venueId },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
}
