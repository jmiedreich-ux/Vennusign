using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface ITierManagementService
{
    Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionTier> CreateAsync(TierManagementRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> UpdateAsync(Guid tierId, TierManagementRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionTier?> CloneAsync(Guid tierId, CancellationToken cancellationToken = default);
    Task<bool> ArchiveAsync(Guid tierId, CancellationToken cancellationToken = default);
}
