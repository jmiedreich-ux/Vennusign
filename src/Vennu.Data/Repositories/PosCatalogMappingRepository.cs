using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class PosCatalogMappingRepository(ISqlDataAccess dataAccess, TimeProvider timeProvider)
    : IPosCatalogMappingRepository
{
    private const string GetAllSql = """
        SELECT Id, VenueId, Provider, EntityType, ExternalId, LocalEntityId, CreatedUtc, UpdatedUtc
        FROM dbo.PosCatalogMappings
        WHERE VenueId = @VenueId AND Provider = @Provider
        ORDER BY EntityType, ExternalId, Id;
        """;

    private const string GetSql = """
        SELECT Id, VenueId, Provider, EntityType, ExternalId, LocalEntityId, CreatedUtc, UpdatedUtc
        FROM dbo.PosCatalogMappings
        WHERE VenueId = @VenueId AND Provider = @Provider
          AND EntityType = @EntityType AND ExternalId = @ExternalId;
        """;

    private const string GetMappedItemSql = """
        SELECT item.Id, item.VenueId, item.MenuSectionId, item.Name, item.Description,
               item.Price, item.HappyHourPrice, item.IsAvailable, item.AvailabilityResetUtc,
               item.QuantityAvailable, item.Tags, item.ImageUrl, item.IsPopular, item.SortOrder,
               item.CreatedUtc, item.UpdatedUtc
        FROM dbo.PosCatalogMappings mapping
        INNER JOIN dbo.MenuItems item
            ON item.Id = mapping.LocalEntityId AND item.VenueId = mapping.VenueId
        WHERE mapping.VenueId = @VenueId
          AND mapping.Provider = @Provider
          AND mapping.EntityType = @EntityType
          AND mapping.ExternalId = @ExternalId;
        """;

    public async Task<IReadOnlyCollection<PosCatalogMapping>> GetAllAsync(
        Guid venueId,
        PosProvider provider,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PosCatalogMapping, object>(
            GetAllSql,
            new { VenueId = RequireId(venueId), Provider = RequireProvider(provider) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<MenuItem?> GetMappedItemAsync(
        Guid venueId,
        PosProvider provider,
        string externalItemId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuItem, object>(
            GetMappedItemSql,
            new
            {
                VenueId = RequireId(venueId),
                Provider = RequireProvider(provider),
                EntityType = (int)PosCatalogEntityType.Item,
                ExternalId = RequireExternalId(externalItemId)
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<PosCatalogMapping> SaveAsync(
        Guid venueId,
        PosCatalogMapping mapping,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        RequireId(venueId);
        if (mapping.VenueId != venueId) throw new ArgumentException("The mapping must belong to the requested venue.", nameof(mapping));
        RequireProvider(mapping.Provider);
        if (!Enum.IsDefined(mapping.EntityType)) throw new ArgumentOutOfRangeException(nameof(mapping));
        if (mapping.LocalEntityId == Guid.Empty) throw new ArgumentException("A local entity identifier is required.", nameof(mapping));
        mapping.ExternalId = RequireExternalId(mapping.ExternalId);

        var existing = (await dataAccess.ExecuteSqlQueryAsync<PosCatalogMapping, object>(
            GetSql,
            new
            {
                VenueId = venueId,
                Provider = (int)mapping.Provider,
                EntityType = (int)mapping.EntityType,
                mapping.ExternalId
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (existing is null)
        {
            mapping.Id = mapping.Id == Guid.Empty ? Guid.NewGuid() : mapping.Id;
            mapping.CreatedUtc = mapping.CreatedUtc == default ? now : mapping.CreatedUtc;
            mapping.UpdatedUtc = now;
            if (await dataAccess.InsertAsync(mapping, cancellationToken).ConfigureAwait(false) <= 0)
                throw new InvalidOperationException("The POS catalog mapping could not be persisted.");
            return mapping;
        }

        existing.LocalEntityId = mapping.LocalEntityId;
        existing.UpdatedUtc = now;
        if (await dataAccess.UpdateAsync(existing, cancellationToken).ConfigureAwait(false) <= 0)
            throw new InvalidOperationException("The POS catalog mapping could not be persisted.");
        return existing;
    }

    private static Guid RequireId(Guid value) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty venue identifier is required.", nameof(value));

    private static int RequireProvider(PosProvider provider) =>
        Enum.IsDefined(provider) ? (int)provider : throw new ArgumentOutOfRangeException(nameof(provider));

    private static string RequireExternalId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var result = value.Trim();
        return result.Length <= 300 ? result : throw new ArgumentException("External identifiers cannot exceed 300 characters.", nameof(value));
    }
}
