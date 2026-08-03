using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ICustomerAuthenticationRepository
{
    Task<CustomerAuthSession> CreateSessionAsync(CustomerAuthSession session, CancellationToken cancellationToken = default);
    Task<CustomerAuthSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<bool> TouchSessionAsync(Guid sessionId, DateTime lastSeenUtc, CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(string tokenHash, DateTime revokedUtc, CancellationToken cancellationToken = default);
    Task<bool> StepUpSessionAsync(Guid sessionId, CustomerAuthenticationMethod method, DateTime stepUpUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
    Task<EmailLoginToken> CreateEmailLoginTokenAsync(EmailLoginToken token, CancellationToken cancellationToken = default);
    Task<EmailLoginToken?> ConsumeEmailLoginTokenAsync(string tokenHash, DateTime consumedUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerPasskeyCredential>> GetPasskeysAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CustomerPasskeyCredential>>([]);
    Task<CustomerPasskeyCredential?> GetPasskeyByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default) => Task.FromResult<CustomerPasskeyCredential?>(null);
    Task<CustomerPasskeyCredential> CreatePasskeyAsync(CustomerPasskeyCredential credential, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    Task<bool> RenamePasskeyAsync(Guid userId, Guid id, string displayName, CancellationToken cancellationToken = default) => Task.FromResult(false);
    Task<bool> RevokePasskeyAsync(Guid userId, Guid id, DateTime revokedUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
    Task<bool> UpdatePasskeyCounterAsync(Guid id, uint counter, DateTime usedUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
    Task<CustomerAuthenticationChallenge> CreateChallengeAsync(CustomerAuthenticationChallenge challenge, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    Task<CustomerAuthenticationChallenge?> ConsumeChallengeAsync(Guid id, CustomerAuthenticationChallengeType type, DateTime consumedUtc, CancellationToken cancellationToken = default) => Task.FromResult<CustomerAuthenticationChallenge?>(null);
    Task<CustomerTotpAuthenticator?> GetTotpAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<CustomerTotpAuthenticator?>(null);
    Task<CustomerTotpAuthenticator> SaveTotpAsync(CustomerTotpAuthenticator authenticator, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    Task<bool> VerifyTotpAsync(Guid id, DateTime verifiedUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
    Task<bool> AcceptTotpCounterAsync(Guid id, long counter, CancellationToken cancellationToken = default) => Task.FromResult(false);
    Task ReplaceRecoveryCodesAsync(Guid userId, IReadOnlyList<CustomerRecoveryCode> codes, CancellationToken cancellationToken = default) => Task.CompletedTask;
    Task<bool> ConsumeRecoveryCodeAsync(Guid userId, string codeHash, DateTime usedUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
}
