using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class ContentRepository(ISqlDataAccess dataAccess) : IContentRepository
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
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        -- Giving a put-away menu a screen would put it back on the shelf without
        -- the ceiling check or the record that makes putting one back deliberate,
        -- so it is refused and the operator is told why.
        IF EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK)
                   WHERE Id = @MenuId AND VenueId = @VenueId AND IsPutAway = 1)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51006, 'This menu is put away. Put it back on the shelf before giving it a screen.', 1;
        END;

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

        COMMIT TRANSACTION;
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

    // Taking a menu off its screens is a deliberate act by a person, so the act and
    // the record naming them commit together (Q207). What it changes is the working
    // state; the screens keep showing the published snapshot until the next publish
    // carries it (Q68).
    private const string TakeOffScreensSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51001, 'The menu does not belong to this venue.', 1;
        END;

        DECLARE @Removed INT;

        DELETE FROM dbo.MenuScreenAssignments
        WHERE VenueId = @VenueId AND MenuId = @MenuId;

        SET @Removed = @@ROWCOUNT;

        IF @Removed > 0
        BEGIN
            INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
            VALUES (NEWID(), @VenueId, @MenuId, N'taken_off_screens', NULL, NULL,
                    CONCAT(N'Took the menu off ', @Removed, N' screen(s); it reaches them on the next publish.'),
                    @Author, @OccurredUtc);
        END;

        COMMIT TRANSACTION;

        SELECT @Removed AS Value;
        """;

    // Put away is the terminal state for a menu this build (Q-register: there is no
    // delete). Putting one back is bounded by the same ceiling as creating one, so
    // the count and the flag move under a single lock.
    private const string SetPutAwaySql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'not_found' AS Outcome, 0 AS ActiveMenuCount;
        END
        ELSE
        BEGIN
            DECLARE @Active INT =
            (
                SELECT COUNT(*) FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK)
                WHERE VenueId = @VenueId AND IsPutAway = 0
            );

            IF @IsPutAway = 0 AND EXISTS (SELECT 1 FROM dbo.Menus WHERE Id = @MenuId AND IsPutAway = 1) AND @Active + 1 > @Limit
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT N'over_ceiling' AS Outcome, @Active AS ActiveMenuCount;
            END
            ELSE IF NOT EXISTS (SELECT 1 FROM dbo.Menus WHERE Id = @MenuId AND IsPutAway = @IsPutAway)
            BEGIN
                -- A menu on a screen is never put away underneath the person: it is
                -- taken off first, which is its own deliberate act (Q195).
                --
                -- Being on a screen is not the presence of an assignment row. A
                -- take-off changes the working state; the screens keep showing the
                -- published snapshot until a publish carries it (Q68). Reading only
                -- the working rows lets a menu be put away with its take-off still
                -- pending -- and the publish that would release the screen is then
                -- refused, because the menu is put away, leaving the screen showing
                -- a menu the system reports as shelved. So the published snapshot
                -- is asked too: take off, publish, then put away.
                --
                -- A screen another menu has since been given is not this menu's to
                -- release -- publish leaves it alone by the owner's rule -- so it
                -- does not hold this menu on the shelf either. Counting it would
                -- strand the menu just as surely, with no publish able to clear it.
                IF @IsPutAway = 1 AND
                (
                    EXISTS (SELECT 1 FROM dbo.MenuScreenAssignments WITH (UPDLOCK, HOLDLOCK) WHERE VenueId = @VenueId AND MenuId = @MenuId)
                    OR EXISTS
                    (
                        SELECT 1
                        FROM dbo.MenuPublishEvents e WITH (UPDLOCK, HOLDLOCK)
                        CROSS APPLY OPENJSON(e.Snapshot, '$.screens')
                            WITH (screenId UNIQUEIDENTIFIER '$.screenId') published
                        WHERE e.VenueId = @VenueId AND e.MenuId = @MenuId
                          AND e.Version = (SELECT MAX(Version) FROM dbo.MenuPublishEvents WHERE VenueId = @VenueId AND MenuId = @MenuId)
                          AND published.screenId IS NOT NULL
                          AND NOT EXISTS (
                              SELECT 1 FROM dbo.MenuScreenAssignments taken WITH (UPDLOCK, HOLDLOCK)
                              WHERE taken.ScreenId = published.screenId AND taken.MenuId <> @MenuId)
                    )
                )
                BEGIN
                    ROLLBACK TRANSACTION;
                    SELECT N'still_on_screens' AS Outcome, @Active AS ActiveMenuCount;
                END
                ELSE
                BEGIN
                    UPDATE dbo.Menus
                    SET IsPutAway = @IsPutAway, UpdatedUtc = @OccurredUtc
                    WHERE Id = @MenuId AND VenueId = @VenueId;

                    INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
                    VALUES (NEWID(), @VenueId, @MenuId, @Kind, NULL, NULL, @Detail, @Author, @OccurredUtc);

                    COMMIT TRANSACTION;
                    SELECT N'changed' AS Outcome, CASE WHEN @IsPutAway = 1 THEN @Active - 1 ELSE @Active + 1 END AS ActiveMenuCount;
                END;
            END
            ELSE
            BEGIN
                -- Already in the asked-for state: nothing to do, and no second
                -- history entry claiming it happened twice.
                COMMIT TRANSACTION;
                SELECT N'unchanged' AS Outcome, @Active AS ActiveMenuCount;
            END;
        END;
        """;

    // What a screen is actually showing.
    //
    // The milestone's whole promise is that a screen shows the last published version
    // and nothing else changes it - and until this read existed there was no way to
    // ask. That absence is why the owner demo could pass twelve checks of twelve while
    // a screen sat stranded on a menu the system called shelved: every check asked the
    // API whether it had accepted a request, and none could ask the screen.
    //
    // Two sources, each used for what it actually means. The delivery rows say which
    // publish last spoke to this screen - that is what a target records, including the
    // publish that releases a screen. The snapshot of that publish says what it was
    // told: if it names the screen, the screen is showing that version; if it does not,
    // that publish was the take-off and the screen is showing nothing.
    //
    // Assignments are deliberately absent. An assignment is unpublished intent, and
    // reading a screen's content from it is the defect independent review #6 found.
    private const string ScreensShowingSql = """
        SELECT
            sc.Id AS ScreenId,
            sc.Name AS ScreenName,
            CASE WHEN named.ScreenId IS NULL THEN NULL ELSE last.MenuId END AS MenuId,
            CASE WHEN named.ScreenId IS NULL THEN NULL ELSE m.Name END AS MenuName,
            CASE WHEN named.ScreenId IS NULL THEN NULL ELSE last.Version END AS Version,
            CASE WHEN named.ScreenId IS NULL THEN NULL ELSE last.PublishedUtc END AS PublishedUtc,
            CASE WHEN named.ScreenId IS NULL THEN NULL ELSE last.Author END AS Author
        FROM dbo.Screens sc
        OUTER APPLY
        (
            SELECT TOP (1) e.Id, e.MenuId, e.Version, e.PublishedUtc, e.Author, e.Snapshot
            FROM dbo.MenuPublishTargets t
            INNER JOIN dbo.MenuPublishEvents e ON e.Id = t.PublishEventId AND e.VenueId = t.VenueId
            WHERE t.VenueId = @VenueId AND t.ScreenId = sc.Id
            ORDER BY e.PublishedUtc DESC, e.Id DESC
        ) last
        OUTER APPLY
        (
            SELECT TOP (1) s.screenId AS ScreenId
            FROM OPENJSON(last.Snapshot, '$.screens')
                WITH (screenId UNIQUEIDENTIFIER '$.screenId') s
            WHERE s.screenId = sc.Id
        ) named
        LEFT JOIN dbo.Menus m ON m.Id = last.MenuId AND m.VenueId = sc.VenueId
        WHERE sc.VenueId = @VenueId
        ORDER BY sc.Name;
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
            WHERE Id = @SectionId AND MenuId = @MenuId AND VenueId = @VenueId)
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

    // ---- the builder's writes -------------------------------------------------
    //
    // Every one of these is a single guarded statement rather than a read, a
    // decision in C#, and a write. That shape has produced this codebase's most
    // common defect four times now: two values that must describe the same instant,
    // read apart. Reorder is the sharpest case, because the builder makes it a
    // drag - fast, repeated, and easy to overlap with somebody else's add.

    /// <summary>
    /// Adds a section at the end of the menu. The next sort order is read under the
    /// same lock as the insert: (MenuId, SortOrder) is unique, so two people adding
    /// a section at once would otherwise collide on it.
    /// </summary>
    private const string CreateSectionOnMenuSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'menu_missing' AS Outcome, 0 AS SortOrder;
        END
        ELSE
        BEGIN
            DECLARE @Next INT = ISNULL((
                SELECT MAX(SortOrder) FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
                WHERE MenuId = @MenuId AND VenueId = @VenueId), -1) + 1;

            INSERT dbo.MenuSections (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            VALUES (@SectionId, @VenueId, @MenuId, @Name, @Next, @Now, @Now);

            COMMIT TRANSACTION;
            SELECT N'created' AS Outcome, @Next AS SortOrder;
        END
        """;

    private const string RenameSectionSql = """
        UPDATE dbo.MenuSections
        SET Name = @Name, UpdatedUtc = @Now
        OUTPUT 1 AS Value
        WHERE Id = @SectionId AND MenuId = @MenuId AND VenueId = @VenueId;
        """;

    /// <summary>
    /// Deletes a section and releases its items back to the library (Q96). The
    /// placements go; the items do not - they were never in the section, a
    /// placement put them there. The released count is returned so the UI can say
    /// what happened rather than guess at it.
    /// </summary>
    private const string DeleteSectionSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @SectionId AND MenuId = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'section_missing' AS Outcome, 0 AS Released;
        END
        ELSE
        BEGIN
            DECLARE @Released INT =
                (SELECT COUNT(*) FROM dbo.Placements WHERE MenuSectionId = @SectionId AND VenueId = @VenueId);

            DELETE FROM dbo.Placements WHERE MenuSectionId = @SectionId AND VenueId = @VenueId;
            DELETE FROM dbo.MenuSections WHERE Id = @SectionId AND VenueId = @VenueId;

            COMMIT TRANSACTION;
            SELECT N'deleted' AS Outcome, @Released AS Released;
        END
        """;

    /// <summary>
    /// The order the caller sends must still be exactly this menu's sections when
    /// the write happens, proved under the lock rather than by an earlier read. A
    /// section added between the two would otherwise be left out of the numbering
    /// and keep a stale sort order.
    ///
    /// (MenuId, SortOrder) is unique, so every section is parked out of the range
    /// first - otherwise swapping two collides half-way through.
    /// </summary>
    private const string ReorderSectionsGuardedSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Order TABLE (SectionId UNIQUEIDENTIFIER PRIMARY KEY, Position INT NOT NULL);
        INSERT @Order (SectionId, Position)
        SELECT TRY_CONVERT(UNIQUEIDENTIFIER, [value]), CAST([key] AS INT)
        FROM OPENJSON(@SectionIdsJson);

        DECLARE @Live INT = (
            SELECT COUNT(*) FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
            WHERE MenuId = @MenuId AND VenueId = @VenueId);

        IF @Live <> (SELECT COUNT(*) FROM @Order)
           OR EXISTS (
               SELECT 1 FROM @Order o
               WHERE NOT EXISTS (
                   SELECT 1 FROM dbo.MenuSections s
                   WHERE s.Id = o.SectionId AND s.MenuId = @MenuId AND s.VenueId = @VenueId))
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'order_stale' AS Outcome, @Live AS Moved;
        END
        ELSE
        BEGIN
            UPDATE s SET s.SortOrder = 1000000 + o.Position, s.UpdatedUtc = @Now
            FROM dbo.MenuSections s INNER JOIN @Order o ON o.SectionId = s.Id;

            UPDATE s SET s.SortOrder = o.Position, s.UpdatedUtc = @Now
            FROM dbo.MenuSections s INNER JOIN @Order o ON o.SectionId = s.Id;

            COMMIT TRANSACTION;
            SELECT N'reordered' AS Outcome, @Live AS Moved;
        END
        """;

    /// <summary>
    /// The same guard for placements. The old path trusted the caller's list, so
    /// any placement omitted from it kept a stale sort order that could collide
    /// with a rewritten one - leaving board order resting on a tiebreaker nobody
    /// chose.
    /// </summary>
    private const string ReorderPlacementsGuardedSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Order TABLE (ItemId UNIQUEIDENTIFIER PRIMARY KEY, Position INT NOT NULL);
        INSERT @Order (ItemId, Position)
        SELECT TRY_CONVERT(UNIQUEIDENTIFIER, [value]), CAST([key] AS INT)
        FROM OPENJSON(@ItemIdsJson);

        DECLARE @Live INT = (
            SELECT COUNT(*) FROM dbo.Placements WITH (UPDLOCK, HOLDLOCK)
            WHERE MenuId = @MenuId AND MenuSectionId = @SectionId AND VenueId = @VenueId);

        IF @Live <> (SELECT COUNT(*) FROM @Order)
           OR EXISTS (
               SELECT 1 FROM @Order o
               WHERE NOT EXISTS (
                   SELECT 1 FROM dbo.Placements p
                   WHERE p.ItemId = o.ItemId AND p.MenuSectionId = @SectionId
                     AND p.MenuId = @MenuId AND p.VenueId = @VenueId))
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'order_stale' AS Outcome, @Live AS Moved;
        END
        ELSE
        BEGIN
            UPDATE p SET p.SortOrder = o.Position, p.UpdatedUtc = @Now
            FROM dbo.Placements p
            INNER JOIN @Order o ON o.ItemId = p.ItemId
            WHERE p.MenuSectionId = @SectionId AND p.MenuId = @MenuId AND p.VenueId = @VenueId;

            COMMIT TRANSACTION;
            SELECT N'reordered' AS Outcome, @Live AS Moved;
        END
        """;

    /// <summary>
    /// Places an item the library already holds. An item already on this board is
    /// neither an error nor a second copy: the caller is told which section it
    /// already sits in, so the UI can jump to it (Q112). The items-per-menu ceiling
    /// is counted under the same lock as the insert (Q201).
    /// </summary>
    private const string PlaceExistingItemSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @SectionId AND MenuId = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'section_missing' AS Outcome, 0 AS ItemCountOnMenu, 0 AS SortOrder,
                   CAST(NULL AS UNIQUEIDENTIFIER) AS ExistingSectionId;
        END
        ELSE IF NOT EXISTS (SELECT 1 FROM dbo.Items WHERE Id = @ItemId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'item_missing' AS Outcome, 0 AS ItemCountOnMenu, 0 AS SortOrder,
                   CAST(NULL AS UNIQUEIDENTIFIER) AS ExistingSectionId;
        END
        ELSE
        BEGIN
            DECLARE @Existing UNIQUEIDENTIFIER = (
                SELECT TOP (1) MenuSectionId FROM dbo.Placements WITH (UPDLOCK, HOLDLOCK)
                WHERE MenuId = @MenuId AND ItemId = @ItemId AND VenueId = @VenueId);

            DECLARE @OnMenu INT = (
                SELECT COUNT(*) FROM dbo.Placements WITH (UPDLOCK, HOLDLOCK)
                WHERE MenuId = @MenuId AND VenueId = @VenueId);

            IF @Existing IS NOT NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT N'already_on_board' AS Outcome, @OnMenu AS ItemCountOnMenu, 0 AS SortOrder,
                       @Existing AS ExistingSectionId;
            END
            ELSE IF @OnMenu >= @ItemsPerMenuLimit
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT N'ceiling_reached' AS Outcome, @OnMenu AS ItemCountOnMenu, 0 AS SortOrder,
                       CAST(NULL AS UNIQUEIDENTIFIER) AS ExistingSectionId;
            END
            ELSE
            BEGIN
                DECLARE @Next INT = ISNULL((
                    SELECT MAX(SortOrder) FROM dbo.Placements
                    WHERE MenuSectionId = @SectionId AND VenueId = @VenueId), -1) + 1;

                INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
                VALUES (@PlacementId, @VenueId, @MenuId, @SectionId, @ItemId, @Next, @Now, @Now);

                COMMIT TRANSACTION;
                SELECT N'placed' AS Outcome, @OnMenu + 1 AS ItemCountOnMenu, @Next AS SortOrder,
                       CAST(NULL AS UNIQUEIDENTIFIER) AS ExistingSectionId;
            END
        END
        """;

    /// <summary>
    /// Takes an item off this board. The item stays in the library (Q97) - only the
    /// placement goes, exactly as deleting a section releases everything it held.
    /// </summary>
    private const string RemoveItemFromMenuSql = """
        DELETE FROM dbo.Placements
        OUTPUT 1 AS Value
        WHERE MenuId = @MenuId AND ItemId = @ItemId AND VenueId = @VenueId;
        """;

    /// <summary>
    /// The add row's search: the whole venue library, 86'd items included (Q112).
    /// Bounded by TOP, because the library has no ceiling of its own and a search
    /// that returns everything is a search nobody reads. Prefix matches sort first.
    /// </summary>
    private const string SearchItemsSql = """
        SELECT TOP (@Take) i.Id, i.VenueId, i.Name, i.Description, i.Price, i.ImageUrl,
               i.Source, i.IsActive, i.CreatedUtc, i.UpdatedUtc
        FROM dbo.Items i
        WHERE i.VenueId = @VenueId
          AND (@Pattern IS NULL OR i.Name LIKE @Pattern)
        ORDER BY CASE WHEN @Prefix IS NOT NULL AND i.Name LIKE @Prefix THEN 0 ELSE 1 END,
                 i.Name, i.Id;
        """;

    /// <summary>
    /// Which boards each of these items sits on, for "also on Late Night" (Q123).
    /// Menu names come from the row that owns them, so a renamed menu cannot be
    /// described by a stale label the caller was holding.
    /// </summary>
    private const string ItemBoardsSql = """
        SELECT p.ItemId, p.MenuId, m.Name AS MenuName
        FROM dbo.Placements p
        INNER JOIN dbo.Menus m ON m.Id = p.MenuId AND m.VenueId = p.VenueId
        INNER JOIN OPENJSON(@ItemIdsJson) ids
            ON p.ItemId = TRY_CONVERT(UNIQUEIDENTIFIER, ids.[value])
        WHERE p.VenueId = @VenueId
        ORDER BY m.Name, p.MenuId;
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

    // One projection, used wherever a board is read: the draft's two halves, and
    // the shelf's card for every menu at once. Written twice it would drift, and a
    // shelf card that disagrees with the draft it sits above is the defect this
    // milestone is most able to produce. Only the menu predicate differs, so only
    // the menu predicate is written twice.
    private const string WorkingSnapshotProjection = """
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
                WHERE s.MenuId = m.Id AND s.VenueId = @VenueId
                ORDER BY s.SortOrder, s.Id
                FOR JSON PATH
            )) AS sections
        FROM dbo.Menus m
        WHERE
        """;

    private const string WorkingSnapshotSuffix = """

        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        """;

    /// <summary>One named menu's board, for the draft.</summary>
    private const string WorkingSnapshotBody =
        WorkingSnapshotProjection + " m.Id = @MenuId AND m.VenueId = @VenueId " + WorkingSnapshotSuffix;

    /// <summary>The same board, correlated to the shelf row being read.</summary>
    private const string ShelfWorkingSnapshotBody =
        WorkingSnapshotProjection + " m.Id = shelf.Id AND m.VenueId = @VenueId " + WorkingSnapshotSuffix;

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

    // The board a screen is showing, with the version and author that put it there,
    // from ONE row. Reading the snapshot and then its version is the torn read this
    // codebase has produced repeatedly: a publish landing between the two hands back
    // one version's board labelled with another's.
    private const string LatestPublishedBoardSql = """
        SELECT TOP (1) Snapshot, Version, PublishedUtc, Author
        FROM dbo.MenuPublishEvents
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY Version DESC;
        """;

    // Every menu the venue has, with the board its screens are showing and the board
    // as it stands, in one statement. One round trip whatever the menu count: the
    // shelf shows thirteen menus at the scale this milestone ships (Q176), and asking
    // per menu would be thirteen diffs on every page load.
    //
    // The published half carries its version, time and author from the same row for
    // the reason above. The working half is the shared projection, so a card can
    // never describe a different board from the draft count beneath it.
    private const string ShelfSql = """
        SELECT
            shelf.Id AS MenuId,
            shelf.Name AS Name,
            shelf.Theme AS Theme,
            shelf.IsPutAway AS IsPutAway,
            latest.Version AS PublishedVersion,
            latest.PublishedUtc AS LastPublishedUtc,
            latest.Author AS LastPublishedBy,
            latest.Snapshot AS PublishedSnapshot,
            (
        """ + ShelfWorkingSnapshotBody + """
            ) AS WorkingSnapshot
        FROM dbo.Menus shelf
        OUTER APPLY
        (
            SELECT TOP (1) e.Snapshot, e.Version, e.PublishedUtc, e.Author
            FROM dbo.MenuPublishEvents e
            WHERE e.VenueId = @VenueId AND e.MenuId = shelf.Id
            ORDER BY e.Version DESC
        ) latest
        WHERE shelf.VenueId = @VenueId
        ORDER BY shelf.Name;
        """;

    // Duplicating a menu copies the working state onto a new menu that has never
    // been published and is on no screen (Q20). The placements point at the SAME
    // library items: sharing is the point of a library, so a later price edit
    // reaches both boards rather than quietly diverging.
    //
    // The ceiling is counted under the same lock as the insert, exactly as creating
    // a menu does - a duplicate is a new menu and must not be a way around the limit.
    //
    // The name is chosen inside that same lock, not handed in by the caller. Menu
    // names are not unique in the database, so two people duplicating the same menu
    // at once would otherwise both read "no 'Summer Menu copy' yet" and both use it.
    // Reading and writing a pair of values separately is the defect shape this
    // codebase produces most, so the read and the write are one statement.
    private const string DuplicateMenuWithinCeilingSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @SourceName NVARCHAR(200) =
        (
            SELECT Name FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @SourceMenuId AND VenueId = @VenueId
        );

        IF @SourceName IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51001, 'The menu does not belong to this venue.', 1;
        END;

        DECLARE @Active INT =
        (
            SELECT COUNT(*) FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK)
            WHERE VenueId = @VenueId AND IsPutAway = 0
        );

        IF @Active + 1 > @Limit
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT CAST(0 AS BIT) AS Created, @Active AS ActiveMenuCount, CAST(NULL AS NVARCHAR(200)) AS Name;
        END
        ELSE
        BEGIN
            -- "<Name> copy", then "<Name> copy 2" and upward while that is taken.
            -- The base is trimmed so the whole name still fits the 200-character
            -- limit: a long name loses its tail rather than the copy silently
            -- failing to be created.
            DECLARE @Base NVARCHAR(200) = LEFT(@SourceName, 200 - LEN(N' copy')) + N' copy';
            DECLARE @Name NVARCHAR(200) = @Base;
            DECLARE @Suffix INT = 1;

            WHILE EXISTS (
                SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK)
                WHERE VenueId = @VenueId AND Name = @Name)
            BEGIN
                SET @Suffix = @Suffix + 1;

                IF @Suffix > 999
                BEGIN
                    ROLLBACK TRANSACTION;
                    THROW 51009, 'There are already too many copies of that menu to name another one.', 1;
                END;

                DECLARE @Tail NVARCHAR(10) = CAST(@Suffix AS NVARCHAR(10));
                SET @Name = LEFT(@Base, 200 - LEN(@Tail) - 1) + N' ' + @Tail;
            END;

            -- The copy lands on the shelf, unpublished, on no screen: delivery is
            -- always deliberate, and one screen holds one menu.
            INSERT dbo.Menus (Id, VenueId, Name, IsActive, DwellSeconds, LoopWarningSeconds, Theme, IsPutAway, PublishedVersion, CreatedUtc, UpdatedUtc)
            SELECT @NewMenuId, @VenueId, @Name, 1, src.DwellSeconds, src.LoopWarningSeconds, src.Theme, 0, NULL, @Now, @Now
            FROM dbo.Menus src
            WHERE src.Id = @SourceMenuId AND src.VenueId = @VenueId;

            DECLARE @Sections TABLE (OldId UNIQUEIDENTIFIER, NewId UNIQUEIDENTIFIER);

            INSERT @Sections (OldId, NewId)
            SELECT s.Id, NEWID()
            FROM dbo.MenuSections s
            WHERE s.MenuId = @SourceMenuId AND s.VenueId = @VenueId;

            INSERT dbo.MenuSections (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            SELECT map.NewId, @VenueId, @NewMenuId, s.Name, s.SortOrder, @Now, @Now
            FROM dbo.MenuSections s
            INNER JOIN @Sections map ON map.OldId = s.Id
            WHERE s.VenueId = @VenueId;

            -- Same ItemId: the copy places the library's items, it does not clone them.
            INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
            SELECT NEWID(), @VenueId, @NewMenuId, map.NewId, p.ItemId, p.SortOrder, @Now, @Now
            FROM dbo.Placements p
            INNER JOIN @Sections map ON map.OldId = p.MenuSectionId
            WHERE p.VenueId = @VenueId AND p.MenuId = @SourceMenuId;

            INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
            VALUES (NEWID(), @VenueId, @NewMenuId, N'duplicated', NULL, NULL, @Detail, @Author, @Now);

            COMMIT TRANSACTION;
            SELECT CAST(1 AS BIT) AS Created, @Active + 1 AS ActiveMenuCount, @Name AS Name;
        END;
        """;

    // Both halves of the draft in one statement. Reading them separately let a
    // publish land between the two reads, which produced a diff against a version
    // that was already gone.
    // The published snapshot and its version come from one row, not from two
    // subqueries. Read separately, a publish committing between them hands back
    // one version's snapshot labelled with another's, and a diff computed from
    // that pair describes a comparison that never existed.
    // Everything a builder needs to open a menu, in ONE read: the board it draws
    // (Working), the board its screens are showing (Published), and the publish
    // that put the second there. The publish bar states all three in one sentence
    // - "3 changes not on your screens / published Tue 4:12pm by Dana" - so
    // reading them separately is how that sentence starts lying (Q182).
    private const string DraftSnapshotsSql = """
        SELECT
            latest.Snapshot AS Published,
            ISNULL(latest.Version, 0) AS PublishedVersion,
            latest.PublishedUtc AS PublishedUtc,
            latest.Author AS PublishedBy,
            (
        """ + WorkingSnapshotBody + """
            ) AS Working
        FROM (SELECT 1 AS Anchor) anchor
        OUTER APPLY (
            SELECT TOP (1) e.Snapshot, e.Version, e.PublishedUtc, e.Author
            FROM dbo.MenuPublishEvents e
            WHERE e.VenueId = @VenueId AND e.MenuId = @MenuId
            ORDER BY e.Version DESC
        ) latest;
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

        -- A restore puts back whatever shape it was given, screens included, so a
        -- version that was on a screen is a third way onto the shelf -- around the
        -- ceiling check and the record that make putting one back deliberate, and
        -- into a state the model says cannot exist: put away and on a screen.
        --
        -- The rule is that nothing puts a put-away menu on a screen except a
        -- deliberate put-back, not that a put-away menu is frozen. A shape with no
        -- screens in it cannot break that rule, so discarding a draft on a shelved
        -- menu -- the common case, since the menu had to leave its screens before
        -- it could be put away at all -- is still allowed.
        IF EXISTS (SELECT 1 FROM dbo.Menus WHERE Id = @MenuId AND VenueId = @VenueId AND IsPutAway = 1)
            AND EXISTS (SELECT 1 FROM OPENJSON(@SnapshotJson, '$.screens'))
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51008, 'This menu is put away. Put it back on the shelf before going back to a version it was on a screen for.', 1;
        END;

        -- A screen the recorded shape wants, which another menu has since been
        -- given, is a named refusal. Restoring around it would report success and
        -- leave the menu different from the version it claims to have gone back to.
        IF EXISTS (
            SELECT 1
            FROM OPENJSON(@SnapshotJson, '$.screens') WITH (screenId UNIQUEIDENTIFIER '$.screenId') want
            INNER JOIN dbo.MenuScreenAssignments a WITH (UPDLOCK, HOLDLOCK) ON a.ScreenId = want.screenId
            WHERE a.MenuId <> @MenuId)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51005, 'A screen this version was on is now showing a different menu.', 1;
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

        -- Theme is assigned straight across, without the ISNULL the other settings
        -- carry. A snapshot whose theme is null recorded a menu with no theme
        -- attached, which is a valid state (Q86) and a fact to restore -- not a
        -- gap to paper over with whatever is attached now. The guards stay on the
        -- others, where null means the snapshot simply did not record the field.
        UPDATE m
        SET m.Name = ISNULL(t.Name, m.Name),
            m.Theme = t.Theme,
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

        -- Sections are put back to the recorded shape, not merely updated where
        -- they happen to still exist: one added since is put away, one removed
        -- since comes back, and one deleted outright is recreated under its own id
        -- so its placements and history keep pointing at the same section.
        --
        -- (MenuId, SortOrder) is unique, so every section is first moved out of the
        -- numbering the snapshot is about to claim. Otherwise a restore that swaps
        -- two sections collides half-way through.
        UPDATE s
        SET s.SortOrder = 1000000 + parked.Position, s.UpdatedUtc = @OccurredUtc
        FROM dbo.MenuSections s
        INNER JOIN (
            SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) AS Position
            FROM dbo.MenuSections
            WHERE VenueId = @VenueId AND MenuId = @MenuId
        ) parked ON parked.Id = s.Id;

        UPDATE s
        SET s.Name = src.Name,
            s.SortOrder = src.SortOrder,
            s.UpdatedUtc = @OccurredUtc
        FROM dbo.MenuSections s
        INNER JOIN @Sections src ON src.SectionId = s.Id
        WHERE s.VenueId = @VenueId AND s.MenuId = @MenuId;

        INSERT dbo.MenuSections (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
        SELECT src.SectionId, @VenueId, @MenuId, src.Name, src.SortOrder, @OccurredUtc, @OccurredUtc
        FROM @Sections src
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuSections s WHERE s.Id = src.SectionId);

        -- A section added since the snapshot is deleted, and its placements with
        -- it. Its ITEMS survive: they were never in the section, a placement put
        -- them there, so they go back to the library exactly as a deliberate
        -- section delete leaves them (Q96). This used to hide the section behind
        -- IsActive = 0, which left a row nothing could ever reach again and a
        -- column whose next writer would silently change a live board.
        DELETE p
        FROM dbo.Placements p
        WHERE p.VenueId = @VenueId AND p.MenuId = @MenuId
          AND NOT EXISTS (SELECT 1 FROM @Sections src WHERE src.SectionId = p.MenuSectionId);

        DELETE s
        FROM dbo.MenuSections s
        WHERE s.VenueId = @VenueId AND s.MenuId = @MenuId
          AND NOT EXISTS (SELECT 1 FROM @Sections src WHERE src.SectionId = s.Id);

        DECLARE @Items TABLE (SectionId UNIQUEIDENTIFIER, ItemId UNIQUEIDENTIFIER, Name NVARCHAR(200), Description NVARCHAR(1000), Price NVARCHAR(40), SortOrder INT);

        -- One placement per item per board (UQ_Placements_MenuItem, migration 061).
        -- A snapshot recorded BEFORE that constraint existed can name the same item
        -- in two sections, and restoring it verbatim would fail on the constraint --
        -- turning an old version into one nobody can ever go back to. The first
        -- occurrence by section order then item order wins: the copy a guest would
        -- have read first on that board.
        WITH recorded AS
        (
            SELECT sec.SectionId, i.itemId, i.name, i.description, i.price, i.sortOrder,
                   ROW_NUMBER() OVER (
                       PARTITION BY i.itemId
                       ORDER BY sec.SortOrder, i.sortOrder) AS Rank
            FROM @Sections sec
            CROSS APPLY OPENJSON(sec.Items)
            WITH (
                itemId UNIQUEIDENTIFIER '$.itemId',
                name NVARCHAR(200) '$.name',
                description NVARCHAR(1000) '$.description',
                price NVARCHAR(40) '$.price',
                sortOrder INT '$.sortOrder'
            ) i
        )
        INSERT @Items (SectionId, ItemId, Name, Description, Price, SortOrder)
        SELECT SectionId, itemId, name, description, price, sortOrder
        FROM recorded
        WHERE Rank = 1;

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
    //
    // The caller's diff was computed from a working snapshot it read a moment ago.
    // This statement rebuilds that snapshot under the menu's lock and refuses if it
    // has moved, so the shipped set recorded in history always describes exactly
    // the snapshot committed. Without that check the two are separate observations
    // and can disagree (Q182).
    private const string PublishHeaderSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51001, 'The menu does not belong to this venue.', 1;
        END;

        -- A put-away menu is off the shelf. Publishing is not a way back on:
        -- putting one back is its own deliberate, ceiling-checked act.
        IF EXISTS (SELECT 1 FROM dbo.Menus WHERE Id = @MenuId AND VenueId = @VenueId AND IsPutAway = 1)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51007, 'This menu is put away. Put it back on the shelf before publishing it.', 1;
        END;

        DECLARE @PreviousVersion BIGINT =
        (
            SELECT ISNULL(MAX(Version), 0)
            FROM dbo.MenuPublishEvents WITH (UPDLOCK, HOLDLOCK)
            WHERE MenuId = @MenuId
        );

        DECLARE @PreviousSnapshot NVARCHAR(MAX) =
        (
            SELECT TOP (1) Snapshot
            FROM dbo.MenuPublishEvents WITH (UPDLOCK, HOLDLOCK)
            WHERE VenueId = @VenueId AND MenuId = @MenuId
            ORDER BY Version DESC
        );

        -- The caller's shipped set is the difference between a particular published
        -- snapshot and the working state. Both ends are proved here, not just the
        -- version: a version alone would still accept a diff computed from some
        -- other version's content, and the recorded set would describe a comparison
        -- that never existed. Either way the caller recomputes against what is
        -- actually published now (Q182).
        IF @PreviousVersion <> @ExpectedPublishedVersion
            OR (CASE WHEN @PreviousSnapshot IS NULL THEN 1 ELSE 0 END)
               <> (CASE WHEN @ExpectedPublishedSnapshot IS NULL THEN 1 ELSE 0 END)
            OR (@PreviousSnapshot IS NOT NULL
                AND @PreviousSnapshot COLLATE Latin1_General_BIN2
                    <> @ExpectedPublishedSnapshot COLLATE Latin1_General_BIN2)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51003, 'The menu was published by someone else while this publish was being prepared.', 1;
        END;

        -- Which screens were showing this menu comes from the published snapshot,
        -- never from the delivery rows. A delivery target records who was *told*
        -- about a publish - including the screens a take-off released - so reading
        -- membership from it would keep re-targeting screens already let go.
        DECLARE @PreviousScreens TABLE (ScreenId UNIQUEIDENTIFIER PRIMARY KEY);
        INSERT @PreviousScreens (ScreenId)
        SELECT DISTINCT screens.screenId
        FROM dbo.MenuPublishEvents e
        CROSS APPLY OPENJSON(e.Snapshot, '$.screens')
            WITH (screenId UNIQUEIDENTIFIER '$.screenId') screens
        WHERE e.VenueId = @VenueId AND e.MenuId = @MenuId AND e.Version = @PreviousVersion
          AND screens.screenId IS NOT NULL;

        -- A screen this menu was on, which another menu has since been given, is
        -- not this publish's to touch. Releasing it would blank content somebody
        -- else deliberately put there.
        DECLARE @Conflicts TABLE (ScreenId UNIQUEIDENTIFIER PRIMARY KEY);
        INSERT @Conflicts (ScreenId)
        SELECT p.ScreenId
        FROM @PreviousScreens p
        WHERE EXISTS (
            SELECT 1 FROM dbo.MenuScreenAssignments a WITH (UPDLOCK, HOLDLOCK)
            WHERE a.ScreenId = p.ScreenId AND a.MenuId <> @MenuId);

        -- Where this publish lands: the screens the menu is on now, plus the ones
        -- it was on and is now leaving, minus the ones another menu has taken.
        DECLARE @Targets TABLE (ScreenId UNIQUEIDENTIFIER PRIMARY KEY);
        INSERT @Targets (ScreenId)
        SELECT a.ScreenId
        FROM dbo.MenuScreenAssignments a WITH (UPDLOCK, HOLDLOCK)
        WHERE a.VenueId = @VenueId AND a.MenuId = @MenuId
        UNION
        SELECT p.ScreenId
        FROM @PreviousScreens p
        WHERE NOT EXISTS (SELECT 1 FROM @Conflicts c WHERE c.ScreenId = p.ScreenId);

        IF NOT EXISTS (SELECT 1 FROM @Targets)
        BEGIN
            IF EXISTS (SELECT 1 FROM @Conflicts)
            BEGIN
                ROLLBACK TRANSACTION;
                THROW 51004, 'Every screen this menu was on is now showing a different menu, so this publish would reach nothing.', 1;
            END;

            -- Q80: a publish that can reach nothing, and has nothing to release, is
            -- a named refusal rather than a silent version bump.
            ROLLBACK TRANSACTION;
            THROW 51002, 'Pair a screen to publish. This menu is not on a screen yet, so publishing it would reach nothing.', 1;
        END;

        DECLARE @ResolvedVersion BIGINT = @PreviousVersion + 1;

        DECLARE @SnapshotJson NVARCHAR(MAX) =
        (
        """;

    private const string PublishTailSql = """
        );

        -- The shipped set the caller computed describes the menu it read. If the
        -- menu has moved since, publishing now would record a set that does not
        -- match the snapshot going out; the caller recomputes and tries again.
        --
        -- The comparison is binary on purpose. The database collation is
        -- case- and accent-insensitive, so a plain <> would read "Burger" and
        -- "burger" as the same snapshot and let exactly the mismatch this guard
        -- exists to prevent through.
        IF @ExpectedSnapshot IS NULL
            OR @SnapshotJson IS NULL
            OR @SnapshotJson COLLATE Latin1_General_BIN2 <> @ExpectedSnapshot COLLATE Latin1_General_BIN2
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51003, 'The menu changed while it was being published.', 1;
        END;

        -- The count is not taken from the caller. It and the shipped set are two
        -- recordings of one fact, and a version that claims a number its own set does
        -- not contain is the same disagreement Q182 exists to prevent - just written
        -- into history instead of onto a screen. Deriving it here makes them agree by
        -- construction rather than by everyone remembering to pass both.
        DECLARE @ResolvedChangeCount INT =
            CASE WHEN @ShippedChanges IS NULL OR ISJSON(@ShippedChanges) = 0
                 THEN 0
                 ELSE (SELECT COUNT(*) FROM OPENJSON(@ShippedChanges))
            END;

        INSERT dbo.MenuPublishEvents (Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot, ShippedChanges)
        VALUES (@Id, @VenueId, @MenuId, @ResolvedVersion, @ResolvedChangeCount, @Author, @PublishedUtc, @SnapshotJson, @ShippedChanges);

        -- The screens that were showing this menu are told about the publish even
        -- when they are being released, so a take-off reaches them instead of
        -- leaving them on stale content.
        INSERT dbo.MenuPublishTargets (Id, VenueId, PublishEventId, ScreenId, State, UpdatedUtc)
        SELECT NEWID(), @VenueId, @Id, screens.ScreenId,
               CASE WHEN sc.Status = N'Online' THEN N'Pending' ELSE N'Offline' END,
               @PublishedUtc
        FROM @Targets screens
        INNER JOIN dbo.Screens sc ON sc.Id = screens.ScreenId AND sc.VenueId = @VenueId;

        INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
        VALUES (NEWID(), @VenueId, @MenuId, N'published', @Id, NULL, @Detail, @Author, @PublishedUtc);

        -- Taking a menu off its screens is permanent and reaches them here, so the
        -- publish that carries it records that act under its own name (Q68, Q207).
        IF NOT EXISTS (SELECT 1 FROM dbo.MenuScreenAssignments WHERE VenueId = @VenueId AND MenuId = @MenuId)
           AND EXISTS (SELECT 1 FROM @Targets)
        BEGIN
            INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
            VALUES (NEWID(), @VenueId, @MenuId, N'taken_off_screens', @Id, NULL,
                    CONCAT(N'Taken off ', (SELECT COUNT(*) FROM @Targets), N' screen(s).'), @Author, @PublishedUtc);
        END;

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

        -- Publishing does not bring a put-away menu back: putting one back is its
        -- own deliberate act, bounded by the ceiling and recorded with its author.
        UPDATE dbo.Menus
        SET PublishedVersion = @ResolvedVersion,
            UpdatedUtc = @PublishedUtc
        WHERE Id = @MenuId AND VenueId = @VenueId;

        COMMIT TRANSACTION;

        SELECT e.Id, e.VenueId, e.MenuId, e.Version, e.ChangeCount, e.Author, e.PublishedUtc, e.Snapshot, e.ShippedChanges,
               (SELECT STRING_AGG(CONVERT(NVARCHAR(36), c.ScreenId), ',') FROM @Conflicts c) AS ConflictedScreenIds
        FROM dbo.MenuPublishEvents e
        WHERE e.Id = @Id;
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

    // Version comes from the publish event the entry names. Without it the only
    // place a client ever learns a version is the response to its own publish, so
    // "Go back to..." - which is addressed by version - is unreachable from a list
    // of what happened. Null for the kinds that are not a publish.
    private const string HistorySql = """
        SELECT TOP (@Limit)
            h.Id, h.VenueId, h.MenuId, h.Kind, h.PublishEventId, h.ReplacedByVersion,
            h.Detail, h.Author, h.OccurredUtc, e.Version
        FROM dbo.MenuHistoryEntries h
        LEFT JOIN dbo.MenuPublishEvents e
            ON e.Id = h.PublishEventId AND e.MenuId = h.MenuId AND e.VenueId = h.VenueId
        WHERE h.VenueId = @VenueId AND h.MenuId = @MenuId
        ORDER BY h.OccurredUtc DESC, h.Id DESC;
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

    // ---- the builder's writes -------------------------------------------------

    public async Task<SectionCreateOutcome> CreateSectionOnMenuAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<SectionCreateRow, object>(
            CreateSectionOnMenuSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                Name = name,
                Now = now
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new SectionCreateOutcome(row.Outcome, row.SortOrder);
    }

    public async Task<bool> RenameSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        DateTime now,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            RenameSectionSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                Name = name,
                Now = now
            },
            cancellationToken).ConfigureAwait(false)).Any();

    public async Task<SectionDeleteOutcome> DeleteSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<SectionDeleteRow, object>(
            DeleteSectionSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId))
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new SectionDeleteOutcome(row.Outcome, row.Released);
    }

    public async Task<ReorderOutcome> ReorderSectionsGuardedAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sectionIds);
        var row = (await dataAccess.ExecuteSqlQueryAsync<ReorderRow, object>(
            ReorderSectionsGuardedSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionIdsJson = System.Text.Json.JsonSerializer.Serialize(sectionIds),
                Now = now
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new ReorderOutcome(row.Outcome, row.Moved);
    }

    public async Task<ReorderOutcome> ReorderPlacementsGuardedAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var row = (await dataAccess.ExecuteSqlQueryAsync<ReorderRow, object>(
            ReorderPlacementsGuardedSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                ItemIdsJson = System.Text.Json.JsonSerializer.Serialize(itemIds),
                Now = now
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new ReorderOutcome(row.Outcome, row.Moved);
    }

    public async Task<PlaceExistingOutcome> PlaceExistingItemAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        int itemsPerMenuLimit,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<PlaceExistingRow, object>(
            PlaceExistingItemSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                ItemId = RequireId(itemId, nameof(itemId)),
                PlacementId = Guid.NewGuid(),
                ItemsPerMenuLimit = itemsPerMenuLimit,
                Now = now
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new PlaceExistingOutcome(row.Outcome, row.ItemCountOnMenu, row.SortOrder, row.ExistingSectionId);
    }

    public async Task<bool> RemoveItemFromMenuAsync(
        Guid venueId,
        Guid menuId,
        Guid itemId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            RemoveItemFromMenuSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                ItemId = RequireId(itemId, nameof(itemId))
            },
            cancellationToken).ConfigureAwait(false)).Any();

    public async Task<IReadOnlyCollection<Item>> SearchItemsAsync(
        Guid venueId,
        string? query,
        int take,
        CancellationToken cancellationToken = default)
    {
        // LIKE wildcards typed into a search box are characters, not operators: a
        // person looking for "50% off" should not match everything.
        var trimmed = (query ?? string.Empty).Trim();
        var escaped = trimmed
            .Replace("[", "[[]", StringComparison.Ordinal)
            .Replace("%", "[%]", StringComparison.Ordinal)
            .Replace("_", "[_]", StringComparison.Ordinal);

        return (await dataAccess.ExecuteSqlQueryAsync<Item, object>(
            SearchItemsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                Take = take,
                Pattern = trimmed.Length == 0 ? null : $"%{escaped}%",
                Prefix = trimmed.Length == 0 ? null : $"{escaped}%"
            },
            cancellationToken).ConfigureAwait(false)).ToArray();
    }

    public async Task<IReadOnlyCollection<ItemBoard>> GetItemBoardsAsync(
        Guid venueId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        if (itemIds.Count == 0)
        {
            return [];
        }

        return (await dataAccess.ExecuteSqlQueryAsync<ItemBoard, object>(
            ItemBoardsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                ItemIdsJson = System.Text.Json.JsonSerializer.Serialize(itemIds)
            },
            cancellationToken).ConfigureAwait(false)).ToArray();
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
        try
        {
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
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51006)
        {
            throw new MenuPutAwayException(exception.Message);
        }
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

    public async Task<int> TakeOffScreensAsync(
        Guid venueId,
        Guid menuId,
        string? author,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            TakeOffScreensSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                Author = author,
                OccurredUtc = occurredUtc
            },
            cancellationToken).ConfigureAwait(false)).Single().Value;

    public async Task<PutAwayOutcome> SetPutAwayAsync(
        Guid venueId,
        Guid menuId,
        bool isPutAway,
        int activeMenuLimit,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<PutAwayRow, object>(
            SetPutAwaySql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                IsPutAway = isPutAway,
                Limit = activeMenuLimit,
                Kind = isPutAway ? MenuHistoryKinds.PutAway : MenuHistoryKinds.PutBack,
                Detail = detail,
                Author = author,
                OccurredUtc = occurredUtc
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new PutAwayOutcome(row.Outcome, row.ActiveMenuCount);
    }

    public async Task<IReadOnlyCollection<MenuScreenAssignment>> GetAssignmentsAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuScreenAssignment, object>(
            AssignmentsSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<ScreenShowing>> GetScreensShowingAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScreenShowing, object>(
            ScreensShowingSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    // ----- Publish and history ---------------------------------------------------------

    public async Task<PublishOutcome> PublishAsync(
        MenuPublishEvent publishEvent,
        string? shippedChanges,
        string expectedWorkingSnapshot,
        string? expectedPublishedSnapshot,
        long expectedPublishedVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(publishEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedWorkingSnapshot);

        // Targets are resolved inside the statement from the assignments and the
        // previous publish, never from a caller-supplied list that could name
        // another venue's screens. The named refusals are raised by the statement
        // and translated here, so no caller has to know a SQL error number.
        PublishRow row;
        try
        {
            row = (await dataAccess.ExecuteSqlQueryAsync<PublishRow, object>(
                PublishSql,
                new
                {
                    Id = publishEvent.Id == Guid.Empty ? Guid.NewGuid() : publishEvent.Id,
                    VenueId = RequireId(publishEvent.VenueId, nameof(publishEvent.VenueId)),
                    MenuId = RequireId(publishEvent.MenuId, nameof(publishEvent.MenuId)),
                    publishEvent.Author,
                    PublishedUtc = publishEvent.PublishedUtc == default ? DateTime.UtcNow : publishEvent.PublishedUtc,
                    ShippedChanges = shippedChanges,
                    ExpectedSnapshot = expectedWorkingSnapshot,
                    ExpectedPublishedSnapshot = expectedPublishedSnapshot,
                    ExpectedPublishedVersion = expectedPublishedVersion,
                    Detail = (string?)null
                },
                cancellationToken).ConfigureAwait(false)).Single();
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51002)
        {
            throw new MenuNotOnAnyScreenException(exception.Message);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51003)
        {
            throw new MenuMovedWhilePublishingException(exception.Message);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51004)
        {
            throw new ScreensTakenByAnotherMenuException(exception.Message);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51007)
        {
            throw new MenuPutAwayException(exception.Message);
        }

        var conflicted = string.IsNullOrWhiteSpace(row.ConflictedScreenIds)
            ? []
            : row.ConflictedScreenIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Guid.Parse)
                .ToArray();

        return new PublishOutcome(
            new MenuPublishEvent
            {
                Id = row.Id,
                VenueId = row.VenueId,
                MenuId = row.MenuId,
                Version = row.Version,
                ChangeCount = row.ChangeCount,
                Author = row.Author,
                PublishedUtc = row.PublishedUtc,
                Snapshot = row.Snapshot,
                ShippedChanges = row.ShippedChanges
            },
            conflicted);
    }

    public async Task<DraftSnapshots> GetDraftSnapshotsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<DraftSnapshotRow, object>(
            DraftSnapshotsSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

        return new DraftSnapshots(row?.Published, row?.Working, row?.PublishedVersion ?? 0, row?.PublishedUtc, row?.PublishedBy);
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

    public async Task<PublishedBoard?> GetLatestPublishedBoardAsync(
        Guid venueId,
        Guid menuId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PublishedBoard, object>(
            LatestPublishedBoardSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId))
            },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task<IReadOnlyCollection<ShelfMenu>> GetShelfAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ShelfMenu, object>(
            ShelfSql,
            new { VenueId = RequireId(venueId, nameof(venueId)) },
            cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<MenuDuplicateOutcome> DuplicateMenuWithinCeilingAsync(
        Guid venueId,
        Guid sourceMenuId,
        Guid newMenuId,
        int activeMenuLimit,
        string? author,
        string detail,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var row = (await dataAccess.ExecuteSqlQueryAsync<MenuDuplicateRow, object>(
                DuplicateMenuWithinCeilingSql,
                new
                {
                    VenueId = RequireId(venueId, nameof(venueId)),
                    SourceMenuId = RequireId(sourceMenuId, nameof(sourceMenuId)),
                    NewMenuId = RequireId(newMenuId, nameof(newMenuId)),
                    Limit = activeMenuLimit,
                    Author = author,
                    Detail = detail,
                    Now = occurredUtc
                },
                cancellationToken).ConfigureAwait(false)).Single();

            return new MenuDuplicateOutcome(row.Created, row.ActiveMenuCount, row.Name);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51009)
        {
            throw new TooManyMenuCopiesException(exception.Message);
        }
    }

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

        try
        {
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
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51005)
        {
            throw new ScreensTakenByAnotherMenuException(exception.Message);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51008)
        {
            throw new MenuPutAwayException(exception.Message);
        }
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

    private sealed class MenuDuplicateRow
    {
        public bool Created { get; set; }

        public int ActiveMenuCount { get; set; }

        public string? Name { get; set; }
    }

    private sealed class ItemPlacementRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int ItemCountOnMenu { get; set; }

        public int SortOrder { get; set; }
    }

    private sealed class PutAwayRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int ActiveMenuCount { get; set; }
    }

    private sealed class SectionCreateRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int SortOrder { get; set; }
    }

    private sealed class SectionDeleteRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int Released { get; set; }
    }

    private sealed class ReorderRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int Moved { get; set; }
    }

    private sealed class PlaceExistingRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int ItemCountOnMenu { get; set; }

        public int SortOrder { get; set; }

        public Guid? ExistingSectionId { get; set; }
    }

    private sealed class DraftSnapshotRow
    {
        public string? Published { get; set; }

        public long PublishedVersion { get; set; }

        public DateTime? PublishedUtc { get; set; }

        public string? PublishedBy { get; set; }

        public string? Working { get; set; }
    }

    private sealed class PublishRow
    {
        public Guid Id { get; set; }

        public Guid VenueId { get; set; }

        public Guid MenuId { get; set; }

        public long Version { get; set; }

        public int ChangeCount { get; set; }

        public string? Author { get; set; }

        public DateTime PublishedUtc { get; set; }

        public string? Snapshot { get; set; }

        public string? ShippedChanges { get; set; }

        public string? ConflictedScreenIds { get; set; }
    }
}
