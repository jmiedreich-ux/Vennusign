/*
    Menus M3-A slice 3: page-attributed item placement history.

    WHAT THIS DISCARDS: nothing. This widens the supported history vocabulary;
    existing rows and retention behavior are unchanged.
*/

IF OBJECT_ID(N'dbo.CK_MenuHistoryEntries_Kind', N'C') IS NOT NULL
    ALTER TABLE dbo.MenuHistoryEntries DROP CONSTRAINT CK_MenuHistoryEntries_Kind;
ALTER TABLE dbo.MenuHistoryEntries ADD CONSTRAINT CK_MenuHistoryEntries_Kind CHECK
(
    Kind IN
    (
        N'published', N'draft_discarded', N'put_away', N'put_back',
        N'taken_off_screens', N'restored', N'assigned', N'duplicated',
        N'section_added', N'section_renamed', N'sections_reordered', N'section_deleted',
        N'item_added', N'items_reordered', N'item_moved', N'item_removed'
    )
);
GO
