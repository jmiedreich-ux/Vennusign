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

    // Editing the same field twice replaces the row: the queue is the current
    // diff from what the screens are showing, never a log of keystrokes.
    // Editing the same field twice replaces the row: the queue is the current diff
    // from what the screens are showing, never a log of keystrokes. Taking a value
    // back to what is already published removes the row entirely -- a change that
    // no longer differs is not a change (Q182). The menu is proved to belong to the
    // venue first, so a foreign menu id writes nothing.
    private const string UpsertDraftChangeSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51001, 'The menu does not belong to this venue.', 1;
        END;

        IF (@AfterValue IS NULL AND @BeforeValue IS NULL)
            OR (@AfterValue IS NOT NULL AND @BeforeValue IS NOT NULL AND @AfterValue = @BeforeValue)
        BEGIN
            DELETE FROM dbo.MenuDraftChanges
            WHERE MenuId = @MenuId
              AND TargetKind = @TargetKind
              AND Field = @Field
              AND ((TargetId IS NULL AND @TargetId IS NULL) OR TargetId = @TargetId);
        END
        ELSE
        BEGIN
            MERGE dbo.MenuDraftChanges WITH (HOLDLOCK) AS target
            USING (SELECT @MenuId AS MenuId, @TargetKind AS TargetKind, @TargetId AS TargetId, @Field AS Field) AS source
                ON target.MenuId = source.MenuId
               AND target.TargetKind = source.TargetKind
               AND target.Field = source.Field
               AND ((target.TargetId IS NULL AND source.TargetId IS NULL) OR target.TargetId = source.TargetId)
            WHEN MATCHED THEN
                UPDATE SET AfterValue = @AfterValue, Author = @Author, UpdatedUtc = @UpdatedUtc
            WHEN NOT MATCHED THEN
                INSERT (Id, VenueId, MenuId, TargetKind, TargetId, Field, BeforeValue, AfterValue, Author, CreatedUtc, UpdatedUtc)
                VALUES (@Id, @VenueId, @MenuId, @TargetKind, @TargetId, @Field, @BeforeValue, @AfterValue, @Author, @CreatedUtc, @UpdatedUtc);
        END;

        COMMIT TRANSACTION;

        -- Only the row this call touched. A collapsed change returns nothing, which
        -- is how the caller learns the edit no longer differs from the screens.
        SELECT Id, VenueId, MenuId, TargetKind, TargetId, Field, BeforeValue, AfterValue, Author, CreatedUtc, UpdatedUtc
        FROM dbo.MenuDraftChanges
        WHERE MenuId = @MenuId
          AND TargetKind = @TargetKind
          AND Field = @Field
          AND ((TargetId IS NULL AND @TargetId IS NULL) OR TargetId = @TargetId);
        """;

    private const string DraftChangesSql = """
        SELECT Id, VenueId, MenuId, TargetKind, TargetId, Field, BeforeValue, AfterValue, Author, CreatedUtc, UpdatedUtc
        FROM dbo.MenuDraftChanges
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY CreatedUtc, Id;
        """;

    // Discarding is the one irreversible act in the model, so the clearing and the
    // history entry that names who did it commit together. If the record cannot be
    // written, the draft is not thrown away.
    private const string ClearDraftSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Removed INT;

        DELETE FROM dbo.MenuDraftChanges
        WHERE VenueId = @VenueId AND MenuId = @MenuId;

        SET @Removed = @@ROWCOUNT;

        IF @Removed > 0 AND @RecordHistory = 1
        BEGIN
            INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
            VALUES (NEWID(), @VenueId, @MenuId, N'draft_discarded', NULL, NULL,
                    CONCAT(N'Discarded ', @Removed, N' queued change(s).'), @Author, @OccurredUtc);
        END;

        COMMIT TRANSACTION;

        SELECT @Removed AS Value;
        """;

    private const string NextVersionSql = """
        SELECT ISNULL(MAX(Version), 0) + 1 AS Value
        FROM dbo.MenuPublishEvents
        WHERE MenuId = @MenuId;
        """;

    // A publish is one deliberate act. Everything it implies -- the event, the
    // per-screen delivery rows, the history entry, the emptied queue and the
    // menu's new published version -- lands in a single transaction, so a
    // failure leaves the screens and the draft exactly as they were.
    // A publish is one deliberate act. Everything it implies -- the event, the
    // content snapshot, the shipped change set, the per-screen delivery rows, the
    // history entry, the emptied queue and the menu's new published version --
    // lands in a single transaction, so a failure leaves the screens and the draft
    // exactly as they were.
    //
    // The draft rows are captured by the same DELETE that removes them, so the
    // count and the shipped set describe exactly what went, even if another
    // request queues a change while this one runs. Counting first and deleting
    // later would let a late arrival be destroyed without ever being shipped.
    //
    // The menu is re-checked against the caller's venue inside the transaction:
    // a foreign menu id ships nothing and rolls the whole thing back.
    private const string PublishSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51001, 'The menu does not belong to this venue.', 1;
        END;

        DECLARE @ResolvedVersion BIGINT =
        (
            SELECT ISNULL(MAX(Version), 0) + 1
            FROM dbo.MenuPublishEvents WITH (UPDLOCK, HOLDLOCK)
            WHERE MenuId = @MenuId
        );

        -- Take the queue and remove it in one statement so nothing can slip in
        -- between counting and clearing.
        DECLARE @Shipped TABLE
        (
            TargetKind NVARCHAR(20) NOT NULL,
            TargetId UNIQUEIDENTIFIER NULL,
            Field NVARCHAR(100) NOT NULL,
            BeforeValue NVARCHAR(MAX) NULL,
            AfterValue NVARCHAR(MAX) NULL,
            Author NVARCHAR(200) NULL
        );

        DELETE FROM dbo.MenuDraftChanges
        OUTPUT deleted.TargetKind, deleted.TargetId, deleted.Field,
               deleted.BeforeValue, deleted.AfterValue, deleted.Author
        INTO @Shipped
        WHERE VenueId = @VenueId AND MenuId = @MenuId;

        DECLARE @Count INT = (SELECT COUNT(*) FROM @Shipped);

        DECLARE @ShippedJson NVARCHAR(MAX) =
        (
            SELECT TargetKind AS targetKind, TargetId AS targetId, Field AS field,
                   BeforeValue AS beforeValue, AfterValue AS afterValue, Author AS author
            FROM @Shipped
            FOR JSON PATH
        );

        -- The snapshot is the published content itself, so this version can be
        -- rendered and restored later without the queue that produced it.
        -- Item identity is permanent: the snapshot records values, never new ids.
        DECLARE @SnapshotJson NVARCHAR(MAX) =
        (
            SELECT
                m.Id AS menuId, m.Name AS name, m.Theme AS theme,
                m.DwellSeconds AS dwellSeconds, m.LoopWarningSeconds AS loopWarningSeconds,
                (
                    SELECT s.Id AS sectionId, s.Name AS name, s.SortOrder AS sortOrder,
                    (
                        SELECT p.ItemId AS itemId, i.Name AS name, i.Description AS description,
                               i.Price AS price, p.SortOrder AS sortOrder
                        FROM dbo.Placements p
                        INNER JOIN dbo.Items i ON i.Id = p.ItemId AND i.VenueId = p.VenueId
                        WHERE p.MenuSectionId = s.Id AND p.VenueId = @VenueId
                        ORDER BY p.SortOrder, p.Id
                        FOR JSON PATH
                    ) AS items
                    FROM dbo.MenuSections s
                    WHERE s.MenuId = m.Id AND s.VenueId = @VenueId AND s.IsActive = 1
                    ORDER BY s.SortOrder, s.Id
                    FOR JSON PATH
                ) AS sections
            FROM dbo.Menus m
            WHERE m.Id = @MenuId AND m.VenueId = @VenueId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        );

        INSERT dbo.MenuPublishEvents (Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot, ShippedChanges)
        VALUES (@Id, @VenueId, @MenuId, @ResolvedVersion, @Count, @Author, @PublishedUtc, @SnapshotJson, @ShippedJson);

        INSERT dbo.MenuPublishTargets (Id, PublishEventId, ScreenId, State, UpdatedUtc)
        SELECT NEWID(), @Id, a.ScreenId,
               CASE WHEN sc.Status = N'Online' THEN N'Pending' ELSE N'Offline' END,
               @PublishedUtc
        FROM dbo.MenuScreenAssignments a
        INNER JOIN dbo.Screens sc ON sc.Id = a.ScreenId
        WHERE a.VenueId = @VenueId AND a.MenuId = @MenuId;

        INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
        VALUES (NEWID(), @VenueId, @MenuId, N'published', @Id, NULL, @Detail, @Author, @PublishedUtc);

        -- Take-off is permanent, so it waited in the queue rather than committing
        -- when the operator confirmed it (Q68). This is the deliberate act that
        -- ships it. The targets above were written first on purpose: the screens
        -- that were showing this menu are told about the publish, and only then
        -- released to the venue fallback.
        IF EXISTS (
            SELECT 1 FROM @Shipped
            WHERE TargetKind = N'screens'
              AND Field = N'assignedScreens'
              AND (AfterValue IS NULL OR AfterValue = N''))
        BEGIN
            DECLARE @Released INT;

            DELETE FROM dbo.MenuScreenAssignments
            WHERE VenueId = @VenueId AND MenuId = @MenuId;

            SET @Released = @@ROWCOUNT;

            INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
            VALUES (NEWID(), @VenueId, @MenuId, N'taken_off_screens', @Id, NULL,
                    CONCAT(N'Released ', @Released, N' screen(s); the venue fallback shows instead.'),
                    @Author, @PublishedUtc);
        END;

        -- Supersession is never an action; it survives only as a fact on the
        -- entry that came before.
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

    private const string CountMenusSql = """
        SELECT COUNT_BIG(*) AS Value
        FROM dbo.Menus
        WHERE VenueId = @VenueId;
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
        return await dataAccess.UpdateAsync(item, cancellationToken).ConfigureAwait(false) > 0;
    }

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

    // ----- Draft queue ----------------------------------------------------------------

    public async Task<MenuDraftChange?> UpsertDraftChangeAsync(MenuDraftChange change, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);
        if (!DraftTargetKinds.IsSupported(change.TargetKind))
        {
            throw new ArgumentException($"Unsupported draft target kind '{change.TargetKind}'.", nameof(change));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(change.Field);
        var now = DateTime.UtcNow;
        return (await dataAccess.ExecuteSqlQueryAsync<MenuDraftChange, object>(
            UpsertDraftChangeSql,
            new
            {
                Id = change.Id == Guid.Empty ? Guid.NewGuid() : change.Id,
                VenueId = RequireId(change.VenueId, nameof(change.VenueId)),
                MenuId = RequireId(change.MenuId, nameof(change.MenuId)),
                change.TargetKind,
                change.TargetId,
                change.Field,
                change.BeforeValue,
                change.AfterValue,
                change.Author,
                CreatedUtc = change.CreatedUtc == default ? now : change.CreatedUtc,
                UpdatedUtc = now
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
    }

    public async Task<IReadOnlyCollection<MenuDraftChange>> GetDraftChangesAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuDraftChange, object>(
            DraftChangesSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<int> ClearDraftAsync(
        Guid venueId,
        Guid menuId,
        string? author = null,
        bool recordHistory = false,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            ClearDraftSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                Author = author,
                RecordHistory = recordHistory,
                OccurredUtc = DateTime.UtcNow
            },
            cancellationToken).ConfigureAwait(false)).Single().Value;

    // ----- Publish and history ---------------------------------------------------------

    public async Task<long> GetNextPublishVersionAsync(Guid menuId, CancellationToken cancellationToken = default)
    {
        var result = (await dataAccess.ExecuteSqlQueryAsync<CountResult, object>(
            NextVersionSql,
            new { MenuId = RequireId(menuId, nameof(menuId)) },
            cancellationToken).ConfigureAwait(false)).Single();
        return result.Value;
    }

    public async Task<MenuPublishEvent> PublishAsync(
        MenuPublishEvent publishEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publishEvent);

        // Targets come from the assignments inside the transaction, not from the
        // caller: a caller-supplied screen list could name another venue's screens.
        return (await dataAccess.ExecuteSqlQueryAsync<MenuPublishEvent, object>(
            PublishSql,
            new
            {
                Id = publishEvent.Id == Guid.Empty ? Guid.NewGuid() : publishEvent.Id,
                VenueId = RequireId(publishEvent.VenueId, nameof(publishEvent.VenueId)),
                MenuId = RequireId(publishEvent.MenuId, nameof(publishEvent.MenuId)),
                publishEvent.Author,
                PublishedUtc = publishEvent.PublishedUtc == default ? DateTime.UtcNow : publishEvent.PublishedUtc,
                Detail = (string?)null
            },
            cancellationToken).ConfigureAwait(false)).Single();
    }

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

    private sealed class CeilingRow
    {
        public string CapabilityId { get; set; } = string.Empty;

        public int LimitValue { get; set; }
    }
}
