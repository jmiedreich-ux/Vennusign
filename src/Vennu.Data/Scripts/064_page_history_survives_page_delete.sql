/*
    Menus M3-A slice 2 convergence: page history keeps immutable attribution
    after a page is deleted.

    WHAT THIS DISCARDS: no rows or values. This removes only a live foreign-key
    relationship that made durable history prevent page deletion. PageId and
    PageName remain on every existing history row as audit snapshots.
*/

IF OBJECT_ID(N'dbo.FK_MenuHistoryEntries_PageOnMenu', N'F') IS NOT NULL
    ALTER TABLE dbo.MenuHistoryEntries DROP CONSTRAINT FK_MenuHistoryEntries_PageOnMenu;
IF OBJECT_ID(N'dbo.FK_MenuHistoryEntries_Pages', N'F') IS NOT NULL
    ALTER TABLE dbo.MenuHistoryEntries DROP CONSTRAINT FK_MenuHistoryEntries_Pages;
GO
