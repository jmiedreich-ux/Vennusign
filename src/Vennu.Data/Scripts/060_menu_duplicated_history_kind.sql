/*
    Menus M2 - "duplicated" joins the kinds a menu's history can record.

    Duplicate is one of the six named card actions (Q195, build-decision 16). The
    copy it makes is a menu that has never been published and is on no screen, and
    without a history entry there is nothing anywhere that says where it came from.
    "Duplicated from 'Summer Menu'" is not derivable from any other column, which
    is what earns it a row; plain creation stays unrecorded because CreatedUtc
    already says everything there is to say about it.

    Discards nothing: this widens a CHECK constraint and rewrites no data.
*/

ALTER TABLE dbo.MenuHistoryEntries DROP CONSTRAINT CK_MenuHistoryEntries_Kind;
GO

ALTER TABLE dbo.MenuHistoryEntries ADD CONSTRAINT CK_MenuHistoryEntries_Kind
    CHECK (Kind IN (
        N'published', N'draft_discarded', N'put_away', N'put_back',
        N'taken_off_screens', N'restored', N'assigned', N'duplicated'));
GO
