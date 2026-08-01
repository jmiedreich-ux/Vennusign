using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ICustomerAuthenticationRepository
{
    Task<CustomerAuthSession> CreateSessionAsync(CustomerAuthSession session, CancellationToken cancellationToken = default);
    Task<CustomerAuthSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<bool> TouchSessionAsync(Guid sessionId, DateTime lastSeenUtc, CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(string tokenHash, DateTime revokedUtc, CancellationToken cancellationToken = default);
    Task<EmailLoginToken> CreateEmailLoginTokenAsync(EmailLoginToken token, CancellationToken cancellationToken = default);
    Task<EmailLoginToken?> ConsumeEmailLoginTokenAsync(string tokenHash, DateTime consumedUtc, CancellationToken cancellationToken = default);
}
