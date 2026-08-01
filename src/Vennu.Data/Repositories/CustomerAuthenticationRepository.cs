using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class CustomerAuthenticationRepository(ISqlDataAccess dataAccess)
    : ICustomerAuthenticationRepository
{
    private const string SessionSql = """
        SELECT Id, UserId, TokenHash, AuthenticationMethod, AuthenticatedUtc,
               LastSeenUtc, ExpiresUtc, RevokedUtc, CreatedUtc
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
