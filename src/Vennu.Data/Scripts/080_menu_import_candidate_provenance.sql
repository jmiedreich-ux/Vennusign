-- A21: two candidates that look the same are two candidates.
--
-- A venue library can hold the same dish twice at the same price, split by an older import. The
-- review screen offered both as "Use the one you already have - Pad Thai $12.95", twice, and there
-- was nothing on the screen to choose between them. An operator cannot answer that question; they
-- can only guess at it.
--
-- The owner ruled on 2026-08-27 against merging them silently. The screen names what makes them
-- different - which menus each one is on, and when it was made - and the operator decides. A
-- duplicate the venue can see is a duplicate the venue can deal with; one the product quietly
-- resolves is one nobody ever finds out about.
--
-- Both columns are a SNAPSHOT of what the operator was shown, like DisplayName and DisplayPrice
-- beside them. A menu renamed after the review does not rewrite the question that was asked.
--
-- Nullable because they are only filled where there is an ambiguity to resolve. A question with
-- one candidate has nothing to distinguish it from, and paying for that read on every import to
-- store a value no screen draws would be work for its own sake.
--
-- WHAT THIS DISCARDS (per AGENTS.md): nothing. Two nullable columns on a table whose rows live at
-- most as long as an import session.
ALTER TABLE dbo.MenuImportCandidates ADD
    OnMenusJson NVARCHAR(1000) NULL,
    ItemCreatedUtc DATETIME2(7) NULL;
GO
