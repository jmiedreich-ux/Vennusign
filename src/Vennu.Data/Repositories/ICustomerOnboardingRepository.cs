using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ICustomerOnboardingRepository
{
    Task<CustomerOnboardingState?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CustomerOnboardingState> SaveAsync(
        CustomerOnboardingState state,
        CancellationToken cancellationToken = default);
}
