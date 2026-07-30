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
}
