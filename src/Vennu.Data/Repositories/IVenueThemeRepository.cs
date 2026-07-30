using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IVenueThemeRepository
{
    Task<VenueTheme?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task UpsertAsync(VenueTheme theme, CancellationToken cancellationToken = default);
}
