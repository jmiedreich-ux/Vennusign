using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IHaasContractRepository
{
    Task<HaasContract?> GetCurrentByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<HaasContract?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task<bool> SaveAsync(HaasContract contract, CancellationToken cancellationToken = default);
}
