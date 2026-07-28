using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IVenueFeatureOverrideManagementService
{
    Task<VenueFeatureOverride?> SetAsync(
        Guid venueId,
        Guid featureId,
        VenueFeatureOverrideRequest request,
        CancellationToken cancellationToken = default);

    Task<bool?> RemoveAsync(Guid venueId, Guid featureId, CancellationToken cancellationToken = default);
}
