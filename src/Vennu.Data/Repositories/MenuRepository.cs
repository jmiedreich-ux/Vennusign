using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class MenuRepository(ISqlDataAccess dataAccess) : IMenuRepository
{
    private const string MenusSql = """
        SELECT Id, VenueId, Name, IsActive, CreatedUtc, UpdatedUtc
        FROM dbo.Menus
        WHERE VenueId = @VenueId
        ORDER BY Name, Id;
        """;

    private const string SectionsSql = """
        SELECT Id, VenueId, MenuId, Name, SortOrder, IsActive, CreatedUtc, UpdatedUtc
        FROM dbo.MenuSections
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY SortOrder, Id;
        """;

    private const string ItemsSql = """
        SELECT Id, VenueId, MenuSectionId, Name, Description, Price, HappyHourPrice,
               IsAvailable, QuantityAvailable, Tags, ImageUrl, IsPopular, SortOrder,
               CreatedUtc, UpdatedUtc
        FROM dbo.MenuItems
        WHERE VenueId = @VenueId AND MenuSectionId = @MenuSectionId
        ORDER BY SortOrder, Id;
        """;

    private const string TranslationsSql = """
        SELECT Id, VenueId, MenuItemId, LanguageCode, Name, Description,
               IsAutoTranslated, CreatedUtc, UpdatedUtc
        FROM dbo.MenuItemTranslations
        WHERE VenueId = @VenueId AND MenuItemId = @MenuItemId
        ORDER BY LanguageCode, Id;
        """;

    public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) =>
        InsertAsync(menu, cancellationToken);

    public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) =>
        InsertAsync(section, cancellationToken);

    public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default) =>
        InsertAsync(item, cancellationToken);

    public Task<Guid> CreateTranslationAsync(MenuItemTranslation translation, CancellationToken cancellationToken = default) =>
        InsertAsync(translation, cancellationToken);

    public async Task<IReadOnlyCollection<Menu>> GetMenusAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<Menu, object>(
            MenusSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<MenuSection>> GetSectionsAsync(
        Guid venueId,
        Guid menuId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuSection, object>(
            SectionsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<MenuItem>> GetItemsAsync(
        Guid venueId,
        Guid sectionId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuItem, object>(
            ItemsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuSectionId = RequireId(sectionId, nameof(sectionId))
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<MenuItemTranslation>> GetTranslationsAsync(
        Guid venueId,
        Guid itemId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuItemTranslation, object>(
            TranslationsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuItemId = RequireId(itemId, nameof(itemId))
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    private async Task<Guid> InsertAsync<T>(T entity, CancellationToken cancellationToken)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);

        var idProperty = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} must expose an Id property.");
        var id = (Guid)(idProperty.GetValue(entity) ?? Guid.Empty);
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
            idProperty.SetValue(entity, id);
        }

        var now = DateTime.UtcNow;
        SetDefaultDate(entity, "CreatedUtc", now);
        SetDefaultDate(entity, "UpdatedUtc", now);
        await dataAccess.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
        return id;
    }

    private static Guid RequireId(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException("Identifier cannot be empty.", parameterName)
            : value;

    private static void SetDefaultDate<T>(T entity, string propertyName, DateTime value)
        where T : class
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property?.GetValue(entity) is DateTime current && current == default)
        {
            property.SetValue(entity, value);
        }
    }
}
