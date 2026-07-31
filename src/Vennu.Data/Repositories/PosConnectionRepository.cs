using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class PosConnectionRepository(ISqlDataAccess dataAccess, TimeProvider timeProvider)
    : IPosConnectionRepository
{
    private const string GetSql = """
        SELECT Id, VenueId, Provider, Status, ExternalMerchantId,
               ProtectedAccessToken, ProtectedRefreshToken, AccessTokenExpiresUtc,
               LastSyncedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.PosConnections
        WHERE VenueId = @VenueId AND Provider = @Provider;
        """;

    private const string GetAllSql = """
        SELECT Id, VenueId, Provider, Status, ExternalMerchantId,
               ProtectedAccessToken, ProtectedRefreshToken, AccessTokenExpiresUtc,
               LastSyncedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.PosConnections
        WHERE VenueId = @VenueId
        ORDER BY Provider, Id;
        """;

    public async Task<PosConnection?> GetAsync(
        Guid venueId,
        PosProvider provider,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PosConnection, object>(
            GetSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                Provider = RequireProvider(provider)
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PosConnection, object>(
            GetAllSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<PosConnection> SaveAsync(
        Guid venueId,
        PosConnection connection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        RequireId(venueId, nameof(venueId));
        if (connection.VenueId != venueId)
        {
            throw new ArgumentException("The connection must belong to the requested venue.", nameof(connection));
        }

        RequireProvider(connection.Provider);
        var existing = await GetAsync(venueId, connection.Provider, cancellationToken).ConfigureAwait(false);
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (existing is null)
        {
            connection.Id = connection.Id == Guid.Empty ? Guid.NewGuid() : connection.Id;
            connection.CreatedUtc = connection.CreatedUtc == default ? utcNow : connection.CreatedUtc;
            connection.UpdatedUtc = utcNow;
            var inserted = await dataAccess.InsertAsync(connection, cancellationToken).ConfigureAwait(false);
            if (inserted <= 0)
            {
                throw new InvalidOperationException("The POS connection could not be persisted.");
            }

            return connection;
        }

        existing.Status = connection.Status;
        existing.ExternalMerchantId = connection.ExternalMerchantId;
        existing.ProtectedAccessToken = connection.ProtectedAccessToken;
        existing.ProtectedRefreshToken = connection.ProtectedRefreshToken;
        existing.AccessTokenExpiresUtc = connection.AccessTokenExpiresUtc;
        existing.LastSyncedUtc = connection.LastSyncedUtc;
        existing.UpdatedUtc = utcNow;
        var updated = await dataAccess.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
        if (updated <= 0)
        {
            throw new InvalidOperationException("The POS connection could not be persisted.");
        }

        return existing;
    }

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty identifier is required.", parameterName);

    private static int RequireProvider(PosProvider provider) =>
        Enum.IsDefined(provider) ? (int)provider : throw new ArgumentOutOfRangeException(nameof(provider));
}
