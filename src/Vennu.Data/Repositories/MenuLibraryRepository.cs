using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class MenuLibraryRepository(ISqlDataAccess dataAccess) : IMenuLibraryRepository
{
    private const string ItemsSql = """
        SELECT Id, VenueId, Name, Description, Price, ImageUrl, Source, IsActive, CreatedUtc, UpdatedUtc
        FROM dbo.Items
        WHERE VenueId = @VenueId
        ORDER BY Name, Id;
        """;

    private const string ItemSql = """
        SELECT Id, VenueId, Name, Description, Price, ImageUrl, Source, IsActive, CreatedUtc, UpdatedUtc
        FROM dbo.Items
        WHERE VenueId = @VenueId AND Id = @ItemId;
        """;

    private const string CountItemsOnMenuSql = """
        SELECT COUNT_BIG(DISTINCT p.ItemId) AS Value
        FROM dbo.Placements p
        WHERE p.VenueId = @VenueId AND p.MenuId = @MenuId;
        """;

    private const string PlacementsSql = """
        SELECT Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc
        FROM dbo.Placements
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY MenuSectionId, SortOrder, Id;
        """;

    private const string PlacementsForItemSql = """
        SELECT Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc
        FROM dbo.Placements
        WHERE VenueId = @VenueId AND ItemId = @ItemId
        ORDER BY MenuId, SortOrder, Id;
        """;

    private const string RemovePlacementSql = """
        DELETE FROM dbo.Placements
        OUTPUT 1 AS Value
        WHERE VenueId = @VenueId AND Id = @PlacementId;
        """;

    // Availability is a fact about the venue: last write wins, and it stays
    // written until a person changes it again.
    private const string SetAvailabilitySql = """
        MERGE dbo.ItemAvailability WITH (HOLDLOCK) AS target
        USING (SELECT @VenueId AS VenueId, @ItemId AS ItemId) AS source
            ON target.VenueId = source.VenueId AND target.ItemId = source.ItemId
        WHEN MATCHED THEN
            UPDATE SET IsAvailable = @IsAvailable, ChangedUtc = @ChangedUtc, ChangedBy = @ChangedBy
        WHEN NOT MATCHED THEN
            INSERT (VenueId, ItemId, IsAvailable, ChangedUtc, ChangedBy)
            VALUES (@VenueId, @ItemId, @IsAvailable, @ChangedUtc, @ChangedBy)
        OUTPUT inserted.VenueId, inserted.ItemId, inserted.IsAvailable, inserted.ChangedUtc, inserted.ChangedBy;
        """;

    private const string AvailabilitySql = """
        SELECT VenueId, ItemId, IsAvailable, ChangedUtc, ChangedBy
        FROM dbo.ItemAvailability
        WHERE VenueId = @VenueId
        ORDER BY ItemId;
        """;

    // A screen shows exactly one menu, so assigning replaces whatever it showed.
    // The screen id arrives from the route, so the venue owns neither side of this
    // by default. Both the screen and the menu must belong to the calling venue or
    // the source set is empty and nothing is written -- otherwise one venue could
    // hand another venue's screen to its own menu.
    private const string AssignScreenSql = """
        MERGE dbo.MenuScreenAssignments WITH (HOLDLOCK) AS target
        USING (
            SELECT s.Id AS ScreenId
            FROM dbo.Screens s
            WHERE s.Id = @ScreenId
              AND s.VenueId = @VenueId
              AND EXISTS (SELECT 1 FROM dbo.Menus m WHERE m.Id = @MenuId AND m.VenueId = @VenueId)
        ) AS source
            ON target.ScreenId = source.ScreenId
        WHEN MATCHED THEN
            UPDATE SET MenuId = @MenuId, VenueId = @VenueId, AssignedUtc = @AssignedUtc, AssignedBy = @AssignedBy
        WHEN NOT MATCHED THEN
            INSERT (Id, VenueId, ScreenId, MenuId, AssignedUtc, AssignedBy)
            VALUES (@Id, @VenueId, @ScreenId, @MenuId, @AssignedUtc, @AssignedBy)
        OUTPUT inserted.Id, inserted.VenueId, inserted.ScreenId, inserted.MenuId, inserted.AssignedUtc, inserted.AssignedBy;
        """;

    private const string ClearScreenAssignmentSql = """
        DELETE FROM dbo.MenuScreenAssignments
        OUTPUT 1 AS Value
        WHERE VenueId = @VenueId AND ScreenId = @ScreenId;
        """;

    private const string ClearMenuAssignmentsSql = """
        DELETE FROM dbo.MenuScreenAssignments
        OUTPUT 1 AS Value
        WHERE VenueId = @VenueId AND MenuId = @MenuId;
        """;

    private const string AssignmentsSql = """
        SELECT Id, VenueId, ScreenId, MenuId, AssignedUtc, AssignedBy
        FROM dbo.MenuScreenAssignments
        WHERE VenueId = @VenueId
        ORDER BY ScreenId;
        """;

    // The item update names the venue as well as the id: a caller carrying venue
    // A's context can never move or rewrite venue B's item by guessing its key.
    private const string UpdateItemSql = """
        UPDATE dbo.Items
        SET Name = @Name,
            Description = @Description,
            Price = @Price,
            ImageUrl = @ImageUrl,
            IsActive = @IsActive,
            UpdatedUtc = @UpdatedUtc
        OUTPUT 1 AS Value
        WHERE Id = @ItemId AND VenueId = @VenueId;
        """;

    // The count and the insert hold the same lock, so two requests arriving at
    // limit-minus-one cannot both succeed (Q201). Put-away menus are excluded:
    // the refusal tells the operator to put one away, so putting one away must
    // actually create room.
    private const string CreateMenuWithinCeilingSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Active INT =
        (
            SELECT COUNT(*) FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK)
            WHERE VenueId = @VenueId AND IsPutAway = 0
        );

        IF @Active + 1 > @Limit
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT CAST(0 AS BIT) AS Created, @Active AS ActiveMenuCount;
        END
        ELSE
        BEGIN
            INSERT dbo.Menus (Id, VenueId, Name, IsActive, CreatedUtc, UpdatedUtc)
            VALUES (@Id, @VenueId, @Name, 1, @Now, @Now);

            COMMIT TRANSACTION;
            SELECT CAST(1 AS BIT) AS Created, @Active + 1 AS ActiveMenuCount;
        END;
        """;

    // Creating an item and placing it are one act for the editor, so they commit
    // together, with the items-per-menu ceiling counted under the same lock as
    // the insert (Q201). The section is proved to sit on this menu in this venue.
    private const string CreateItemOnMenuSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.MenuSections
            WHERE Id = @SectionId AND MenuId = @MenuId AND VenueId = @VenueId AND IsActive = 1)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'section_missing' AS Outcome, 0 AS ItemCountOnMenu, 0 AS SortOrder;
        END
        ELSE
        BEGIN
            DECLARE @OnMenu INT =
            (
                SELECT COUNT(DISTINCT ItemId) FROM dbo.Placements WITH (UPDLOCK, HOLDLOCK)
                WHERE VenueId = @VenueId AND MenuId = @MenuId
            );

            IF @OnMenu + 1 > @Limit
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT N'over_ceiling' AS Outcome, @OnMenu AS ItemCountOnMenu, 0 AS SortOrder;
            END
            ELSE
            BEGIN
                DECLARE @SortOrder INT =
                    ISNULL((SELECT MAX(SortOrder) + 1 FROM dbo.Placements WHERE MenuSectionId = @SectionId), 0);

                INSERT dbo.Items (Id, VenueId, Name, Description, Price, ImageUrl, Source, IsActive, CreatedUtc, UpdatedUtc)
                VALUES (@ItemId, @VenueId, @Name, @Description, @Price, NULL, @Source, 1, @Now, @Now);

                INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
                VALUES (@PlacementId, @VenueId, @MenuId, @SectionId, @ItemId, @SortOrder, @Now, @Now);

                COMMIT TRANSACTION;
                SELECT N'created' AS Outcome, @OnMenu + 1 AS ItemCountOnMenu, @SortOrder AS SortOrder;
            END;
        END;
        """;

    // The new order arrives as a JSON array whose position is the sort order.
    private const string ReorderPlacementsSql = """
        UPDATE p
        SET p.SortOrder = CAST(ids.[key] AS INT), p.UpdatedUtc = @UpdatedUtc
        OUTPUT 1 AS Value
        FROM dbo.Placements p
        INNER JOIN OPENJSON(@ItemIdsJson) ids
            ON p.ItemId = TRY_CONVERT(UNIQUEIDENTIFIER, ids.[value])
        WHERE p.VenueId = @VenueId AND p.MenuId = @MenuId AND p.MenuSectionId = @SectionId;
        """;

    // What the editor renders: every placement in the venue with its item values
    // and its live availability, in board order.
    private const string PlacedItemsSql = """
        SELECT p.MenuId, p.MenuSectionId, p.ItemId, i.Name, i.Description, i.Price, p.SortOrder,
               CAST(ISNULL(a.IsAvailable, 1) AS BIT) AS IsAvailable, i.IsActive,
               p.CreatedUtc, p.UpdatedUtc
        FROM dbo.Placements p
        INNER JOIN dbo.Items i ON i.Id = p.ItemId AND i.VenueId = p.VenueId
        LEFT JOIN dbo.ItemAvailability a ON a.VenueId = p.VenueId AND a.ItemId = p.ItemId
        WHERE p.VenueId = @VenueId
        ORDER BY p.MenuSectionId, p.SortOrder, p.Id;
        """;

    private const string WorkingSnapshotBody = """
        SELECT
            m.Id AS menuId, m.Name AS name, m.Theme AS theme,
            m.DwellSeconds AS dwellSeconds, m.LoopWarningSeconds AS loopWarningSeconds,
            JSON_QUERY((
                SELECT CAST(a.ScreenId AS NVARCHAR(36)) AS screenId
                FROM dbo.MenuScreenAssignments a
                WHERE a.MenuId = m.Id AND a.VenueId = @VenueId
                ORDER BY a.ScreenId
                FOR JSON PATH
            )) AS screens,
            JSON_QUERY((
                SELECT s.Id AS sectionId, s.Name AS name, s.SortOrder AS sortOrder,
                    JSON_QUERY((
                        SELECT p.ItemId AS itemId, i.Name AS name, i.Description AS description,
                               i.Price AS price, p.SortOrder AS sortOrder
                        FROM dbo.Placements p
                        INNER JOIN dbo.Items i ON i.Id = p.ItemId AND i.VenueId = p.VenueId
                        WHERE p.MenuSectionId = s.Id AND p.VenueId = @VenueId
                        ORDER BY p.SortOrder, p.Id
                        FOR JSON PATH
                    )) AS items
                FROM dbo.MenuSections s
                WHERE s.MenuId = m.Id AND s.VenueId = @VenueId AND s.IsActive = 1
                ORDER BY s.SortOrder, s.Id
                FOR JSON PATH
            )) AS sections
        FROM dbo.Menus m
        WHERE m.Id = @MenuId AND m.VenueId = @VenueId
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        """;

    private const string WorkingSnapshotSql = """
        SELECT (
        """ + WorkingSnapshotBody + """
        ) AS Value;
        """;

    private const string LatestPublishedSnapshotSql = """
        SELECT TOP (1) Snapshot AS Value
        FROM dbo.MenuPublishEvents
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY Version DESC;
        """;

    // Restore and discard are the same operation against different snapshots: put
    // the working rows back to a recorded shape. It applies values onto the items
    // that already exist rather than re-minting them, so an 86 keeps its anchor
    // (Q43), and it commits the change together with the record of who did it.
    private const string RestoreSnapshotSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51001, 'The menu does not belong to this venue.', 1;
        END;

        DECLARE @Menu TABLE (Name NVARCHAR(200), Theme NVARCHAR(40), DwellSeconds INT, LoopWarningSeconds INT);
        INSERT @Menu (Name, Theme, DwellSeconds, LoopWarningSeconds)
        SELECT name, theme, dwellSeconds, loopWarningSeconds
        FROM OPENJSON(@SnapshotJson)
        WITH (
            name NVARCHAR(200) '$.name',
            theme NVARCHAR(40) '$.theme',
            dwellSeconds INT '$.dwellSeconds',
            loopWarningSeconds INT '$.loopWarningSeconds'
        );

        UPDATE m
        SET m.Name = ISNULL(t.Name, m.Name),
            m.Theme = ISNULL(t.Theme, m.Theme),
            m.DwellSeconds = ISNULL(NULLIF(t.DwellSeconds, 0), m.DwellSeconds),
            m.LoopWarningSeconds = ISNULL(NULLIF(t.LoopWarningSeconds, 0), m.LoopWarningSeconds),
            m.UpdatedUtc = @OccurredUtc
        FROM dbo.Menus m CROSS JOIN @Menu t
        WHERE m.Id = @MenuId AND m.VenueId = @VenueId;

        DECLARE @Sections TABLE (SectionId UNIQUEIDENTIFIER, Name NVARCHAR(200), SortOrder INT, Items NVARCHAR(MAX));
        INSERT @Sections (SectionId, Name, SortOrder, Items)
        SELECT sectionId, name, sortOrder, items
        FROM OPENJSON(@SnapshotJson, '$.sections')
        WITH (
            sectionId UNIQUEIDENTIFIER '$.sectionId',
            name NVARCHAR(200) '$.name',
            sortOrder INT '$.sortOrder',
            items NVARCHAR(MAX) '$.items' AS JSON
        );

        UPDATE s
        SET s.Name = src.Name,
            s.SortOrder = src.SortOrder,
            s.UpdatedUtc = @OccurredUtc
        FROM dbo.MenuSections s
        INNER JOIN @Sections src ON src.SectionId = s.Id
        WHERE s.VenueId = @VenueId AND s.MenuId = @MenuId;

        DECLARE @Items TABLE (SectionId UNIQUEIDENTIFIER, ItemId UNIQUEIDENTIFIER, Name NVARCHAR(200), Description NVARCHAR(1000), Price NVARCHAR(40), SortOrder INT);
        INSERT @Items (SectionId, ItemId, Name, Description, Price, SortOrder)
        SELECT sec.SectionId, i.itemId, i.name, i.description, i.price, i.sortOrder
        FROM @Sections sec
        CROSS APPLY OPENJSON(sec.Items)
        WITH (
            itemId UNIQUEIDENTIFIER '$.itemId',
            name NVARCHAR(200) '$.name',
            description NVARCHAR(1000) '$.description',
            price NVARCHAR(40) '$.price',
            sortOrder INT '$.sortOrder'
        ) i;

        -- Values go back onto the items that already exist. Identity is permanent.
        UPDATE it
        SET it.Name = src.Name,
            it.Description = src.Description,
            it.Price = src.Price,
            it.UpdatedUtc = @OccurredUtc
        FROM dbo.Items it
        INNER JOIN @Items src ON src.ItemId = it.Id
        WHERE it.VenueId = @VenueId;

        -- Placements return to the snapshot exactly: anything added since goes, and
        -- anything removed since comes back.
        DELETE p
        FROM dbo.Placements p
        WHERE p.VenueId = @VenueId AND p.MenuId = @MenuId
          AND NOT EXISTS (SELECT 1 FROM @Items src WHERE src.SectionId = p.MenuSectionId AND src.ItemId = p.ItemId);

        UPDATE p
        SET p.SortOrder = src.SortOrder, p.UpdatedUtc = @OccurredUtc
        FROM dbo.Placements p
        INNER JOIN @Items src ON src.SectionId = p.MenuSectionId AND src.ItemId = p.ItemId
        WHERE p.VenueId = @VenueId AND p.MenuId = @MenuId;

        INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
        SELECT NEWID(), @VenueId, @MenuId, src.SectionId, src.ItemId, src.SortOrder, @OccurredUtc, @OccurredUtc
        FROM @Items src
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Placements p
            WHERE p.MenuSectionId = src.SectionId AND p.ItemId = src.ItemId);

        -- Which screens the menu is on is part of the shape, so a restore puts that
        -- back too -- including undoing a take-off that has not shipped yet.
        DECLARE @Screens TABLE (ScreenId UNIQUEIDENTIFIER);
        INSERT @Screens (ScreenId)
        SELECT screenId FROM OPENJSON(@SnapshotJson, '$.screens')
        WITH (screenId UNIQUEIDENTIFIER '$.screenId');

        DELETE a
        FROM dbo.MenuScreenAssignments a
        WHERE a.VenueId = @VenueId AND a.MenuId = @MenuId
          AND NOT EXISTS (SELECT 1 FROM @Screens s WHERE s.ScreenId = a.ScreenId);

        INSERT dbo.MenuScreenAssignments (Id, VenueId, ScreenId, MenuId, AssignedUtc, AssignedBy)
        SELECT NEWID(), @VenueId, s.ScreenId, @MenuId, @OccurredUtc, @Author
        FROM @Screens s
        INNER JOIN dbo.Screens sc ON sc.Id = s.ScreenId AND sc.VenueId = @VenueId
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuScreenAssignments a WHERE a.ScreenId = s.ScreenId);

        INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
        VALUES (NEWID(), @VenueId, @MenuId, @Kind, NULL, NULL, @Detail, @Author, @OccurredUtc);

        COMMIT TRANSACTION;

        SELECT 1 AS Value;
        """;

    // A publish is one deliberate act, and under the derived save model it is the
    // only thing that changes what a screen shows. It applies no edits: the working
    // rows already are the menu. It records what the menu looks like right now and
    // sends that snapshot to the screens.
    //
    // Everything it implies -- the event, the snapshot, the per-screen delivery
    // rows, the history entry and the menu's new published version -- commits
    // together, so a failure leaves the screens exactly as they were.
    //
    // The menu is proved to belong to the caller's venue inside the transaction,
    // and the zero-screen refusal (Q80) is evaluated here rather than in the
    // service, so it cannot be lost to a race with a concurrent take-off.
    private const string PublishHeaderSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51001, 'The menu does not belong to this venue.', 1;
        END;

        DECLARE @WorkingScreens INT =
        (
            SELECT COUNT(*) FROM dbo.MenuScreenAssignments WITH (UPDLOCK, HOLDLOCK)
            WHERE VenueId = @VenueId AND MenuId = @MenuId
        );

        DECLARE @PreviousVersion BIGINT =
        (
            SELECT ISNULL(MAX(Version), 0)
            FROM dbo.MenuPublishEvents WITH (UPDLOCK, HOLDLOCK)
            WHERE MenuId = @MenuId
        );

        DECLARE @PreviouslyTargeted INT =
        (
            SELECT COUNT(*)
            FROM dbo.MenuPublishTargets t
            INNER JOIN dbo.MenuPublishEvents e ON e.Id = t.PublishEventId
            WHERE e.MenuId = @MenuId AND e.Version = @PreviousVersion
        );

        -- Q80: a publish that can reach nothing, and has nothing to release, is a
        -- named refusal rather than a silent version bump. Releasing screens stays
        -- publishable, because the screens being released have to be told.
        IF @WorkingScreens = 0 AND @PreviouslyTargeted = 0
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51002, 'Pair a screen to publish. This menu is not on a screen yet, so publishing it would reach nothing.', 1;
        END;

        DECLARE @ResolvedVersion BIGINT = @PreviousVersion + 1;

        DECLARE @SnapshotJson NVARCHAR(MAX) =
        (
        """;

    private const string PublishTailSql = """
        );

        INSERT dbo.MenuPublishEvents (Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot, ShippedChanges)
        VALUES (@Id, @VenueId, @MenuId, @ResolvedVersion, @ChangeCount, @Author, @PublishedUtc, @SnapshotJson, @ShippedChanges);

        -- The screens that were showing this menu are told about the publish even
        -- when they are being released, so a take-off reaches them instead of
        -- leaving them on stale content.
        INSERT dbo.MenuPublishTargets (Id, VenueId, PublishEventId, ScreenId, State, UpdatedUtc)
        SELECT NEWID(), @VenueId, @Id, screens.ScreenId,
               CASE WHEN sc.Status = N'Online' THEN N'Pending' ELSE N'Offline' END,
               @PublishedUtc
        FROM (
            SELECT a.ScreenId FROM dbo.MenuScreenAssignments a
            WHERE a.VenueId = @VenueId AND a.MenuId = @MenuId
            UNION
            SELECT t.ScreenId
            FROM dbo.MenuPublishTargets t
            INNER JOIN dbo.MenuPublishEvents e ON e.Id = t.PublishEventId
            WHERE e.MenuId = @MenuId AND e.Version = @PreviousVersion
        ) screens
        INNER JOIN dbo.Screens sc ON sc.Id = screens.ScreenId AND sc.VenueId = @VenueId;

        INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
        VALUES (NEWID(), @VenueId, @MenuId, N'published', @Id, NULL, @Detail, @Author, @PublishedUtc);

        -- Supersession is never an action; it survives only as a fact on the entry
        -- that came before.
        UPDATE h
        SET h.ReplacedByVersion = @ResolvedVersion
        FROM dbo.MenuHistoryEntries h
        INNER JOIN dbo.MenuPublishEvents e ON e.Id = h.PublishEventId
        WHERE h.MenuId = @MenuId
          AND h.Kind = N'published'
          AND e.Version < @ResolvedVersion
          AND h.ReplacedByVersion IS NULL;

        UPDATE dbo.Menus
        SET PublishedVersion = @ResolvedVersion,
            IsPutAway = 0,
            UpdatedUtc = @PublishedUtc
        WHERE Id = @MenuId AND VenueId = @VenueId;

        COMMIT TRANSACTION;

        SELECT Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot, ShippedChanges
        FROM dbo.MenuPublishEvents
        WHERE Id = @Id;
        """;

    private const string PublishSql = PublishHeaderSql + WorkingSnapshotBody + PublishTailSql;


    private const string PublishHistorySql = """
        SELECT TOP (@Limit) Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot
        FROM dbo.MenuPublishEvents
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY Version DESC;
        """;

    private const string PublishEventSql = """
        SELECT Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot
        FROM dbo.MenuPublishEvents
        WHERE VenueId = @VenueId AND MenuId = @MenuId AND Version = @Version;
        """;

    private const string PublishTargetsSql = """
        SELECT Id, PublishEventId, ScreenId, State, UpdatedUtc
        FROM dbo.MenuPublishTargets
        WHERE PublishEventId = @PublishEventId
        ORDER BY ScreenId;
        """;

    private const string HistorySql = """
        SELECT TOP (@Limit) Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc
        FROM dbo.MenuHistoryEntries
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY OccurredUtc DESC, Id DESC;
        """;

    // A venue-scoped allowance wins outright over an organization-wide one, matching
    // how capability access already resolves. Taking the minimum would silently
    // ignore a ceiling someone deliberately raised for one venue.
    private const string CeilingsSql = """
        SELECT ranked.CapabilityId, ranked.LimitValue
        FROM (
            SELECT a.CapabilityId, a.LimitValue,
                   ROW_NUMBER() OVER (
                       PARTITION BY a.CapabilityId
                       ORDER BY CASE WHEN a.VenueId = @VenueId THEN 0 ELSE 1 END, a.StartsUtc DESC) AS Precedence
            FROM dbo.CapabilityAllowances a
            INNER JOIN dbo.Venues v ON v.Id = @VenueId
            WHERE (a.VenueId = @VenueId OR (a.VenueId IS NULL AND a.OrganizationId = v.OrganizationId))
              AND a.StartsUtc <= SYSUTCDATETIME()
              AND (a.EndsUtc IS NULL OR a.EndsUtc > SYSUTCDATETIME())
        ) ranked
        WHERE ranked.Precedence = 1;
        """;

    // Put-away menus do not count against the ceiling: the refusal names putting
    // one away as the way to make room, so it has to actually make room.
    private const string CountMenusSql = """
        SELECT COUNT_BIG(*) AS Value
        FROM dbo.Menus
        WHERE VenueId = @VenueId AND IsPutAway = 0;
        """;

    // ----- Library and placements -------------------------------------------------

    public Task<Guid> CreateItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateItem(item);
        return InsertAsync(item, cancellationToken);
    }

    public async Task<bool> UpdateItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateItem(item);
        item.UpdatedUtc = DateTime.UtcNow;
        return (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            UpdateItemSql,
            new
            {
                ItemId = RequireId(item.Id, nameof(item.Id)),
                VenueId = RequireId(item.VenueId, nameof(item.VenueId)),
                item.Name,
                item.Description,
                item.Price,
                item.ImageUrl,
                item.IsActive,
                item.UpdatedUtc
            },
            cancellationToken).ConfigureAwait(false)).Any();
    }

    public async Task<MenuCreateOutcome> CreateMenuWithinCeilingAsync(
        Menu menu,
        int activeMenuLimit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(menu);
        ArgumentException.ThrowIfNullOrWhiteSpace(menu.Name);
        if (menu.Id == Guid.Empty)
        {
            menu.Id = Guid.NewGuid();
        }

        var row = (await dataAccess.ExecuteSqlQueryAsync<MenuCreateRow, object>(
            CreateMenuWithinCeilingSql,
            new
            {
                menu.Id,
                VenueId = RequireId(menu.VenueId, nameof(menu.VenueId)),
                menu.Name,
                Limit = activeMenuLimit,
                Now = menu.CreatedUtc == default ? DateTime.UtcNow : menu.CreatedUtc
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new MenuCreateOutcome(row.Created, row.ActiveMenuCount);
    }

    public async Task<ItemPlacementOutcome> CreateItemOnMenuAsync(
        Item item,
        Guid menuId,
        Guid sectionId,
        int itemsPerMenuLimit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateItem(item);
        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.NewGuid();
        }

        var row = (await dataAccess.ExecuteSqlQueryAsync<ItemPlacementRow, object>(
            CreateItemOnMenuSql,
            new
            {
                ItemId = item.Id,
                PlacementId = Guid.NewGuid(),
                VenueId = RequireId(item.VenueId, nameof(item.VenueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                item.Name,
                item.Description,
                item.Price,
                item.Source,
                Limit = itemsPerMenuLimit,
                Now = DateTime.UtcNow
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new ItemPlacementOutcome(row.Outcome, row.ItemCountOnMenu, row.SortOrder);
    }

    public async Task<int> ReorderPlacementsAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime updatedUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        return (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            ReorderPlacementsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                ItemIdsJson = System.Text.Json.JsonSerializer.Serialize(itemIds),
                UpdatedUtc = updatedUtc
            },
            cancellationToken).ConfigureAwait(false)).Count();
    }

    public async Task<IReadOnlyCollection<PlacedMenuItem>> GetPlacedItemsForVenueAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PlacedMenuItem, object>(
            PlacedItemsSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<Item?> GetItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<Item, object>(
            ItemSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                ItemId = RequireId(itemId, nameof(itemId))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<IReadOnlyCollection<Item>> GetItemsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<Item, object>(
            ItemsSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<int> CountItemsOnMenuAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default)
    {
        var result = (await dataAccess.ExecuteSqlQueryAsync<CountResult, object>(
            CountItemsOnMenuSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).Single();
        return (int)result.Value;
    }

    public Task<Guid> CreatePlacementAsync(Placement placement, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(placement);
        return InsertAsync(placement, cancellationToken);
    }

    public async Task<bool> RemovePlacementAsync(Guid venueId, Guid placementId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            RemovePlacementSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                PlacementId = RequireId(placementId, nameof(placementId))
            },
            cancellationToken).ConfigureAwait(false)).Any();

    public async Task<IReadOnlyCollection<Placement>> GetPlacementsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<Placement, object>(
            PlacementsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<Placement>> GetPlacementsForItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<Placement, object>(
            PlacementsForItemSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                ItemId = RequireId(itemId, nameof(itemId))
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    // ----- Availability -----------------------------------------------------------

    public async Task<ItemAvailability> SetAvailabilityAsync(ItemAvailability availability, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(availability);
        return (await dataAccess.ExecuteSqlQueryAsync<ItemAvailability, object>(
            SetAvailabilitySql,
            new
            {
                VenueId = RequireId(availability.VenueId, nameof(availability.VenueId)),
                ItemId = RequireId(availability.ItemId, nameof(availability.ItemId)),
                availability.IsAvailable,
                ChangedUtc = availability.ChangedUtc == default ? DateTime.UtcNow : availability.ChangedUtc,
                availability.ChangedBy
            },
            cancellationToken).ConfigureAwait(false)).Single();
    }

    public async Task<IReadOnlyCollection<ItemAvailability>> GetAvailabilityAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ItemAvailability, object>(
            AvailabilitySql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    // ----- Assignment ---------------------------------------------------------------

    public async Task<MenuScreenAssignment> AssignScreenAsync(MenuScreenAssignment assignment, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        return (await dataAccess.ExecuteSqlQueryAsync<MenuScreenAssignment, object>(
            AssignScreenSql,
            new
            {
                Id = assignment.Id == Guid.Empty ? Guid.NewGuid() : assignment.Id,
                VenueId = RequireId(assignment.VenueId, nameof(assignment.VenueId)),
                ScreenId = RequireId(assignment.ScreenId, nameof(assignment.ScreenId)),
                MenuId = RequireId(assignment.MenuId, nameof(assignment.MenuId)),
                AssignedUtc = assignment.AssignedUtc == default ? DateTime.UtcNow : assignment.AssignedUtc,
                assignment.AssignedBy
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault()
            ?? throw new InvalidOperationException(
                $"Screen '{assignment.ScreenId}' and menu '{assignment.MenuId}' must both belong to venue '{assignment.VenueId}'.");
    }

    public async Task<bool> ClearScreenAssignmentAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            ClearScreenAssignmentSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                ScreenId = RequireId(screenId, nameof(screenId))
            },
            cancellationToken).ConfigureAwait(false)).Any();

    public async Task<int> ClearMenuAssignmentsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            ClearMenuAssignmentsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).Count();

    public async Task<IReadOnlyCollection<MenuScreenAssignment>> GetAssignmentsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuScreenAssignment, object>(
            AssignmentsSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    // ----- Publish and history ---------------------------------------------------------

    public async Task<MenuPublishEvent> PublishAsync(
        MenuPublishEvent publishEvent,
        int changeCount,
        string? shippedChanges,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publishEvent);

        // Targets are resolved inside the statement from the assignments and the
        // previous publish, never from a caller-supplied list that could name
        // another venue's screens.
        return (await dataAccess.ExecuteSqlQueryAsync<MenuPublishEvent, object>(
            PublishSql,
            new
            {
                Id = publishEvent.Id == Guid.Empty ? Guid.NewGuid() : publishEvent.Id,
                VenueId = RequireId(publishEvent.VenueId, nameof(publishEvent.VenueId)),
                MenuId = RequireId(publishEvent.MenuId, nameof(publishEvent.MenuId)),
                publishEvent.Author,
                PublishedUtc = publishEvent.PublishedUtc == default ? DateTime.UtcNow : publishEvent.PublishedUtc,
                ChangeCount = changeCount,
                ShippedChanges = shippedChanges,
                Detail = (string?)null
            },
            cancellationToken).ConfigureAwait(false)).Single();
    }

    /// <summary>The menu as it stands right now, in the shape a publish records.</summary>
    public async Task<string?> GetWorkingSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<JsonRow, object>(
            WorkingSnapshotSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Value;

    public async Task<IReadOnlyCollection<MenuPublishEvent>> GetPublishHistoryAsync(
        Guid venueId,
        Guid menuId,
        int limit,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuPublishEvent, object>(
            PublishHistorySql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                Limit = RequireLimit(limit)
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<MenuPublishEvent?> GetPublishEventAsync(Guid venueId, Guid menuId, long version, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuPublishEvent, object>(
            PublishEventSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                Version = version
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<IReadOnlyCollection<MenuPublishTarget>> GetPublishTargetsAsync(Guid publishEventId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuPublishTarget, object>(
            PublishTargetsSql,
            new { PublishEventId = RequireId(publishEventId, nameof(publishEventId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<string?> GetLatestPublishedSnapshotAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<JsonRow, object>(
            LatestPublishedSnapshotSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault()?.Value;

    public async Task RestoreSnapshotAsync(
        Guid venueId,
        Guid menuId,
        string snapshotJson,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default,
        string kind = MenuHistoryKinds.Restored)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshotJson);
        if (!MenuHistoryKinds.IsSupported(kind))
        {
            throw new ArgumentException($"Unsupported history kind '{kind}'.", nameof(kind));
        }

        await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            RestoreSnapshotSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SnapshotJson = snapshotJson,
                Author = author,
                Detail = detail,
                OccurredUtc = occurredUtc,
                Kind = kind
            },
            cancellationToken).ConfigureAwait(false);
    }

    public Task<Guid> RecordHistoryAsync(MenuHistoryEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (!MenuHistoryKinds.IsSupported(entry.Kind))
        {
            throw new ArgumentException($"Unsupported history kind '{entry.Kind}'.", nameof(entry));
        }

        if (entry.OccurredUtc == default)
        {
            entry.OccurredUtc = DateTime.UtcNow;
        }

        return InsertAsync(entry, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MenuHistoryEntry>> GetHistoryAsync(
        Guid venueId,
        Guid menuId,
        int limit,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuHistoryEntry, object>(
            HistorySql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                Limit = RequireLimit(limit)
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    // ----- Ceilings ---------------------------------------------------------------------

    public async Task<IReadOnlyDictionary<string, int>> GetCeilingsAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var rows = await dataAccess.ExecuteSqlQueryAsync<CeilingRow, object>(
            CeilingsSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false);

        return rows.ToDictionary(row => row.CapabilityId, row => row.LimitValue, StringComparer.Ordinal);
    }

    public async Task<int> CountMenusAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var result = (await dataAccess.ExecuteSqlQueryAsync<CountResult, object>(
            CountMenusSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).Single();
        return (int)result.Value;
    }

    // ----- Helpers ---------------------------------------------------------------------

    private static void ValidateItem(Item item)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(item.Name);
        if (item.Name.Length > Item.NameMaxLength)
        {
            throw new ArgumentException($"Item name cannot exceed {Item.NameMaxLength} characters.", nameof(item));
        }

        if (item.Description is { Length: > Item.DescriptionMaxLength })
        {
            throw new ArgumentException($"Item description cannot exceed {Item.DescriptionMaxLength} characters.", nameof(item));
        }

        if (item.Price is { Length: > Item.PriceMaxLength })
        {
            throw new ArgumentException($"Item price cannot exceed {Item.PriceMaxLength} characters.", nameof(item));
        }

        if (!ItemSources.IsSupported(item.Source))
        {
            throw new ArgumentException($"Unsupported item source '{item.Source}'.", nameof(item));
        }
    }

    private async Task<Guid> InsertAsync<T>(T entity, CancellationToken cancellationToken)
        where T : class
    {
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

    private static void SetDefaultDate<T>(T entity, string propertyName, DateTime value)
    {
        var property = typeof(T).GetProperty(propertyName);
        if (property is null || property.PropertyType != typeof(DateTime))
        {
            return;
        }

        if (property.GetValue(entity) is DateTime current && current != default)
        {
            return;
        }

        property.SetValue(entity, value);
    }

    private static Guid RequireId(Guid value, string parameterName) =>
        value == Guid.Empty
            ? throw new ArgumentException("Identifier cannot be empty.", parameterName)
            : value;

    private static int RequireLimit(int limit) =>
        limit is < 1 or > 500
            ? throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be between 1 and 500.")
            : limit;

    private sealed class CountResult
    {
        public long Value { get; set; }
    }

    private sealed class ScalarResult
    {
        public int Value { get; set; }
    }

    private sealed class JsonRow
    {
        public string? Value { get; set; }
    }

    private sealed class CeilingRow
    {
        public string CapabilityId { get; set; } = string.Empty;

        public int LimitValue { get; set; }
    }

    private sealed class MenuCreateRow
    {
        public bool Created { get; set; }

        public int ActiveMenuCount { get; set; }
    }

    private sealed class ItemPlacementRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int ItemCountOnMenu { get; set; }

        public int SortOrder { get; set; }
    }
}
