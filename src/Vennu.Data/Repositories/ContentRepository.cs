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
        SELECT Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc
        FROM dbo.Placements
        WHERE VenueId = @VenueId AND MenuId = @MenuId
        ORDER BY MenuSectionId, SortOrder, Id;
        """;

    private const string PlacementsForItemSql = """
        SELECT Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc
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

        IF @Rotate = 0
            DELETE dbo.MenuScreenAssignments WHERE ScreenId=@ScreenId AND VenueId=@VenueId AND PageId<>@PageId;

        MERGE dbo.MenuScreenAssignments WITH (HOLDLOCK) AS target
        USING (
            SELECT s.Id AS ScreenId, p.Id AS PageId
            FROM dbo.Screens s
            CROSS APPLY (
                SELECT mp.Id
                FROM dbo.MenuPages mp
                WHERE mp.Id=@PageId AND mp.MenuId = @MenuId AND mp.VenueId = @VenueId
            ) p
            WHERE s.Id = @ScreenId
              AND s.VenueId = @VenueId
              AND EXISTS (SELECT 1 FROM dbo.Menus m WHERE m.Id = @MenuId AND m.VenueId = @VenueId)
        ) AS source
            ON target.ScreenId = source.ScreenId AND target.PageId = source.PageId
        WHEN MATCHED THEN
            UPDATE SET MenuId = @MenuId, PageId = source.PageId, VenueId = @VenueId, AssignedUtc = @AssignedUtc, AssignedBy = @AssignedBy
        WHEN NOT MATCHED THEN
            INSERT (Id, VenueId, ScreenId, MenuId, PageId, AssignedUtc, AssignedBy)
            VALUES (@Id, @VenueId, @ScreenId, @MenuId, source.PageId, @AssignedUtc, @AssignedBy)
        OUTPUT inserted.Id, inserted.VenueId, inserted.ScreenId, inserted.MenuId, inserted.PageId, inserted.AssignedUtc, inserted.AssignedBy;

        COMMIT TRANSACTION;
        """;

    private const string ClearScreenAssignmentSql = """
        DELETE FROM dbo.MenuScreenAssignments
        OUTPUT 1 AS Value
        WHERE VenueId = @VenueId AND ScreenId = @ScreenId;
        """;

    private const string ClearPageScreenAssignmentSql = """
        DELETE FROM dbo.MenuScreenAssignments
        OUTPUT 1 AS Value
        WHERE VenueId = @VenueId
          AND ScreenId = @ScreenId
          AND MenuId = @MenuId
          AND PageId = @PageId;
        """;

    private const string ApplyPageScreenAssignmentsSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Changes TABLE (ScreenId UNIQUEIDENTIFIER NOT NULL, PageId UNIQUEIDENTIFIER NOT NULL, Mode NVARCHAR(10) NOT NULL, PRIMARY KEY(ScreenId,PageId));
        INSERT @Changes (ScreenId, PageId, Mode)
        SELECT ScreenId, PageId, Mode
        FROM OPENJSON(@ChangesJson)
        WITH (ScreenId UNIQUEIDENTIFIER '$.screenId', PageId UNIQUEIDENTIFIER '$.pageId', Mode NVARCHAR(10) '$.mode');

        IF EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id=@MenuId AND VenueId=@VenueId AND IsPutAway=1)
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51006, 'This menu is put away. Put it back on the shelf before giving it a screen.', 1;
        END;

        IF EXISTS (SELECT 1 FROM @Changes WHERE Mode NOT IN (N'remove', N'replace', N'rotate'))
           OR (SELECT COUNT(*) FROM @Changes) <> @ExpectedCount
           OR EXISTS (SELECT 1 FROM @Changes c WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuPages p WHERE p.Id=c.PageId AND p.MenuId=@MenuId AND p.VenueId=@VenueId))
           OR EXISTS (SELECT 1 FROM @Changes GROUP BY ScreenId HAVING SUM(CASE WHEN Mode=N'replace' THEN 1 ELSE 0 END) > 1)
           OR EXISTS (
                SELECT 1 FROM @Changes c
                WHERE NOT EXISTS (SELECT 1 FROM dbo.Screens s WITH (UPDLOCK, HOLDLOCK) WHERE s.Id=c.ScreenId AND s.VenueId=@VenueId)
           )
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51008, 'The screen assignment changed before it could be saved. Nothing was changed.', 1;
        END;

        DELETE a
        FROM dbo.MenuScreenAssignments a
        INNER JOIN @Changes c ON c.ScreenId=a.ScreenId AND c.Mode=N'remove'
        WHERE a.VenueId=@VenueId AND a.MenuId=@MenuId AND a.PageId=c.PageId;

        DELETE a
        FROM dbo.MenuScreenAssignments a
        INNER JOIN @Changes c ON c.ScreenId=a.ScreenId AND c.Mode=N'replace'
        WHERE a.VenueId=@VenueId AND a.PageId<>c.PageId;

        MERGE dbo.MenuScreenAssignments WITH (HOLDLOCK) AS target
        USING (
            SELECT c.ScreenId, c.PageId, c.Mode FROM @Changes c WHERE c.Mode IN (N'replace', N'rotate')
        ) AS source
        ON target.ScreenId=source.ScreenId AND target.PageId=source.PageId
        WHEN MATCHED THEN UPDATE SET MenuId=@MenuId, VenueId=@VenueId, AssignedUtc=@OccurredUtc, AssignedBy=@Author
        WHEN NOT MATCHED THEN
            INSERT (Id,VenueId,ScreenId,MenuId,PageId,AssignedUtc,AssignedBy)
            VALUES (NEWID(),@VenueId,source.ScreenId,@MenuId,source.PageId,@OccurredUtc,@Author);

        IF EXISTS (SELECT 1 FROM @Changes)
            INSERT dbo.MenuHistoryEntries (Id,VenueId,MenuId,Kind,PublishEventId,ReplacedByVersion,Detail,Author,OccurredUtc)
            VALUES (NEWID(),@VenueId,@MenuId,N'assigned',NULL,NULL,N'Updated screen assignments.',@Author,@OccurredUtc);

        COMMIT TRANSACTION;
        SELECT 1 AS Value;
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
            sc.Location,
            sc.Status,
            sc.LastSeen AS LastSeenUtc,
            sc.WidthPixels,
            sc.HeightPixels,
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
        SELECT a.Id,a.VenueId,a.ScreenId,a.MenuId,a.PageId,a.AssignedUtc,a.AssignedBy,m.Name MenuName,p.Name PageName
        FROM dbo.MenuScreenAssignments a
        INNER JOIN dbo.Menus m ON m.Id=a.MenuId AND m.VenueId=a.VenueId
        INNER JOIN dbo.MenuPages p ON p.Id=a.PageId AND p.MenuId=a.MenuId AND p.VenueId=a.VenueId
        WHERE a.VenueId = @VenueId
        ORDER BY a.ScreenId,p.SortOrder,a.AssignedUtc;
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

            INSERT dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            VALUES (@PageId, @VenueId, @Id, N'Page 1', 0, @Now, @Now);

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

                INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
                SELECT @PlacementId, @VenueId, @MenuId, @SectionId, PageId, @ItemId, @SortOrder, @Now, @Now
                FROM dbo.MenuSections WHERE Id=@SectionId AND VenueId=@VenueId;

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
            DECLARE @ResolvedPageId UNIQUEIDENTIFIER = COALESCE(@RequestedPageId, (
                SELECT TOP (1) Id FROM dbo.MenuPages
                WHERE MenuId = @MenuId AND VenueId = @VenueId
                ORDER BY SortOrder, Id));
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuPages WHERE Id=@ResolvedPageId AND MenuId=@MenuId AND VenueId=@VenueId)
            BEGIN ROLLBACK; SELECT N'menu_missing' AS Outcome, 0 AS SortOrder; RETURN; END;
            DECLARE @Next INT = ISNULL((
                SELECT MAX(SortOrder) FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
                WHERE PageId = @ResolvedPageId AND VenueId = @VenueId), -1) + 1;

            INSERT dbo.MenuSections (Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            VALUES (@SectionId, @VenueId, @MenuId, @ResolvedPageId, @Name, @Next, @Now, @Now);

            INSERT dbo.MenuHistoryEntries
                (Id,VenueId,MenuId,PageId,PageName,Kind,Detail,Author,OccurredUtc)
            SELECT NEWID(),@VenueId,@MenuId,p.Id,p.Name,N'section_added',
                   CONCAT(N'Section added — ', @Name),@Author,@Now
            FROM dbo.MenuPages p
            WHERE p.Id=@ResolvedPageId AND p.MenuId=@MenuId AND p.VenueId=@VenueId;

            COMMIT TRANSACTION;
            SELECT N'created' AS Outcome, @Next AS SortOrder;
        END
        """;

    private const string RenameSectionSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;
        DECLARE @Changed TABLE (PageId UNIQUEIDENTIFIER, OldName NVARCHAR(200));
        UPDATE dbo.MenuSections
        SET Name = @Name, UpdatedUtc = @Now
        OUTPUT inserted.PageId, deleted.Name INTO @Changed(PageId,OldName)
        WHERE Id = @SectionId AND MenuId = @MenuId AND VenueId = @VenueId
          AND Name COLLATE Latin1_General_100_BIN2 <> @Name COLLATE Latin1_General_100_BIN2;

        INSERT dbo.MenuHistoryEntries
            (Id,VenueId,MenuId,PageId,PageName,Kind,Detail,Author,OccurredUtc)
        SELECT NEWID(),@VenueId,@MenuId,c.PageId,p.Name,N'section_renamed',
               CONCAT(c.OldName,N' renamed to ',@Name),@Author,@Now
        FROM @Changed c INNER JOIN dbo.MenuPages p
          ON p.Id=c.PageId AND p.MenuId=@MenuId AND p.VenueId=@VenueId;
        COMMIT TRANSACTION;
        SELECT TOP (1) 1 AS Value
        FROM dbo.MenuSections
        WHERE Id=@SectionId AND MenuId=@MenuId AND VenueId=@VenueId;
        """;

    /// <summary>
    /// Deletes a section after atomically moving its placements to a sibling or
    /// releasing them to the library. Every refusal happens before a write.
    /// </summary>
    private const string DeleteSectionSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
            WHERE Id = @SectionId AND MenuId = @MenuId AND VenueId = @VenueId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'section_missing' AS Outcome, 0 AS Moved, 0 AS Released;
        END
        ELSE
        BEGIN
            DECLARE @PlacementCount INT =
                (SELECT COUNT(*) FROM dbo.Placements WHERE MenuSectionId = @SectionId AND VenueId = @VenueId);
            DECLARE @SourcePageId UNIQUEIDENTIFIER =
                (SELECT PageId FROM dbo.MenuSections WHERE Id=@SectionId AND MenuId=@MenuId AND VenueId=@VenueId);
            DECLARE @SourceName NVARCHAR(200) =
                (SELECT Name FROM dbo.MenuSections WHERE Id=@SectionId AND MenuId=@MenuId AND VenueId=@VenueId);
            DECLARE @PageName NVARCHAR(200) =
                (SELECT Name FROM dbo.MenuPages WHERE Id=@SourcePageId AND MenuId=@MenuId AND VenueId=@VenueId);

            IF @PlacementCount > 0 AND @DeletePlacements = 0 AND NOT EXISTS (
                SELECT 1 FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
                WHERE Id=@MoveItemsToSectionId AND Id<>@SectionId AND MenuId=@MenuId
                  AND VenueId=@VenueId AND PageId=@SourcePageId)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT N'destination_missing' AS Outcome, 0 AS Moved, 0 AS Released;
            END
            ELSE IF @PlacementCount > 0 AND @DeletePlacements = 0 AND EXISTS (
                SELECT 1 FROM dbo.Placements sourcePlacement
                INNER JOIN dbo.Placements destinationPlacement
                    ON destinationPlacement.MenuSectionId=@MoveItemsToSectionId
                   AND destinationPlacement.ItemId=sourcePlacement.ItemId
                   AND destinationPlacement.VenueId=@VenueId
                WHERE sourcePlacement.MenuSectionId=@SectionId AND sourcePlacement.VenueId=@VenueId)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT N'destination_conflict' AS Outcome, 0 AS Moved, 0 AS Released;
            END
            ELSE
            BEGIN
                IF @PlacementCount > 0 AND @DeletePlacements = 0
                BEGIN
                    DECLARE @Offset INT = ISNULL((SELECT MAX(SortOrder)+1 FROM dbo.Placements WHERE MenuSectionId=@MoveItemsToSectionId AND VenueId=@VenueId),0);
                    UPDATE dbo.Placements
                    SET MenuSectionId=@MoveItemsToSectionId, SortOrder=@Offset+SortOrder, UpdatedUtc=SYSUTCDATETIME()
                    WHERE MenuSectionId=@SectionId AND VenueId=@VenueId;
                END
                ELSE
                    DELETE FROM dbo.Placements WHERE MenuSectionId=@SectionId AND VenueId=@VenueId;

                DELETE FROM dbo.MenuSections WHERE Id=@SectionId AND MenuId=@MenuId AND VenueId=@VenueId;
                INSERT dbo.MenuHistoryEntries
                    (Id,VenueId,MenuId,PageId,PageName,Kind,Detail,Author,OccurredUtc)
                VALUES (NEWID(),@VenueId,@MenuId,@SourcePageId,@PageName,N'section_deleted',
                    CASE WHEN @PlacementCount=0 THEN CONCAT(N'Section deleted — ',@SourceName)
                         WHEN @DeletePlacements=1 THEN CONCAT(@SourceName,N' deleted; ',@PlacementCount,N' items returned to the library')
                         ELSE CONCAT(@SourceName,N' deleted; ',@PlacementCount,N' items moved') END,
                    @Author,@Now);
                COMMIT TRANSACTION;
                SELECT N'deleted' AS Outcome,
                       CASE WHEN @DeletePlacements=0 THEN @PlacementCount ELSE 0 END AS Moved,
                       CASE WHEN @DeletePlacements=1 THEN @PlacementCount ELSE 0 END AS Released;
            END
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

        DECLARE @PageId UNIQUEIDENTIFIER = (
            SELECT TOP (1) s.PageId FROM @Order o
            INNER JOIN dbo.MenuSections s ON s.Id=o.SectionId AND s.MenuId=@MenuId AND s.VenueId=@VenueId);
        DECLARE @Live INT = (
            SELECT COUNT(*) FROM dbo.MenuSections WITH (UPDLOCK, HOLDLOCK)
            WHERE MenuId = @MenuId AND VenueId = @VenueId AND PageId=@PageId);

        IF @Live <> (SELECT COUNT(*) FROM @Order)
           OR EXISTS (
               SELECT 1 FROM @Order o
               WHERE NOT EXISTS (
                   SELECT 1 FROM dbo.MenuSections s
                   WHERE s.Id = o.SectionId AND s.MenuId = @MenuId AND s.VenueId = @VenueId AND s.PageId=@PageId))
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

            INSERT dbo.MenuHistoryEntries
                (Id,VenueId,MenuId,PageId,PageName,Kind,Detail,Author,OccurredUtc)
            SELECT NEWID(),@VenueId,@MenuId,p.Id,p.Name,N'sections_reordered',
                   N'Sections reordered',@Author,@Now
            FROM dbo.MenuPages p
            WHERE p.Id=@PageId AND p.MenuId=@MenuId AND p.VenueId=@VenueId;

            COMMIT TRANSACTION;
            SELECT N'reordered' AS Outcome, @Live AS Moved;
        END
        """;

    /// <summary>
    /// An edit that can be made conditional on the values still being the ones the
    /// caller last saw.
    ///
    /// This exists for Undo. An unconditional inverse restores a value captured
    /// before somebody else's edit and erases it silently — the reader of the board
    /// sees their own change vanish with nothing said. Comparing in a prior read
    /// would not help: the row can change between that read and this write, which is
    /// why the comparison happens under the lock that writes.
    ///
    /// NULL and empty are the same absence here. The API normalises an empty
    /// description or price to NULL on the way in, so a caller echoing back what it
    /// was handed must not be refused over which of the two it sent.
    /// </summary>
    private const string UpdateItemValuesGuardedSql = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @Name NVARCHAR(200), @Description NVARCHAR(1000), @Price NVARCHAR(40);
        SELECT @Name = Name, @Description = Description, @Price = Price
        FROM dbo.Items WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @ItemId AND VenueId = @VenueId;

        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'not_found' AS Outcome, NULL AS Name, NULL AS Description, NULL AS Price;
        END
        ELSE IF @Guarded = 1
           AND (@Name COLLATE Latin1_General_100_BIN2 <> @ExpectedName COLLATE Latin1_General_100_BIN2
             OR ISNULL(@Description, N'') COLLATE Latin1_General_100_BIN2 <> ISNULL(@ExpectedDescription, N'') COLLATE Latin1_General_100_BIN2
             OR ISNULL(@Price, N'') COLLATE Latin1_General_100_BIN2 <> ISNULL(@ExpectedPrice, N'') COLLATE Latin1_General_100_BIN2)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT N'item_changed' AS Outcome, @Name AS Name, @Description AS Description, @Price AS Price;
        END
        ELSE
        BEGIN
            UPDATE dbo.Items
            SET Name = @NewName,
                Description = @NewDescription,
                Price = @NewPrice,
                UpdatedUtc = @Now
            WHERE Id = @ItemId AND VenueId = @VenueId;

            COMMIT TRANSACTION;
            SELECT N'updated' AS Outcome, @NewName AS Name, @NewDescription AS Description, @NewPrice AS Price;
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
            DECLARE @TargetPageId UNIQUEIDENTIFIER=(SELECT PageId FROM dbo.MenuSections WHERE Id=@SectionId AND VenueId=@VenueId);
            DECLARE @Existing UNIQUEIDENTIFIER = (
                SELECT TOP (1) p.MenuSectionId FROM dbo.Placements p WITH (UPDLOCK, HOLDLOCK)
                INNER JOIN dbo.MenuSections s ON s.Id=p.MenuSectionId AND s.VenueId=p.VenueId
                WHERE p.MenuId = @MenuId AND p.ItemId = @ItemId AND p.VenueId = @VenueId AND s.PageId=@TargetPageId);

            DECLARE @OnMenu INT = (
                SELECT COUNT(DISTINCT ItemId) FROM dbo.Placements WITH (UPDLOCK, HOLDLOCK)
                WHERE MenuId = @MenuId AND VenueId = @VenueId);
            DECLARE @AlreadyOnMenu BIT=CASE WHEN EXISTS(SELECT 1 FROM dbo.Placements WHERE MenuId=@MenuId AND VenueId=@VenueId AND ItemId=@ItemId) THEN 1 ELSE 0 END;

            IF @Existing IS NOT NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT N'already_on_board' AS Outcome, @OnMenu AS ItemCountOnMenu, 0 AS SortOrder,
                       @Existing AS ExistingSectionId;
            END
            ELSE IF @AlreadyOnMenu=0 AND @OnMenu >= @ItemsPerMenuLimit
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

                INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
                SELECT @PlacementId, @VenueId, @MenuId, @SectionId, PageId, @ItemId, @Next, @Now, @Now
                FROM dbo.MenuSections WHERE Id=@SectionId AND VenueId=@VenueId;

                COMMIT TRANSACTION;
                SELECT N'placed' AS Outcome, @OnMenu + CASE WHEN @AlreadyOnMenu=1 THEN 0 ELSE 1 END AS ItemCountOnMenu, @Next AS SortOrder,
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
                SELECT CAST(a.ScreenId AS NVARCHAR(36)) AS screenId, a.PageId AS pageId
                FROM dbo.MenuScreenAssignments a
                WHERE a.MenuId = m.Id AND a.VenueId = @VenueId
                ORDER BY a.ScreenId
                FOR JSON PATH
            )) AS screens,
            JSON_QUERY((
                SELECT p.Id AS pageId, p.Name AS name, p.SortOrder AS sortOrder
                FROM dbo.MenuPages p
                WHERE p.MenuId = m.Id AND p.VenueId = @VenueId
                ORDER BY p.SortOrder, p.Id
                FOR JSON PATH
            )) AS pages,
            JSON_QUERY((
                SELECT s.Id AS sectionId, s.PageId AS pageId, s.Name AS name, s.SortOrder AS sortOrder,
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
                ORDER BY (SELECT p.SortOrder FROM dbo.MenuPages p WHERE p.Id=s.PageId), s.SortOrder, s.Id
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

            DECLARE @Pages TABLE (OldId UNIQUEIDENTIFIER, NewId UNIQUEIDENTIFIER);
            INSERT @Pages (OldId, NewId)
            SELECT p.Id, NEWID() FROM dbo.MenuPages p
            WHERE p.MenuId = @SourceMenuId AND p.VenueId = @VenueId;

            INSERT dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            SELECT map.NewId, @VenueId, @NewMenuId, p.Name, p.SortOrder, @Now, @Now
            FROM dbo.MenuPages p INNER JOIN @Pages map ON map.OldId = p.Id
            WHERE p.VenueId = @VenueId;

            DECLARE @Sections TABLE (OldId UNIQUEIDENTIFIER, NewId UNIQUEIDENTIFIER);

            INSERT @Sections (OldId, NewId)
            SELECT s.Id, NEWID()
            FROM dbo.MenuSections s
            WHERE s.MenuId = @SourceMenuId AND s.VenueId = @VenueId;

            INSERT dbo.MenuSections (Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            SELECT map.NewId, @VenueId, @NewMenuId, pageMap.NewId, s.Name, s.SortOrder, @Now, @Now
            FROM dbo.MenuSections s
            INNER JOIN @Sections map ON map.OldId = s.Id
            INNER JOIN @Pages pageMap ON pageMap.OldId = s.PageId
            WHERE s.VenueId = @VenueId;

            -- Same ItemId: the copy places the library's items, it does not clone them.
            INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
            SELECT NEWID(), @VenueId, @NewMenuId, map.NewId, pageMap.NewId, p.ItemId, p.SortOrder, @Now, @Now
            FROM dbo.Placements p
            INNER JOIN @Sections map ON map.OldId = p.MenuSectionId
            INNER JOIN @Pages pageMap ON pageMap.OldId = p.PageId
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

        DECLARE @Pages TABLE (PageId UNIQUEIDENTIFIER PRIMARY KEY, Name NVARCHAR(200), SortOrder INT);
        INSERT @Pages SELECT pageId,name,sortOrder FROM OPENJSON(@SnapshotJson,'$.pages')
          WITH (pageId UNIQUEIDENTIFIER '$.pageId',name NVARCHAR(200) '$.name',sortOrder INT '$.sortOrder');
        IF NOT EXISTS (SELECT 1 FROM @Pages)
          INSERT @Pages SELECT TOP (1) Id,Name,SortOrder FROM dbo.MenuPages WHERE VenueId=@VenueId AND MenuId=@MenuId ORDER BY SortOrder,Id;

        DECLARE @PageCount int=(SELECT COUNT(*) FROM dbo.MenuPages WHERE VenueId=@VenueId AND MenuId=@MenuId);
        UPDATE dbo.MenuPages SET SortOrder=SortOrder+@PageCount+100 WHERE VenueId=@VenueId AND MenuId=@MenuId;
        UPDATE p SET Name=src.Name,SortOrder=src.SortOrder,UpdatedUtc=@OccurredUtc FROM dbo.MenuPages p JOIN @Pages src ON src.PageId=p.Id WHERE p.VenueId=@VenueId AND p.MenuId=@MenuId;
        INSERT dbo.MenuPages (Id,VenueId,MenuId,Name,SortOrder,CreatedUtc,UpdatedUtc)
          SELECT src.PageId,@VenueId,@MenuId,src.Name,src.SortOrder,@OccurredUtc,@OccurredUtc FROM @Pages src
          WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuPages p WHERE p.Id=src.PageId);

        DECLARE @Sections TABLE (SectionId UNIQUEIDENTIFIER, PageId UNIQUEIDENTIFIER, Name NVARCHAR(200), SortOrder INT, Items NVARCHAR(MAX));
        INSERT @Sections (SectionId, PageId, Name, SortOrder, Items)
        SELECT sectionId, COALESCE(pageId,(SELECT TOP (1) PageId FROM @Pages ORDER BY SortOrder,PageId)), name, sortOrder, items
        FROM OPENJSON(@SnapshotJson, '$.sections')
        WITH (
            sectionId UNIQUEIDENTIFIER '$.sectionId',
            pageId UNIQUEIDENTIFIER '$.pageId',
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
            s.PageId = src.PageId,
            s.SortOrder = src.SortOrder,
            s.UpdatedUtc = @OccurredUtc
        FROM dbo.MenuSections s
        INNER JOIN @Sections src ON src.SectionId = s.Id
        WHERE s.VenueId = @VenueId AND s.MenuId = @MenuId;

        INSERT dbo.MenuSections (Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc)
        SELECT src.SectionId, @VenueId, @MenuId,
               src.PageId,
               src.Name, src.SortOrder, @OccurredUtc, @OccurredUtc
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

        -- One library item may be placed on more than one page. It remains unique
        -- inside a section, while its values still come from one item identity.
        WITH recorded AS
        (
            SELECT sec.SectionId, i.itemId, i.name, i.description, i.price, i.sortOrder
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
        SELECT SectionId, itemId, name, description, price, sortOrder FROM recorded;

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

        INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, PageId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
        SELECT NEWID(), @VenueId, @MenuId, src.SectionId, s.PageId, src.ItemId, src.SortOrder, @OccurredUtc, @OccurredUtc
        FROM @Items src INNER JOIN dbo.MenuSections s ON s.Id=src.SectionId AND s.VenueId=@VenueId
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.Placements p
            WHERE p.MenuSectionId = src.SectionId AND p.ItemId = src.ItemId);

        -- Which screens the menu is on is part of the shape, so a restore puts that
        -- back too -- including undoing a take-off that has not shipped yet.
        DECLARE @Screens TABLE (ScreenId UNIQUEIDENTIFIER, PageId UNIQUEIDENTIFIER);
        INSERT @Screens (ScreenId,PageId)
        SELECT screenId,COALESCE(pageId,(SELECT TOP (1) PageId FROM @Pages ORDER BY SortOrder,PageId)) FROM OPENJSON(@SnapshotJson, '$.screens')
        WITH (screenId UNIQUEIDENTIFIER '$.screenId',pageId UNIQUEIDENTIFIER '$.pageId');

        -- A stale restore must not silently turn a screen another menu acquired
        -- into a rotation. An existing exact pair is already shared deliberately
        -- and can be restored without touching the other menu; a missing pair
        -- requires a fresh assignment decision, so name the conflict instead.
        IF EXISTS (
            SELECT 1
            FROM @Screens desired
            INNER JOIN dbo.MenuScreenAssignments otherAssignment WITH (UPDLOCK, HOLDLOCK)
              ON otherAssignment.ScreenId=desired.ScreenId
             AND otherAssignment.VenueId=@VenueId
             AND otherAssignment.MenuId<>@MenuId
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.MenuScreenAssignments ownAssignment WITH (UPDLOCK, HOLDLOCK)
                WHERE ownAssignment.ScreenId=desired.ScreenId
                  AND ownAssignment.PageId=desired.PageId
                  AND ownAssignment.MenuId=@MenuId
                  AND ownAssignment.VenueId=@VenueId))
        BEGIN
            ROLLBACK TRANSACTION;
            THROW 51005, 'A screen from this version now belongs to another menu.', 1;
        END;

        DELETE a
        FROM dbo.MenuScreenAssignments a
        WHERE a.VenueId = @VenueId AND a.MenuId = @MenuId
          AND NOT EXISTS (SELECT 1 FROM @Screens s WHERE s.ScreenId = a.ScreenId AND s.PageId=a.PageId);

        UPDATE a SET AssignedUtc=@OccurredUtc,AssignedBy=@Author
        FROM dbo.MenuScreenAssignments a JOIN @Screens s ON s.ScreenId=a.ScreenId AND s.PageId=a.PageId
        WHERE a.VenueId=@VenueId AND a.MenuId=@MenuId;

        INSERT dbo.MenuScreenAssignments (Id, VenueId, ScreenId, MenuId, PageId, AssignedUtc, AssignedBy)
        SELECT NEWID(), @VenueId, s.ScreenId, @MenuId, s.PageId,
               @OccurredUtc, @Author
        FROM @Screens s
        INNER JOIN dbo.Screens sc ON sc.Id = s.ScreenId AND sc.VenueId = @VenueId
        WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuScreenAssignments a WHERE a.ScreenId = s.ScreenId AND a.PageId=s.PageId);

        DELETE p FROM dbo.MenuPages p WHERE p.VenueId=@VenueId AND p.MenuId=@MenuId
          AND NOT EXISTS (SELECT 1 FROM @Pages src WHERE src.PageId=p.Id);

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
        WHERE NOT EXISTS (
            SELECT 1 FROM dbo.MenuScreenAssignments ownAssignment WITH (UPDLOCK, HOLDLOCK)
            WHERE ownAssignment.ScreenId=p.ScreenId AND ownAssignment.MenuId=@MenuId)
          AND EXISTS (
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
            h.Id, h.VenueId, h.MenuId, h.PageId, h.PageName, h.Kind, h.PublishEventId, h.ReplacedByVersion,
            h.Detail, h.Author, h.OccurredUtc, e.Version
        FROM dbo.MenuHistoryEntries h
        LEFT JOIN dbo.MenuPublishEvents e
            ON e.Id = h.PublishEventId AND e.MenuId = h.MenuId AND e.VenueId = h.VenueId
        WHERE h.VenueId = @VenueId AND h.MenuId = @MenuId
        ORDER BY h.OccurredUtc DESC, h.Id DESC;
        """;

    private const string PageHistorySql = """
        SELECT TOP (@Limit)
            h.Id, h.VenueId, h.MenuId, h.PageId, h.PageName, h.Kind,
            h.PublishEventId, h.ReplacedByVersion, h.Detail, h.Author,
            h.OccurredUtc, CAST(NULL AS BIGINT) AS Version
        FROM dbo.MenuHistoryEntries h
        WHERE h.VenueId=@VenueId AND h.MenuId=@MenuId AND h.PageId=@PageId
          AND h.Kind<>N'published'
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

    public async Task<IReadOnlyCollection<MenuPage>> GetPagesAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuPage, object>(
            "SELECT Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc FROM dbo.MenuPages WHERE VenueId=@VenueId AND MenuId=@MenuId ORDER BY SortOrder, Id;",
            new { VenueId = venueId, MenuId = menuId }, cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<MenuPage?> CreatePageAsync(Guid venueId, Guid menuId, Guid pageId, string name, DateTime now, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuPage, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            IF NOT EXISTS (SELECT 1 FROM dbo.Menus WITH (UPDLOCK, HOLDLOCK) WHERE Id=@MenuId AND VenueId=@VenueId)
            BEGIN ROLLBACK; SELECT TOP (0) * FROM dbo.MenuPages; RETURN; END;
            DECLARE @SortOrder int = ISNULL((SELECT MAX(SortOrder) FROM dbo.MenuPages WITH (UPDLOCK, HOLDLOCK) WHERE MenuId=@MenuId AND VenueId=@VenueId), -1) + 1;
            INSERT dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
            OUTPUT inserted.* VALUES (@PageId, @VenueId, @MenuId, @Name, @SortOrder, @Now, @Now);
            COMMIT;
            """, new { VenueId = venueId, MenuId = menuId, PageId = pageId, Name = name, Now = now }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

    public async Task<bool> RenamePageAsync(Guid venueId, Guid menuId, Guid pageId, string name, DateTime now, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<PageUpdateRow, object>(
            "UPDATE dbo.MenuPages SET Name=@Name, UpdatedUtc=@Now OUTPUT 1 AS Value WHERE Id=@PageId AND MenuId=@MenuId AND VenueId=@VenueId;",
            new { VenueId = venueId, MenuId = menuId, PageId = pageId, Name = name, Now = now }, cancellationToken).ConfigureAwait(false)).Any();

    public async Task<ReorderOutcome> ReorderPagesGuardedAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> pageIds, DateTime now, CancellationToken cancellationToken = default)
    {
        var rows = await dataAccess.ExecuteSqlQueryAsync<string, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            DECLARE @Order TABLE (Id uniqueidentifier PRIMARY KEY, SortOrder int NOT NULL);
            INSERT @Order SELECT Id, SortOrder FROM OPENJSON(@OrderJson) WITH (Id uniqueidentifier '$.id', SortOrder int '$.sortOrder');
            IF (SELECT COUNT(*) FROM @Order) <> (SELECT COUNT(*) FROM dbo.MenuPages WITH (UPDLOCK, HOLDLOCK) WHERE VenueId=@VenueId AND MenuId=@MenuId)
               OR EXISTS (SELECT 1 FROM @Order o LEFT JOIN dbo.MenuPages p ON p.Id=o.Id AND p.VenueId=@VenueId AND p.MenuId=@MenuId WHERE p.Id IS NULL)
            BEGIN ROLLBACK; SELECT N'order_stale'; RETURN; END;
            DECLARE @Offset int = (SELECT COUNT(*) FROM @Order) + 1;
            UPDATE p SET SortOrder = SortOrder + @Offset, UpdatedUtc=@Now FROM dbo.MenuPages p WHERE p.VenueId=@VenueId AND p.MenuId=@MenuId;
            UPDATE p SET SortOrder = o.SortOrder, UpdatedUtc=@Now FROM dbo.MenuPages p JOIN @Order o ON o.Id=p.Id;
            COMMIT; SELECT N'reordered';
            """,
            new { VenueId = venueId, MenuId = menuId, OrderJson = System.Text.Json.JsonSerializer.Serialize(pageIds.Select((id, index) => new { id, sortOrder = index })), Now = now }, cancellationToken).ConfigureAwait(false);
        return new ReorderOutcome(rows.FirstOrDefault() ?? "order_stale", 0);
    }

    public async Task<MenuPage?> DuplicatePageAsync(Guid venueId, Guid menuId, Guid sourcePageId, Guid newPageId, DateTime now, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuPage, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            DECLARE @SourceOrder int, @SourceName nvarchar(200);
            SELECT @SourceOrder=SortOrder, @SourceName=Name FROM dbo.MenuPages WITH (UPDLOCK, HOLDLOCK) WHERE Id=@SourcePageId AND MenuId=@MenuId AND VenueId=@VenueId;
            IF @SourceOrder IS NULL BEGIN ROLLBACK; SELECT TOP (0) * FROM dbo.MenuPages; RETURN; END;
            DECLARE @PageCount int=(SELECT COUNT(*) FROM dbo.MenuPages WHERE MenuId=@MenuId AND VenueId=@VenueId);
            UPDATE dbo.MenuPages SET SortOrder=SortOrder+@PageCount+1 WHERE MenuId=@MenuId AND VenueId=@VenueId AND SortOrder>@SourceOrder;
            UPDATE dbo.MenuPages SET SortOrder=SortOrder-@PageCount WHERE MenuId=@MenuId AND VenueId=@VenueId AND SortOrder>@SourceOrder+@PageCount+1;
            INSERT dbo.MenuPages (Id,VenueId,MenuId,Name,SortOrder,CreatedUtc,UpdatedUtc) VALUES (@NewPageId,@VenueId,@MenuId,CONCAT(@SourceName,N' copy'),@SourceOrder+1,@Now,@Now);
            DECLARE @Sections TABLE (OldId uniqueidentifier PRIMARY KEY, NewId uniqueidentifier NOT NULL);
            INSERT @Sections SELECT Id, NEWID() FROM dbo.MenuSections WHERE PageId=@SourcePageId AND VenueId=@VenueId;
            INSERT dbo.MenuSections (Id,VenueId,MenuId,PageId,Name,SortOrder,CreatedUtc,UpdatedUtc)
              SELECT x.NewId,@VenueId,@MenuId,@NewPageId,s.Name,s.SortOrder,@Now,@Now FROM dbo.MenuSections s JOIN @Sections x ON x.OldId=s.Id;
            INSERT dbo.Placements (Id,VenueId,MenuId,MenuSectionId,PageId,ItemId,SortOrder,CreatedUtc,UpdatedUtc)
              SELECT NEWID(),@VenueId,@MenuId,x.NewId,@NewPageId,p.ItemId,p.SortOrder,@Now,@Now FROM dbo.Placements p JOIN @Sections x ON x.OldId=p.MenuSectionId;
            COMMIT;
            SELECT Id,VenueId,MenuId,Name,SortOrder,CreatedUtc,UpdatedUtc FROM dbo.MenuPages WHERE Id=@NewPageId;
            """, new { VenueId = venueId, MenuId = menuId, SourcePageId = sourcePageId, NewPageId = newPageId, Now = now }, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

    public async Task<PageDeleteOutcome> DeletePageAsync(Guid venueId, Guid menuId, Guid pageId, Guid? moveSectionsToPageId, bool deleteSections = false, CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<PageDeleteRow, object>(
            """
            SET XACT_ABORT ON; BEGIN TRANSACTION;
            IF (SELECT COUNT(*) FROM dbo.MenuPages WITH (UPDLOCK,HOLDLOCK) WHERE VenueId=@VenueId AND MenuId=@MenuId) <= 1
            BEGIN ROLLBACK; SELECT N'last_page' Outcome,0 AffectedSectionCount,0 RemovedAssignmentCount; RETURN; END;
            IF NOT EXISTS (SELECT 1 FROM dbo.MenuPages WHERE Id=@PageId AND VenueId=@VenueId AND MenuId=@MenuId)
            BEGIN ROLLBACK; SELECT N'page_missing' Outcome,0 AffectedSectionCount,0 RemovedAssignmentCount; RETURN; END;
            DECLARE @SectionCount int=(SELECT COUNT(*) FROM dbo.MenuSections WHERE PageId=@PageId AND VenueId=@VenueId);
            IF @SectionCount>0 AND @DeleteSections=0 AND (@MoveSectionsToPageId IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.MenuPages WHERE Id=@MoveSectionsToPageId AND VenueId=@VenueId AND MenuId=@MenuId AND Id<>@PageId))
            BEGIN ROLLBACK; SELECT N'move_required' Outcome,0 AffectedSectionCount,0 RemovedAssignmentCount; RETURN; END;
            IF @SectionCount>0 AND @DeleteSections=0 AND EXISTS (
              SELECT 1 FROM dbo.Placements sourcePlacement
              INNER JOIN dbo.MenuSections sourceSection ON sourceSection.Id=sourcePlacement.MenuSectionId AND sourceSection.PageId=@PageId
              INNER JOIN dbo.Placements destinationPlacement ON destinationPlacement.PageId=@MoveSectionsToPageId AND destinationPlacement.ItemId=sourcePlacement.ItemId
              WHERE sourcePlacement.VenueId=@VenueId)
            BEGIN ROLLBACK; SELECT N'item_conflict' Outcome,0 AffectedSectionCount,0 RemovedAssignmentCount; RETURN; END;
            IF @SectionCount>0 AND @DeleteSections=0 BEGIN
              DECLARE @Offset int=ISNULL((SELECT MAX(SortOrder)+1 FROM dbo.MenuSections WHERE PageId=@MoveSectionsToPageId),0);
              UPDATE dbo.MenuSections SET PageId=@MoveSectionsToPageId, SortOrder=@Offset+SortOrder, UpdatedUtc=SYSUTCDATETIME() WHERE PageId=@PageId AND VenueId=@VenueId;
            END;
            IF @SectionCount>0 AND @DeleteSections=1 BEGIN
              DELETE dbo.Placements WHERE PageId=@PageId AND VenueId=@VenueId;
              DELETE dbo.MenuSections WHERE PageId=@PageId AND VenueId=@VenueId;
            END;
            DELETE dbo.MenuScreenAssignments WHERE PageId=@PageId AND VenueId=@VenueId; DECLARE @Assignments int=@@ROWCOUNT;
            DECLARE @OldOrder int=(SELECT SortOrder FROM dbo.MenuPages WHERE Id=@PageId);
            DELETE dbo.MenuPages WHERE Id=@PageId AND VenueId=@VenueId AND MenuId=@MenuId;
            DECLARE @PageCount int=(SELECT COUNT(*) FROM dbo.MenuPages WHERE VenueId=@VenueId AND MenuId=@MenuId);
            UPDATE dbo.MenuPages SET SortOrder=SortOrder+@PageCount+1 WHERE VenueId=@VenueId AND MenuId=@MenuId AND SortOrder>@OldOrder;
            UPDATE dbo.MenuPages SET SortOrder=SortOrder-@PageCount-2 WHERE VenueId=@VenueId AND MenuId=@MenuId AND SortOrder>@OldOrder+@PageCount+1;
            COMMIT; SELECT N'deleted' Outcome,@SectionCount AffectedSectionCount,@Assignments RemovedAssignmentCount;
            """, new { VenueId = venueId, MenuId = menuId, PageId = pageId, MoveSectionsToPageId = moveSectionsToPageId, DeleteSections = deleteSections }, cancellationToken).ConfigureAwait(false)).Single();
        return new PageDeleteOutcome(row.Outcome, row.AffectedSectionCount, row.RemovedAssignmentCount);
    }

    private sealed record PageDeleteRow(string Outcome, int AffectedSectionCount, int RemovedAssignmentCount);
    private sealed record PageUpdateRow(int Value);

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
                PageId = Guid.NewGuid(),
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
        Guid? pageId = null,
        string? author = null,
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
                Now = now,
                RequestedPageId = pageId,
                Author = author
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
        string? author = null,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            RenameSectionSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                Name = name,
                Now = now,
                Author = author
            },
            cancellationToken).ConfigureAwait(false)).Any();

    public async Task<SectionDeleteOutcome> DeleteSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid? moveItemsToSectionId,
        bool deletePlacements,
        string? author = null,
        DateTime? now = null,
        CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<SectionDeleteRow, object>(
            DeleteSectionSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                SectionId = RequireId(sectionId, nameof(sectionId)),
                MoveItemsToSectionId = moveItemsToSectionId,
                DeletePlacements = deletePlacements,
                Author = author,
                Now = now ?? DateTime.UtcNow
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new SectionDeleteOutcome(row.Outcome, row.Moved, row.Released);
    }

    public async Task<ReorderOutcome> ReorderSectionsGuardedAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        DateTime now,
        string? author = null,
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
                Now = now,
                Author = author
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new ReorderOutcome(row.Outcome, row.Moved);
    }

    public async Task<ItemUpdateOutcome> UpdateItemValuesGuardedAsync(
        Guid venueId,
        Guid itemId,
        string name,
        string? description,
        string? price,
        ItemValueExpectation? expected,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var row = (await dataAccess.ExecuteSqlQueryAsync<ItemUpdateRow, object>(
            UpdateItemValuesGuardedSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                ItemId = RequireId(itemId, nameof(itemId)),
                NewName = name,
                NewDescription = description,
                NewPrice = price,
                Guarded = expected is null ? 0 : 1,
                ExpectedName = expected?.Name,
                ExpectedDescription = expected?.Description,
                ExpectedPrice = expected?.Price,
                Now = now
            },
            cancellationToken).ConfigureAwait(false)).Single();

        return new ItemUpdateOutcome(row.Outcome, row.Name, row.Description, row.Price);
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
                    PageId = RequireId(assignment.PageId, nameof(assignment.PageId)),
                    assignment.Rotate,
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

    public async Task<bool> ClearPageScreenAssignmentAsync(Guid venueId, Guid screenId, Guid menuId, Guid pageId, CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
            ClearPageScreenAssignmentSql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                ScreenId = RequireId(screenId, nameof(screenId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                PageId = RequireId(pageId, nameof(pageId))
            },
            cancellationToken).ConfigureAwait(false)).Any();

    public async Task ApplyPageScreenAssignmentsAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<PageScreenAssignmentChange> changes,
        string? author,
        DateTime occurredUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);
        var normalized = changes.ToArray();
        try
        {
            await dataAccess.ExecuteSqlQueryAsync<ScalarResult, object>(
                ApplyPageScreenAssignmentsSql,
                new
                {
                    VenueId = RequireId(venueId, nameof(venueId)),
                    MenuId = RequireId(menuId, nameof(menuId)),
                    ChangesJson = System.Text.Json.JsonSerializer.Serialize(normalized.Select(change => new { screenId = change.ScreenId, pageId = change.PageId, mode = change.Mode })),
                    ExpectedCount = normalized.Length,
                    Author = author,
                    OccurredUtc = occurredUtc == default ? DateTime.UtcNow : occurredUtc
                },
                cancellationToken).ConfigureAwait(false);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51008)
        {
            throw new InvalidOperationException(exception.Message, exception);
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 51006)
        {
            throw new MenuPutAwayException(exception.Message);
        }
    }

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

    public async Task<IReadOnlyCollection<MenuHistoryEntry>> GetPageHistoryAsync(
        Guid venueId,
        Guid menuId,
        Guid pageId,
        int limit,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<MenuHistoryEntry, object>(
            PageHistorySql,
            new
            {
                VenueId = RequireId(venueId, nameof(venueId)),
                MenuId = RequireId(menuId, nameof(menuId)),
                PageId = RequireId(pageId, nameof(pageId)),
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

    public async Task ResetAutomationVenueAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;
            BEGIN TRY
                DECLARE @Screens TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY);
                INSERT @Screens (Id) SELECT Id FROM dbo.Screens WITH (UPDLOCK, HOLDLOCK) WHERE VenueId = @VenueId;

                UPDATE dbo.CustomerOnboardingStates
                SET FirstScreenId = NULL, UpdatedUtc = SYSUTCDATETIME()
                WHERE FirstScreenId IN (SELECT Id FROM @Screens);
                DELETE FROM dbo.ScreenReplacementAudits
                WHERE TargetScreenId IN (SELECT Id FROM @Screens) OR SourceScreenId IN (SELECT Id FROM @Screens);
                DELETE FROM dbo.ScreenContentDeliveries WHERE ScreenId IN (SELECT Id FROM @Screens);
                DELETE FROM dbo.ScreenPairingCodes WHERE ScreenId IN (SELECT Id FROM @Screens);
                DELETE FROM dbo.PlaylistSlides WHERE VenueId = @VenueId;
                DELETE FROM dbo.EmergencyBroadcasts WHERE VenueId = @VenueId;

                DELETE FROM dbo.MenuPublishTargets WHERE VenueId = @VenueId;
                DELETE FROM dbo.MenuHistoryEntries WHERE VenueId = @VenueId;
                DELETE FROM dbo.MenuPublishEvents WHERE VenueId = @VenueId;
                DELETE FROM dbo.MenuScreenAssignments WHERE VenueId = @VenueId;
                DELETE FROM dbo.Placements WHERE VenueId = @VenueId;
                DELETE FROM dbo.ItemAvailability WHERE VenueId = @VenueId;
                DELETE FROM dbo.Items WHERE VenueId = @VenueId;
                DELETE FROM dbo.MenuSections WHERE VenueId = @VenueId;
                DELETE FROM dbo.MenuPages WHERE VenueId = @VenueId;
                DELETE FROM dbo.Menus WHERE VenueId = @VenueId;
                DELETE FROM dbo.Screens WHERE VenueId = @VenueId;
                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
                THROW;
            END CATCH;
            SELECT 1 AS Value;
            """;
        _ = await dataAccess.ExecuteSqlQueryAsync<ResetRow, object>(sql, new { VenueId = venueId }, cancellationToken)
            .ConfigureAwait(false);
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

    private sealed class ResetRow
    {
        public int Value { get; set; }
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

        public int Moved { get; set; }

        public int Released { get; set; }
    }

    private sealed class ReorderRow
    {
        public string Outcome { get; set; } = string.Empty;

        public int Moved { get; set; }
    }

    private sealed class ItemUpdateRow
    {
        public string Outcome { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Price { get; set; }
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
