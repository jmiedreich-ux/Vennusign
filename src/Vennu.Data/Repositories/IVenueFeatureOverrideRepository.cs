using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IVenueFeatureOverrideRepository
{
    Task<IReadOnlyCollection<VenueFeatureOverride>> GetActiveByVenueAsync(Guid venueId, DateTime utcNow, CancellationToken cancellationToken = default);
    Task UpsertAsync(VenueFeatureOverride featureOverride, CancellationToken cancellationToken = default);
}
