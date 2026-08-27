-- M6.10: a menu can carry a description, and a pasted line can be left out.
--
-- Two things the review screen needs and the model did not have.
--
-- `Menus.Description` is back-office only for now, by owner decision (2026-08-26): it is what an
-- operator calls this menu to themselves and their staff. Whether it belongs on a guest-facing
-- board is a separate design question, deliberately unanswered - the column exists so the answer
-- has somewhere to land without a second migration.
--
-- `leave_out` is the third answer the design always specified for an unreadable line - "An item /
-- A section / Leave it out" (M1a, S6A-Q07) - and only two were built. Without it the only way past
-- a line the parser could not read was to import it as an item and delete it afterwards. Nothing
-- else changes: menu creation pulls unresolved lines only where the answer is `fallback`, so a
-- line answered `leave_out` is simply never placed, while its text stays on the session.
ALTER TABLE dbo.Menus ADD Description NVARCHAR(500) NULL;
GO

ALTER TABLE dbo.MenuImportAnswers DROP CONSTRAINT CK_MenuImportAnswers_Choice;
GO
ALTER TABLE dbo.MenuImportAnswers ADD CONSTRAINT CK_MenuImportAnswers_Choice
    CHECK (Choice IN (N'same_item', N'new_item', N'section', N'fallback', N'leave_out'));
GO
