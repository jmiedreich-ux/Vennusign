using System.Text.Json;
using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class MenuRepository(ISqlDataAccess dataAccess) : IMenuRepository
{
    private const string MenusSql = """
        SELECT Id, VenueId, Name, IsActive, DailySpecial,
               DwellSeconds, LoopWarningSeconds, Theme, IsPutAway, PublishedVersion,
               CreatedUtc, UpdatedUtc
        FROM dbo.Menus
        WHERE VenueId = @VenueId
        ORDER BY Name, Id;
        """;

    private const string SectionsSql = """
        SELECT Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc
        FROM dbo.MenuSections
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY SortOrder, Id;
        """;

    /// <summary>
    /// What a screen should actually show.
    ///
    /// Menu content lives in dbo.Items joined to a board through dbo.Placements.
    /// The older dbo.MenuItems table is not written by the builder and is empty,
    /// so reading it returned nothing for every menu built in the product - which
    /// is why published menus never reached a screen.
    ///
    /// Availability is the 86 board: an item with no ItemAvailability row has
    /// never been 86'd and is available, so the absence of a row means yes.
    /// </summary>
    private const string BoardItemsSql = """
        SELECT p.ItemId AS Id,
               i.VenueId,
               p.MenuSectionId,
               i.Name,
               i.Description,
               /*
                * Items.Price is NVARCHAR: the content model keeps a price exactly as it was
                * typed, so "12", "9.5" and "MP" all round-trip unchanged (Q115/Q190). The
                * display contract carries a decimal, so this is where the two models meet.
                *
                * TWO things were wrong here, and both put a false number on a guest's screen.
                *
                * 1. The symbol. Every price a paste import produces carries the one the menu
                *    was printed with - "$7.00" - and TRY_CONVERT rejects it, so ISNULL made it
                *    zero. Not the rare "market price, a dash" the old note anticipated: EVERY
                *    imported price on EVERY board rendered as $0.00. Stripping the currency
                *    symbol and separators first is what the operator meant by typing it.
                *
                * 2. The placement was ignored. A19 says a price belongs to the placement and
                *    the menu it is printed on, and the import writes that to
                *    Placements.ImportedPriceOverride - which this read straight past, so the
                *    per-menu price never reached a screen. The publish snapshot already
                *    COALESCEs the two; the live board now agrees with it.
                *
                * A genuinely non-numeric price (MP, Market) still lands as 0 and still needs
                * its own answer - showing $0.00 for market price is a different lie, tracked
                * separately. This fixes the case that is simply arithmetic.
                */
               ISNULL(TRY_CONVERT(DECIMAL(10, 2),
                   REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                       COALESCE(p.ImportedPriceOverride, i.Price),
                       N'$', N''), N'£', N''), N'€', N''), N',', N''), NCHAR(160), N'')), 0) AS Price,
               CAST(NULL AS DECIMAL(10, 2)) AS HappyHourPrice,
               ISNULL(a.IsAvailable, CAST(1 AS BIT)) AS IsAvailable,
               CAST(NULL AS INT) AS QuantityAvailable,
               CAST(NULL AS NVARCHAR(400)) AS Tags,
               i.ImageUrl,
               CAST(0 AS BIT) AS IsPopular,
               i.IsActive,
               p.SortOrder,
               i.CreatedUtc,
               i.UpdatedUtc
        FROM dbo.Placements p
        INNER JOIN dbo.Items i ON i.Id = p.ItemId AND i.VenueId = p.VenueId
        LEFT JOIN dbo.ItemAvailability a ON a.ItemId = p.ItemId AND a.VenueId = p.VenueId
        WHERE p.VenueId = @VenueId AND p.MenuSectionId = @MenuSectionId
        ORDER BY p.SortOrder, p.Id;
        """;

    /// <summary>Sections on one page, for a screen that has been assigned that page.</summary>
    private const string SectionsForPageSql = """
        SELECT Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc
        FROM dbo.MenuSections
        WHERE VenueId = @VenueId AND PageId = @PageId
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

    public async Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(menu);
        menu.Id = menu.Id == Guid.Empty ? Guid.NewGuid() : menu.Id;
        var now = menu.CreatedUtc == default ? DateTime.UtcNow : menu.CreatedUtc;
        const string sql = """
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;
            INSERT dbo.Menus (Id, VenueId, Name, IsActive, DwellSeconds, LoopWarningSeconds, Theme, IsPutAway, PublishedVersion, CreatedUtc, UpdatedUtc)
            VALUES (@Id, @VenueId, @Name, @IsActive, @DwellSeconds, @LoopWarningSeconds, @Theme, @IsPutAway, @PublishedVersion, @Now, @Now);
            INSERT dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            VALUES (@PageId, @VenueId, @Id, N'Page 1', 0, @Now, @Now);
            COMMIT TRANSACTION;
            SELECT @Id AS Id;
            """;
        _ = await dataAccess.ExecuteSqlQueryAsync<IdRow, object>(sql, new
        {
            menu.Id, menu.VenueId, menu.Name, menu.IsActive, menu.DwellSeconds, menu.LoopWarningSeconds,
            menu.Theme, menu.IsPutAway, menu.PublishedVersion, Now = now, PageId = Guid.NewGuid()
        }, cancellationToken).ConfigureAwait(false);
        return menu.Id;
    }

    public async Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(section);
        if (section.PageId == Guid.Empty)
        {
            var page = (await dataAccess.ExecuteSqlQueryAsync<IdRow, object>(
                "SELECT TOP (1) Id FROM dbo.MenuPages WHERE VenueId=@VenueId AND MenuId=@MenuId ORDER BY SortOrder, Id;",
                new { section.VenueId, section.MenuId }, cancellationToken).ConfigureAwait(false)).Single();
            section.PageId = page.Id;
        }
        return await InsertAsync(section, cancellationToken).ConfigureAwait(false);
    }

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

    public async Task<IReadOnlyCollection<MenuItem>> GetBoardItemsAsync(
        Guid venueId,
        Guid sectionId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuItem, object>(
            BoardItemsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuSectionId = RequireId(sectionId, nameof(sectionId))
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<MenuSection>> GetSectionsForPageAsync(
        Guid venueId,
        Guid pageId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuSection, object>(
            SectionsForPageSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                PageId = RequireId(pageId, nameof(pageId))
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

    private sealed class IdRow { public Guid Id { get; set; } }
}
