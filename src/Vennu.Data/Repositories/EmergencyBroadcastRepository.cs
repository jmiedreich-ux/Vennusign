using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class EmergencyBroadcastRepository(ISqlDataAccess dataAccess) : IEmergencyBroadcastRepository
{
    private const string ByVenueSql = """
        SELECT Id, VenueId, ScreenId, Title, Message, MediaUrl, StartsUtc, ExpiresUtc,
               IsActive, CreatedUtc, UpdatedUtc
        FROM dbo.EmergencyBroadcasts
        WHERE VenueId = @VenueId
        ORDER BY StartsUtc DESC, CreatedUtc DESC, Id;
        """;

    public async Task<IReadOnlyCollection<EmergencyBroadcast>> GetByVenueAsync(
        Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<EmergencyBroadcast, object>(
            ByVenueSql, new { VenueId = Require(venueId) }, cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<Guid> CreateAsync(EmergencyBroadcast broadcast, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(broadcast);
        if (broadcast.Id == Guid.Empty) broadcast.Id = Guid.NewGuid();
        await dataAccess.InsertAsync(broadcast, cancellationToken).ConfigureAwait(false);
        return broadcast.Id;
    }

    public async Task<bool> UpdateAsync(EmergencyBroadcast broadcast, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(broadcast);
        return await dataAccess.UpdateAsync(broadcast, cancellationToken).ConfigureAwait(false) > 0;
    }

    private static Guid Require(Guid id) =>
        id == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.") : id;
}
