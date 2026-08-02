using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IOrganizationSubscriptionRepository
{
    Task<IReadOnlyCollection<OrganizationSubscription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrganizationSubscription?> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<OrganizationSubscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task<bool> SaveAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default);
}
