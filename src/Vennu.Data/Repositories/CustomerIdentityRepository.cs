using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class CustomerIdentityRepository(ISqlDataAccess dataAccess, TimeProvider timeProvider)
    : ICustomerIdentityRepository
{
    private const string UserByEmailSql = """
        SELECT Id, Email, NormalizedEmail, DisplayName, Status, EmailVerifiedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.CustomerUsers
        WHERE NormalizedEmail = @NormalizedEmail;
        """;

    private const string ExternalIdentitySql = """
        SELECT Id, UserId, Provider, ProviderSubject, CreatedUtc, UpdatedUtc
        FROM dbo.ExternalIdentities
        WHERE Provider = @Provider AND ProviderSubject = @ProviderSubject;
        """;

    public async Task<CustomerUser> CreateUserAsync(
        CustomerUser user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        var email = NormalizeRequired(user.Email, 320, nameof(user.Email));
        var displayName = NormalizeRequired(user.DisplayName, 200, nameof(user.DisplayName));
        if (!Enum.IsDefined(user.Status))
            throw new ArgumentOutOfRangeException(nameof(user.Status));

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
        user.Email = email;
        user.NormalizedEmail = email.ToUpperInvariant();
        user.DisplayName = displayName;
        user.CreatedUtc = user.CreatedUtc == default ? utcNow : user.CreatedUtc;
        user.UpdatedUtc = utcNow;

        if (await dataAccess.InsertAsync(user, cancellationToken).ConfigureAwait(false) <= 0)
            throw new InvalidOperationException("The customer user could not be persisted.");

        return user;
    }

    public Task<CustomerUser?> GetUserByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        dataAccess.QueryAsync<CustomerUser>(new { Id = RequireId(userId, nameof(userId)) }, cancellationToken);

    public async Task<CustomerUser?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<CustomerUser, object>(
            UserByEmailSql,
            new { NormalizedEmail = NormalizeRequired(email, 320, nameof(email)).ToUpperInvariant() },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<ExternalIdentity> LinkExternalIdentityAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identity);
        RequireId(identity.UserId, nameof(identity.UserId));
        RequireProvider(identity.Provider);
        identity.ProviderSubject = NormalizeRequired(identity.ProviderSubject, 255, nameof(identity.ProviderSubject));
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        identity.Id = identity.Id == Guid.Empty ? Guid.NewGuid() : identity.Id;
        identity.CreatedUtc = identity.CreatedUtc == default ? utcNow : identity.CreatedUtc;
        identity.UpdatedUtc = utcNow;

        if (await dataAccess.InsertAsync(identity, cancellationToken).ConfigureAwait(false) <= 0)
            throw new InvalidOperationException("The external identity could not be persisted.");

        return identity;
    }

    public async Task<ExternalIdentity?> GetExternalIdentityAsync(
        ExternalIdentityProvider provider,
        string providerSubject,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ExternalIdentity, object>(
            ExternalIdentitySql,
            new
            {
                Provider = RequireProvider(provider),
                ProviderSubject = NormalizeRequired(providerSubject, 255, nameof(providerSubject))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty identifier is required.", parameterName);

    private static int RequireProvider(ExternalIdentityProvider provider) =>
        Enum.IsDefined(provider) ? (int)provider : throw new ArgumentOutOfRangeException(nameof(provider));

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"The value cannot exceed {maxLength} characters.", parameterName);
        return normalized;
    }
}
