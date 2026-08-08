using System.Text.Json;
using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class MenuRepository(ISqlDataAccess dataAccess) : IMenuRepository
{
    private const string MenusSql = """
        SELECT Id, VenueId, Name, IsActive, DailySpecial, CreatedUtc, UpdatedUtc
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
               IsAvailable, QuantityAvailable, Tags, ImageUrl, IsPopular, IsActive, SortOrder,
               CreatedUtc, UpdatedUtc
        FROM dbo.MenuItems
        WHERE VenueId = @VenueId AND MenuSectionId = @MenuSectionId
        ORDER BY SortOrder, Id;
        """;

    private const string ReorderSectionsSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Requested TABLE
        (
            Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
            SortOrder INT NOT NULL UNIQUE
        );

        INSERT INTO @Requested (Id, SortOrder)
        SELECT Id, SortOrder
        FROM OPENJSON(@SectionsJson)
        WITH (Id UNIQUEIDENTIFIER '$.id', SortOrder INT '$.sortOrder');

        DECLARE @ExpectedCount INT =
        (
            SELECT COUNT(*)
            FROM dbo.MenuSections
            WHERE VenueId = @VenueId AND MenuId = @MenuId
        );

        IF @ExpectedCount <> (SELECT COUNT(*) FROM @Requested)
           OR EXISTS
           (
               SELECT 1 FROM @Requested
               WHERE SortOrder < 0 OR SortOrder >= @ExpectedCount
           )
           OR EXISTS
           (
               SELECT 1
               FROM @Requested requested
               LEFT JOIN dbo.MenuSections section
                 ON section.Id = requested.Id
                AND section.VenueId = @VenueId
                AND section.MenuId = @MenuId
               WHERE section.Id IS NULL
           )
        BEGIN
            THROW 51000, 'Section order must contain every venue menu section exactly once.', 1;
        END;

        DECLARE @Offset INT =
        (
            SELECT ISNULL(MAX(SortOrder), 0) + @ExpectedCount + 1
            FROM dbo.MenuSections
            WHERE VenueId = @VenueId AND MenuId = @MenuId
        );

        UPDATE dbo.MenuSections
        SET SortOrder = SortOrder + @Offset
        WHERE VenueId = @VenueId AND MenuId = @MenuId;

        UPDATE section
        SET SortOrder = requested.SortOrder,
            UpdatedUtc = @UpdatedUtc
        FROM dbo.MenuSections section
        INNER JOIN @Requested requested ON requested.Id = section.Id
        WHERE section.VenueId = @VenueId AND section.MenuId = @MenuId;

        COMMIT TRANSACTION;
        SELECT @ExpectedCount AS ChangedCount;
        """;

    private const string ReorderItemsSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Requested TABLE
        (
            Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
            SortOrder INT NOT NULL UNIQUE
        );

        INSERT INTO @Requested (Id, SortOrder)
        SELECT Id, SortOrder
        FROM OPENJSON(@ItemsJson)
        WITH (Id UNIQUEIDENTIFIER '$.id', SortOrder INT '$.sortOrder');

        DECLARE @ExpectedCount INT =
        (
            SELECT COUNT(*)
            FROM dbo.MenuItems
            WHERE VenueId = @VenueId AND MenuSectionId = @MenuSectionId
        );

        IF @ExpectedCount <> (SELECT COUNT(*) FROM @Requested)
           OR EXISTS (SELECT 1 FROM @Requested WHERE SortOrder < 0 OR SortOrder >= @ExpectedCount)
           OR EXISTS
           (
               SELECT 1
               FROM @Requested requested
               LEFT JOIN dbo.MenuItems item
                 ON item.Id = requested.Id
                AND item.VenueId = @VenueId
                AND item.MenuSectionId = @MenuSectionId
               WHERE item.Id IS NULL
           )
        BEGIN
            THROW 51000, 'Item order must contain every venue menu item exactly once.', 1;
        END;

        DECLARE @Offset INT =
        (
            SELECT ISNULL(MAX(SortOrder), 0) + @ExpectedCount + 1
            FROM dbo.MenuItems
            WHERE VenueId = @VenueId AND MenuSectionId = @MenuSectionId
        );

        UPDATE dbo.MenuItems
        SET SortOrder = SortOrder + @Offset
        WHERE VenueId = @VenueId AND MenuSectionId = @MenuSectionId;

        UPDATE item
        SET SortOrder = requested.SortOrder,
            UpdatedUtc = @UpdatedUtc
        FROM dbo.MenuItems item
        INNER JOIN @Requested requested ON requested.Id = item.Id
        WHERE item.VenueId = @VenueId AND item.MenuSectionId = @MenuSectionId;

        COMMIT TRANSACTION;
        SELECT @ExpectedCount AS ChangedCount;
        """;

    public Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default) =>
        InsertAsync(menu, cancellationToken);

    public Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default) =>
        InsertAsync(section, cancellationToken);

    public Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default) =>
        InsertAsync(item, cancellationToken);

    public async Task<bool> UpdateSectionAsync(
        MenuSection section,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(section);
        return await dataAccess.UpdateAsync(section, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<bool> UpdateItemAsync(
        MenuItem item,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        return await dataAccess.UpdateAsync(item, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<bool> UpdateMenuAsync(
        Menu menu,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(menu);
        return await dataAccess.UpdateAsync(menu, cancellationToken).ConfigureAwait(false) > 0;
    }

    public async Task<int> ReorderSectionsAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        DateTime updatedUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sectionIds);
        var sectionsJson = JsonSerializer.Serialize(
            sectionIds.Select((id, sortOrder) => new { id, sortOrder }),
            JsonSerializerOptions.Web);
        var result = (await dataAccess.ExecuteSqlQueryAsync<SectionOrderResult, object>(
            ReorderSectionsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionsJson = sectionsJson,
                UpdatedUtc = updatedUtc
            },
            cancellationToken).ConfigureAwait(false)).Single();
        return result.ChangedCount;
    }

    public async Task<int> ReorderItemsAsync(
        Guid venueId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime updatedUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var itemsJson = JsonSerializer.Serialize(
            itemIds.Select((id, sortOrder) => new { id, sortOrder }),
            JsonSerializerOptions.Web);
        var result = (await dataAccess.ExecuteSqlQueryAsync<SectionOrderResult, object>(
            ReorderItemsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuSectionId = RequireId(sectionId, nameof(sectionId)),
                ItemsJson = itemsJson,
                UpdatedUtc = updatedUtc
            },
            cancellationToken).ConfigureAwait(false)).Single();
        return result.ChangedCount;
    }

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

    public sealed class SectionOrderResult
    {
        public int ChangedCount { get; set; }
    }
}
