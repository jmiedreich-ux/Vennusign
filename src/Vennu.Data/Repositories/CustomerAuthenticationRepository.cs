using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class CustomerAuthenticationRepository(ISqlDataAccess dataAccess)
    : ICustomerAuthenticationRepository
{
    private const string SessionSql = """
        SELECT Id, UserId, TokenHash, AuthenticationMethod, Assurance, AuthenticatedUtc,
               StepUpUtc, LastSeenUtc, ExpiresUtc, RevokedUtc, CreatedUtc
        FROM dbo.CustomerAuthSessions
        WHERE TokenHash = @TokenHash;
        """;

    public async Task<CustomerAuthSession> CreateSessionAsync(
        CustomerAuthSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        ValidateHash(session.TokenHash, nameof(session.TokenHash));
        if (!Enum.IsDefined(session.AuthenticationMethod))
            throw new ArgumentOutOfRangeException(nameof(session.AuthenticationMethod));
        if (await dataAccess.InsertAsync(session, cancellationToken).ConfigureAwait(false) <= 0)
            throw new InvalidOperationException("The customer session could not be persisted.");
        return session;
    }

    public async Task<CustomerAuthSession?> GetSessionByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<CustomerAuthSession, object>(
            SessionSql,
            new { TokenHash = ValidateHash(tokenHash, nameof(tokenHash)) },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<bool> TouchSessionAsync(
        Guid sessionId,
        DateTime lastSeenUtc,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            """
            UPDATE dbo.CustomerAuthSessions
            SET LastSeenUtc = @LastSeenUtc
            OUTPUT CAST(1 AS BIT) Applied
            WHERE Id = @SessionId AND RevokedUtc IS NULL AND ExpiresUtc > @LastSeenUtc;
            """,
            new { SessionId = RequireId(sessionId, nameof(sessionId)), LastSeenUtc = RequireUtc(lastSeenUtc, nameof(lastSeenUtc)) },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task<bool> RevokeSessionAsync(
        string tokenHash,
        DateTime revokedUtc,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            """
            UPDATE dbo.CustomerAuthSessions
            SET RevokedUtc = @RevokedUtc
            OUTPUT CAST(1 AS BIT) Applied
            WHERE TokenHash = @TokenHash AND RevokedUtc IS NULL;
            """,
            new
            {
                TokenHash = ValidateHash(tokenHash, nameof(tokenHash)),
                RevokedUtc = RequireUtc(revokedUtc, nameof(revokedUtc))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task<bool> StepUpSessionAsync(Guid sessionId, CustomerAuthenticationMethod method, DateTime stepUpUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            """
            UPDATE dbo.CustomerAuthSessions
            SET AuthenticationMethod = @Method, Assurance = 2, StepUpUtc = @StepUpUtc
            OUTPUT CAST(1 AS BIT) Applied
            WHERE Id = @SessionId AND RevokedUtc IS NULL AND ExpiresUtc > @StepUpUtc;
            """,
            new { SessionId = RequireId(sessionId, nameof(sessionId)), Method = (int)method, StepUpUtc = RequireUtc(stepUpUtc, nameof(stepUpUtc)) },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task<EmailLoginToken> CreateEmailLoginTokenAsync(
        EmailLoginToken token,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        ValidateHash(token.TokenHash, nameof(token.TokenHash));
        ValidateReturnPath(token.ReturnPath);
        if (await dataAccess.InsertAsync(token, cancellationToken).ConfigureAwait(false) <= 0)
            throw new InvalidOperationException("The email login token could not be persisted.");
        return token;
    }

    public async Task<EmailLoginToken?> ConsumeEmailLoginTokenAsync(
        string tokenHash,
        DateTime consumedUtc,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<EmailLoginToken, object>(
            """
            UPDATE dbo.EmailLoginTokens WITH (UPDLOCK, ROWLOCK)
            SET ConsumedUtc = @ConsumedUtc
            OUTPUT inserted.Id, inserted.UserId, inserted.TokenHash, inserted.ReturnPath,
                   inserted.ExpiresUtc, inserted.ConsumedUtc, inserted.CreatedUtc
            WHERE TokenHash = @TokenHash AND ConsumedUtc IS NULL AND ExpiresUtc > @ConsumedUtc;
            """,
            new
            {
                TokenHash = ValidateHash(tokenHash, nameof(tokenHash)),
                ConsumedUtc = RequireUtc(consumedUtc, nameof(consumedUtc))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<IReadOnlyList<CustomerPasskeyCredential>> GetPasskeysAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<CustomerPasskeyCredential, object>(
            "SELECT Id, UserId, CredentialId, PublicKey, UserHandle, SignatureCounter, DisplayName, CreatedUtc, LastUsedUtc, RevokedUtc FROM dbo.CustomerPasskeyCredentials WHERE UserId = @UserId AND RevokedUtc IS NULL ORDER BY CreatedUtc, Id;",
            new { UserId = RequireId(userId, nameof(userId)) }, cancellationToken).ConfigureAwait(false)).ToList();

    public async Task<CustomerPasskeyCredential?> GetPasskeyByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credentialId);
        if (credentialId.Length is < 16 or > 1024) throw new ArgumentException("A bounded credential identifier is required.", nameof(credentialId));
        return (await dataAccess.ExecuteSqlQueryAsync<CustomerPasskeyCredential, object>(
            "SELECT Id, UserId, CredentialId, PublicKey, UserHandle, SignatureCounter, DisplayName, CreatedUtc, LastUsedUtc, RevokedUtc FROM dbo.CustomerPasskeyCredentials WHERE CredentialId = @CredentialId AND RevokedUtc IS NULL;",
            new { CredentialId = credentialId }, cancellationToken).ConfigureAwait(false)).SingleOrDefault();
    }

    public async Task<CustomerPasskeyCredential> CreatePasskeyAsync(CustomerPasskeyCredential credential, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(credential);
        if (credential.UserId == Guid.Empty || credential.CredentialId.Length is < 16 or > 1024 || credential.PublicKey.Length == 0)
            throw new ArgumentException("A complete passkey credential is required.", nameof(credential));
        if (await dataAccess.InsertAsync(credential, cancellationToken).ConfigureAwait(false) <= 0)
            throw new InvalidOperationException("The passkey could not be persisted.");
        return credential;
    }

    public async Task<bool> RenamePasskeyAsync(Guid userId, Guid id, string displayName, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            "UPDATE dbo.CustomerPasskeyCredentials SET DisplayName=@DisplayName OUTPUT CAST(1 AS BIT) Applied WHERE Id=@Id AND UserId=@UserId AND RevokedUtc IS NULL;",
            new { UserId = RequireId(userId, nameof(userId)), Id = RequireId(id, nameof(id)), DisplayName = displayName }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task<bool> RevokePasskeyAsync(Guid userId, Guid id, DateTime revokedUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            """
            SET XACT_ABORT ON; SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; BEGIN TRANSACTION;
            UPDATE dbo.CustomerPasskeyCredentials WITH (UPDLOCK, HOLDLOCK)
            SET RevokedUtc=@RevokedUtc OUTPUT CAST(1 AS BIT) Applied
            WHERE Id=@Id AND UserId=@UserId AND RevokedUtc IS NULL
              AND ((SELECT COUNT(*) FROM dbo.CustomerPasskeyCredentials WITH (UPDLOCK, HOLDLOCK) WHERE UserId=@UserId AND RevokedUtc IS NULL) > 1
                   OR EXISTS (SELECT 1 FROM dbo.CustomerUsers WHERE Id=@UserId AND EmailVerifiedUtc IS NOT NULL));
            COMMIT;
            -- Isolation level outlives the batch and rides the pooled connection to the
            -- next caller. Leaving it SERIALIZABLE breaks unrelated queries downstream.
            SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
            """,
            new { UserId = RequireId(userId, nameof(userId)), Id = RequireId(id, nameof(id)), RevokedUtc = RequireUtc(revokedUtc, nameof(revokedUtc)) }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task<bool> UpdatePasskeyCounterAsync(Guid id, uint counter, DateTime usedUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            "UPDATE dbo.CustomerPasskeyCredentials SET SignatureCounter=@Counter, LastUsedUtc=@UsedUtc OUTPUT CAST(1 AS BIT) Applied WHERE Id=@Id AND RevokedUtc IS NULL AND SignatureCounter <= @Counter;",
            new { Id = RequireId(id, nameof(id)), Counter = counter, UsedUtc = RequireUtc(usedUtc, nameof(usedUtc)) }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task<CustomerAuthenticationChallenge> CreateChallengeAsync(CustomerAuthenticationChallenge challenge, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        if (challenge.UserId == Guid.Empty || string.IsNullOrWhiteSpace(challenge.ProtectedOptions)) throw new ArgumentException("A bound challenge is required.", nameof(challenge));
        if (await dataAccess.InsertAsync(challenge, cancellationToken).ConfigureAwait(false) <= 0) throw new InvalidOperationException("The challenge could not be persisted.");
        return challenge;
    }

    public async Task<CustomerAuthenticationChallenge?> ConsumeChallengeAsync(Guid id, CustomerAuthenticationChallengeType type, DateTime consumedUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<CustomerAuthenticationChallenge, object>(
            """
            UPDATE dbo.CustomerAuthenticationChallenges WITH (UPDLOCK, ROWLOCK)
            SET ConsumedUtc=@ConsumedUtc
            OUTPUT inserted.Id, inserted.UserId, inserted.Type, inserted.ProtectedOptions, inserted.ExpiresUtc, inserted.ConsumedUtc, inserted.CreatedUtc
            WHERE Id=@Id AND Type=@Type AND ConsumedUtc IS NULL AND ExpiresUtc > @ConsumedUtc;
            """, new { Id = RequireId(id, nameof(id)), Type = (int)type, ConsumedUtc = RequireUtc(consumedUtc, nameof(consumedUtc)) }, cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<CustomerTotpAuthenticator?> GetTotpAsync(Guid userId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<CustomerTotpAuthenticator, object>(
            "SELECT Id, UserId, ProtectedSecret, CreatedUtc, VerifiedUtc, LastAcceptedCounter, RevokedUtc FROM dbo.CustomerTotpAuthenticators WHERE UserId=@UserId AND RevokedUtc IS NULL;",
            new { UserId = RequireId(userId, nameof(userId)) }, cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<CustomerTotpAuthenticator> SaveTotpAsync(CustomerTotpAuthenticator authenticator, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(authenticator);
        if (authenticator.UserId == Guid.Empty || string.IsNullOrWhiteSpace(authenticator.ProtectedSecret)) throw new ArgumentException("A protected authenticator is required.", nameof(authenticator));
        await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            "UPDATE dbo.CustomerTotpAuthenticators SET RevokedUtc=@CreatedUtc OUTPUT CAST(1 AS BIT) Applied WHERE UserId=@UserId AND RevokedUtc IS NULL;",
            new { authenticator.UserId, authenticator.CreatedUtc }, cancellationToken).ConfigureAwait(false);
        if (await dataAccess.InsertAsync(authenticator, cancellationToken).ConfigureAwait(false) <= 0) throw new InvalidOperationException("The authenticator could not be persisted.");
        return authenticator;
    }

    public async Task<bool> VerifyTotpAsync(Guid id, DateTime verifiedUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            "UPDATE dbo.CustomerTotpAuthenticators SET VerifiedUtc=@VerifiedUtc OUTPUT CAST(1 AS BIT) Applied WHERE Id=@Id AND VerifiedUtc IS NULL AND RevokedUtc IS NULL;",
            new { Id = RequireId(id, nameof(id)), VerifiedUtc = RequireUtc(verifiedUtc, nameof(verifiedUtc)) }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task<bool> AcceptTotpCounterAsync(Guid id, long counter, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            "UPDATE dbo.CustomerTotpAuthenticators WITH (UPDLOCK, ROWLOCK) SET LastAcceptedCounter=@Counter OUTPUT CAST(1 AS BIT) Applied WHERE Id=@Id AND VerifiedUtc IS NOT NULL AND RevokedUtc IS NULL AND (LastAcceptedCounter IS NULL OR LastAcceptedCounter < @Counter);",
            new { Id = RequireId(id, nameof(id)), Counter = counter }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    public async Task ReplaceRecoveryCodesAsync(Guid userId, IReadOnlyList<CustomerRecoveryCode> codes, CancellationToken cancellationToken = default)
    {
        RequireId(userId, nameof(userId));
        if (codes.Count is < 1 or > 20 || codes.Any(code => code.UserId != userId)) throw new ArgumentException("A bounded user-owned recovery-code set is required.", nameof(codes));
        await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            "DELETE FROM dbo.CustomerRecoveryCodes OUTPUT CAST(1 AS BIT) Applied WHERE UserId=@UserId AND UsedUtc IS NULL;", new { UserId = userId }, cancellationToken).ConfigureAwait(false);
        if (await dataAccess.InsertAllAsync(codes, cancellationToken: cancellationToken).ConfigureAwait(false) != codes.Count) throw new InvalidOperationException("Recovery codes could not be persisted.");
    }

    public async Task<bool> ConsumeRecoveryCodeAsync(Guid userId, string codeHash, DateTime usedUtc, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MutationResult, object>(
            "UPDATE dbo.CustomerRecoveryCodes WITH (UPDLOCK, ROWLOCK) SET UsedUtc=@UsedUtc OUTPUT CAST(1 AS BIT) Applied WHERE UserId=@UserId AND CodeHash=@CodeHash AND UsedUtc IS NULL;",
            new { UserId = RequireId(userId, nameof(userId)), CodeHash = ValidateHash(codeHash, nameof(codeHash)), UsedUtc = RequireUtc(usedUtc, nameof(usedUtc)) }, cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Applied ?? false;

    private static string ValidateHash(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        if (value.Length != 64 || value.Any(character => !Uri.IsHexDigit(character)))
            throw new ArgumentException("A SHA-256 hexadecimal hash is required.", parameterName);
        return value.ToUpperInvariant();
    }

    private static string ValidateReturnPath(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!value.StartsWith('/') || value.StartsWith("//", StringComparison.Ordinal) || value.Length > 500)
            throw new ArgumentException("A bounded local return path is required.", nameof(value));
        return value;
    }

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty identifier is required.", parameterName);

    private static DateTime RequireUtc(DateTime value, string parameterName) =>
        value.Kind == DateTimeKind.Utc ? value : throw new ArgumentException("A UTC timestamp is required.", parameterName);

    private sealed class MutationResult { public bool Applied { get; set; } }
}
