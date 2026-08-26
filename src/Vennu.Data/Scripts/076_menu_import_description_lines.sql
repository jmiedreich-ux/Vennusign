-- M6.7: a pasted line can be an item's description.
--
-- Q81 settled on 2026-08-07 that an unpriced, non-heading line under an item is that item's
-- description. It was never implemented, so on a real printed menu every description line came
-- back `unresolved` / `item_format_not_recognized` - the largest single source of the ninety-one
-- questions one four-page menu produced.
--
-- The parser now attaches the text to the item above it AND records the line itself as what it is.
-- Discards nothing: every prior row keeps its disposition, and no existing value changes meaning.
-- `item` and `section` consumers filter on those literals, so a `description` row is invisible to
-- menu creation and replacement by construction.
ALTER TABLE dbo.MenuImportSourceLines DROP CONSTRAINT CK_MenuImportSourceLines_Disposition;
GO

ALTER TABLE dbo.MenuImportSourceLines ADD CONSTRAINT CK_MenuImportSourceLines_Disposition
    CHECK (Disposition IN (N'blank', N'section', N'item', N'unresolved', N'fallback', N'description'));
GO
