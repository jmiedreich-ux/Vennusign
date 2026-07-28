using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface ISubscriptionManagementService
{
    Task<VenueSubscription> StartTrialAsync(Guid venueId, Guid tierId, CancellationToken cancellationToken = default);
    Task<VenueSubscription> ChangeTierAsync(Guid venueId, Guid tierId, CancellationToken cancellationToken = default);
    Task<VenueSubscription> SetStatusAsync(Guid venueId, string status, DateTime? currentPeriodEnd = null, CancellationToken cancellationToken = default);
    Task<int> ExpireTrialsAsync(CancellationToken cancellationToken = default);
}
