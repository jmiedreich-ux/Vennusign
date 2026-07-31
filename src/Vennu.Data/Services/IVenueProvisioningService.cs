using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IVenueProvisioningService
{
    Task<VenueProvisioningResult> ProvisionAsync(
        Venue venue,
        CancellationToken cancellationToken = default);
}

public sealed record VenueProvisioningResult(Guid VenueId, VenueSubscription Subscription);
