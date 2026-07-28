using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IVenueTierSwitchService
{
    Task<VenueSubscription> SwitchAsync(
        Guid venueId,
        Guid targetTierId,
        CancellationToken cancellationToken = default);
}
