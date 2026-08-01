using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ICustomerIdentityRepository
{
    Task<CustomerUser> CreateUserAsync(CustomerUser user, CancellationToken cancellationToken = default);
    Task<CustomerUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ExternalIdentity> LinkExternalIdentityAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);
    Task<ExternalIdentity?> GetExternalIdentityAsync(
        ExternalIdentityProvider provider,
        string providerSubject,
        CancellationToken cancellationToken = default);
}
