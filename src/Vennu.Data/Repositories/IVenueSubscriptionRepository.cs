using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IVenueSubscriptionRepository
{
    Task<IReadOnlyCollection<VenueSubscription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VenueSubscription?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<bool> SaveAsync(VenueSubscription subscription, CancellationToken cancellationToken = default);
}
