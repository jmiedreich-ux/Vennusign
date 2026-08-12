/*
    Menus M3-A slice 2: retain the page identity on history entries so the
    builder can show only the selected page's events while the full menu
    timeline remains durable.

    WHAT THIS DISCARDS: nothing. Existing menu-level history remains menu-level
    with PageId and PageName NULL. PageName is an immutable display snapshot so
    page deletion or later renaming cannot make an old event misleading.

    PageId is deliberately not a foreign key. Menu pages are hard-deleted, while
    history is durable; a foreign key would either erase the audit record or lose
    the page attribution. Every product write resolves PageId through the
    venue/menu-owned section in the same transaction that writes the event.
*/

IF COL_LENGTH(N'dbo.MenuHistoryEntries', N'PageId') IS NULL
    ALTER TABLE dbo.MenuHistoryEntries ADD PageId UNIQUEIDENTIFIER NULL;
IF COL_LENGTH(N'dbo.MenuHistoryEntries', N'PageName') IS NULL
    ALTER TABLE dbo.MenuHistoryEntries ADD PageName NVARCHAR(200) NULL;
GO

IF OBJECT_ID(N'dbo.CK_MenuHistoryEntries_Kind', N'C') IS NOT NULL
    ALTER TABLE dbo.MenuHistoryEntries DROP CONSTRAINT CK_MenuHistoryEntries_Kind;
ALTER TABLE dbo.MenuHistoryEntries ADD CONSTRAINT CK_MenuHistoryEntries_Kind CHECK
(
    Kind IN
    (
        N'published', N'draft_discarded', N'put_away', N'put_back',
        N'taken_off_screens', N'restored', N'assigned', N'duplicated',
        N'section_added', N'section_renamed', N'sections_reordered', N'section_deleted'
    )
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.MenuHistoryEntries') AND name=N'IX_MenuHistoryEntries_PageTimeline')
    CREATE INDEX IX_MenuHistoryEntries_PageTimeline
        ON dbo.MenuHistoryEntries (VenueId, MenuId, PageId, OccurredUtc DESC, Id DESC)
        INCLUDE (Kind, Detail, Author, PageName);
GO
