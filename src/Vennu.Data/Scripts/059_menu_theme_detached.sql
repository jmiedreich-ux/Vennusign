/*
    Menus M2 - a menu's theme becomes an honest attachment, or nothing at all.

    dbo.Menus.Theme was created NOT NULL DEFAULT N'coastal': free text, no table
    behind it, naming a look that was never built. Owner decision (2026-08-09,
    Q86): menu themes and shell themes are categorically different things; menu
    themes are created later in the theme editor and attached to a menu; and a
    menu with NO theme attached is a valid state that the render engine draws
    plainly. So the column was wrong twice over - it forbade the valid unthemed
    state, and it defaulted to a fiction.

    The MenuThemes table is deliberately NOT created here. It arrives with the
    first milestone that reads one, so its shape is designed when its real user
    exists rather than guessed now - which is how dbo.VenueThemes came to hold
    board-render fields under a venue name.

    WHAT THIS DISCARDS (per AGENTS.md: a migration that discards data names it):

    - Every dbo.Menus.Theme value equal to N'coastal' becomes NULL. No theme of
      that name was ever built, and no endpoint ever wrote the column, so every
      such row holds the default rather than a choice anyone made. A value that
      is not N'coastal' is left alone: that would be a real choice.

    - The same string is removed from the theme property of stored publish
      snapshots. Without this, the derived draft would compare a published
      'coastal' against a working NULL and report "theme changed" on every
      published menu in the system - a change nobody made, on a count whose
      whole promise is that it cannot disagree with what a publish will ship
      (Q182). The snapshots keep every other value they recorded.

    FOR JSON PATH omits a NULL column, and JSON_MODIFY with a NULL value deletes
    the key in lax mode, so both sides land on the same shape: an unthemed menu
    has no theme property at all, and MenuSnapshot.Theme parses as null.
*/

-- The default constraint is named in the baseline, but a database built from the
-- superseded chain can carry a different generated name for the same thing. Look
-- it up rather than trusting either name.
DECLARE @DefaultConstraint SYSNAME =
(
    SELECT dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.object_id = dc.parent_object_id
       AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.Menus', N'U')
      AND c.name = N'Theme'
);

IF @DefaultConstraint IS NOT NULL
BEGIN
    EXEC (N'ALTER TABLE dbo.Menus DROP CONSTRAINT ' + @DefaultConstraint + N';');
END;
GO

ALTER TABLE dbo.Menus ALTER COLUMN Theme NVARCHAR(40) NULL;
GO

UPDATE dbo.Menus
SET Theme = NULL
WHERE Theme = N'coastal';
GO

UPDATE dbo.MenuPublishEvents
SET Snapshot = JSON_MODIFY(Snapshot, '$.theme', NULL)
WHERE ISJSON(Snapshot) = 1
  AND JSON_VALUE(Snapshot, '$.theme') = N'coastal';
GO
