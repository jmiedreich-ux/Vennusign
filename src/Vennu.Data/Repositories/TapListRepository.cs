using System.Text.Json;
using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class TapListRepository(ISqlDataAccess dataAccess) : ITapListRepository
{
    private const string CategoriesSql = """
        SELECT Id, VenueId, Name, CategoryPrice, SortOrder, IsActive, CreatedUtc, UpdatedUtc
        FROM dbo.TapCategories
        WHERE VenueId = @VenueId
        ORDER BY SortOrder, Id;
        """;

    private const string ItemsSql = """
        SELECT Id, VenueId, TapCategoryId, Name, Style, Abv, Ibu, Description, Price,
               GlassColor, NameColor, IsAvailable, IsComingSoon, SortOrder, CreatedUtc, UpdatedUtc
        FROM dbo.TapItems
        WHERE VenueId = @VenueId
        ORDER BY SortOrder, Id;
        """;

    private const string DeleteCategorySql = """
        DELETE FROM dbo.TapCategories WHERE VenueId = @VenueId AND Id = @Id;
        SELECT CONVERT(BIT, CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END) AS Removed;
        """;

    private const string DeleteItemSql = """
        DELETE FROM dbo.TapItems WHERE VenueId = @VenueId AND Id = @Id;
        SELECT CONVERT(BIT, CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END) AS Removed;
        """;

    public async Task<IReadOnlyCollection<TapCategory>> GetCategoriesAsync(
        Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<TapCategory, object>(
            CategoriesSql, new { VenueId = Require(venueId, nameof(venueId)) }, cancellationToken)
            .ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<TapItem>> GetItemsAsync(
        Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<TapItem, object>(
            ItemsSql, new { VenueId = Require(venueId, nameof(venueId)) }, cancellationToken)
            .ConfigureAwait(false)).ToArray();

    public Task<Guid> CreateCategoryAsync(TapCategory category, CancellationToken cancellationToken = default) =>
        InsertAsync(category, cancellationToken);

    public Task<Guid> CreateItemAsync(TapItem item, CancellationToken cancellationToken = default) =>
        InsertAsync(item, cancellationToken);

    public async Task<bool> UpdateCategoryAsync(TapCategory category, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(category);
        return await dataAccess.UpdateAsync(category, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<bool> UpdateItemAsync(TapItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        return await dataAccess.UpdateAsync(item, cancellationToken).ConfigureAwait(false) > 0;
    }

    public Task<bool> DeleteCategoryAsync(Guid venueId, Guid categoryId, CancellationToken cancellationToken = default) =>
        DeleteAsync(DeleteCategorySql, venueId, categoryId, cancellationToken);

    public Task<bool> DeleteItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
        DeleteAsync(DeleteItemSql, venueId, itemId, cancellationToken);

    public Task<int> ReorderCategoriesAsync(
        Guid venueId, IReadOnlyCollection<Guid> categoryIds, DateTime updatedUtc,
        CancellationToken cancellationToken = default) =>
        ReorderAsync("TapCategories", venueId, categoryIds, updatedUtc, cancellationToken);

    public Task<int> ReorderItemsAsync(
        Guid venueId, IReadOnlyCollection<Guid> itemIds, DateTime updatedUtc,
        CancellationToken cancellationToken = default) =>
        ReorderAsync("TapItems", venueId, itemIds, updatedUtc, cancellationToken);

    private async Task<bool> DeleteAsync(
        string sql, Guid venueId, Guid id, CancellationToken cancellationToken) =>
        (await dataAccess.ExecuteSqlQueryAsync<RemovalResult, object>(
            sql,
            new { VenueId = Require(venueId, nameof(venueId)), Id = Require(id, nameof(id)) },
            cancellationToken).ConfigureAwait(false)).Single().Removed;

    private async Task<int> ReorderAsync(
        string table, Guid venueId, IReadOnlyCollection<Guid> ids, DateTime updatedUtc,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var sql = $"""
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;
            DECLARE @Requested TABLE (Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, SortOrder INT NOT NULL UNIQUE);
            INSERT INTO @Requested (Id, SortOrder)
            SELECT Id, SortOrder FROM OPENJSON(@RowsJson)
            WITH (Id UNIQUEIDENTIFIER '$.id', SortOrder INT '$.sortOrder');
            DECLARE @ExpectedCount INT = (SELECT COUNT(*) FROM dbo.{table} WHERE VenueId = @VenueId);
            IF @ExpectedCount <> (SELECT COUNT(*) FROM @Requested)
               OR EXISTS (SELECT 1 FROM @Requested WHERE SortOrder < 0 OR SortOrder >= @ExpectedCount)
               OR EXISTS (
                  SELECT 1 FROM @Requested requested
                  LEFT JOIN dbo.{table} row ON row.Id = requested.Id AND row.VenueId = @VenueId
                  WHERE row.Id IS NULL)
                THROW 51000, 'Order must contain every venue row exactly once.', 1;
            DECLARE @Offset INT = (SELECT ISNULL(MAX(SortOrder), 0) + @ExpectedCount + 1 FROM dbo.{table} WHERE VenueId = @VenueId);
            UPDATE dbo.{table} SET SortOrder = SortOrder + @Offset WHERE VenueId = @VenueId;
            UPDATE row SET SortOrder = requested.SortOrder, UpdatedUtc = @UpdatedUtc
            FROM dbo.{table} row INNER JOIN @Requested requested ON requested.Id = row.Id
            WHERE row.VenueId = @VenueId;
            COMMIT TRANSACTION;
            SELECT @ExpectedCount AS ChangedCount;
            """;
        var rowsJson = JsonSerializer.Serialize(ids.Select((id, sortOrder) => new { id, sortOrder }), JsonSerializerOptions.Web);
        return (await dataAccess.ExecuteSqlQueryAsync<OrderResult, object>(
            sql,
            new { VenueId = Require(venueId, nameof(venueId)), RowsJson = rowsJson, UpdatedUtc = updatedUtc },
            cancellationToken).ConfigureAwait(false)).Single().ChangedCount;
    }

    private async Task<Guid> InsertAsync<T>(T entity, CancellationToken cancellationToken) where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        var idProperty = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} must expose an Id.");
        var id = (Guid)(idProperty.GetValue(entity) ?? Guid.Empty);
        if (id == Guid.Empty) { id = Guid.NewGuid(); idProperty.SetValue(entity, id); }
        var now = DateTime.UtcNow;
        SetDefault(entity, "CreatedUtc", now);
        SetDefault(entity, "UpdatedUtc", now);
        await dataAccess.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
        return id;
    }

    private static void SetDefault<T>(T entity, string name, DateTime value) where T : class
    {
        var property = typeof(T).GetProperty(name);
        if (property?.GetValue(entity) is DateTime current && current == default) property.SetValue(entity, value);
    }

    private static Guid Require(Guid value, string parameterName) =>
        value == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.", parameterName) : value;

    public sealed class RemovalResult { public bool Removed { get; set; } }
    public sealed class OrderResult { public int ChangedCount { get; set; } }
}
