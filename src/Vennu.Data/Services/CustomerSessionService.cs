using System.Security.Cryptography;
using System.Text;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class CustomerSessionService(
    ICustomerAuthenticationRepository authenticationRepository,
    ICustomerIdentityRepository identityRepository,
    CustomerSessionPolicy policy,
    TimeProvider timeProvider) : ICustomerSessionService
{
    public async Task<CustomerSessionIssue> IssueAsync(
        Guid userId,
        CustomerAuthenticationMethod method,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A user identifier is required.", nameof(userId));
        if (!Enum.IsDefined(method)) throw new ArgumentOutOfRangeException(nameof(method));
        var user = await identityRepository.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.Status != CustomerUserStatus.Active)
            throw new UnauthorizedAccessException("An active customer account is required.");

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var token = CreateToken();
        var session = await authenticationRepository.CreateSessionAsync(new CustomerAuthSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(token),
            AuthenticationMethod = method,
            AuthenticatedUtc = utcNow,
            LastSeenUtc = utcNow,
            ExpiresUtc = utcNow.Add(policy.AbsoluteLifetime),
            CreatedUtc = utcNow
        }, cancellationToken).ConfigureAwait(false);
        return new CustomerSessionIssue(token, session, user);
    }

    public async Task<CustomerSessionIdentity?> AuthenticateAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var session = await authenticationRepository.GetSessionByTokenHashAsync(Hash(token), cancellationToken)
            .ConfigureAwait(false);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        if (session is null || session.RevokedUtc is not null || session.ExpiresUtc <= utcNow ||
            session.LastSeenUtc.Add(policy.IdleLifetime) <= utcNow)
            return null;
        var user = await identityRepository.GetUserByIdAsync(session.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.Status != CustomerUserStatus.Active) return null;

        if (session.LastSeenUtc.Add(policy.TouchInterval) <= utcNow)
        {
            if (!await authenticationRepository.TouchSessionAsync(session.Id, utcNow, cancellationToken).ConfigureAwait(false))
                return null;
            session.LastSeenUtc = utcNow;
        }
        return new CustomerSessionIdentity(session, user);
    }

    public Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return authenticationRepository.RevokeSessionAsync(
            Hash(token), timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }

    internal static string CreateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    internal static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
