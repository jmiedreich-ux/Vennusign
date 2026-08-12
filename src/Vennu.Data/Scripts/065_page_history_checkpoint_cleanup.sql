/*
    Menus M3-A slice 2 local-checkpoint convergence.

    WHAT THIS DISCARDS: no rows or values. This removes only the redundant
    pre-review page-history index name that may exist in a database upgraded by
    an earlier local checkpoint. IX_MenuHistoryEntries_PageTimeline remains the
    supported page-history index created by migration 063.
*/

IF EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.MenuHistoryEntries')
      AND name = N'IX_MenuHistoryEntries_Page_Time'
)
    DROP INDEX IX_MenuHistoryEntries_Page_Time ON dbo.MenuHistoryEntries;
GO
