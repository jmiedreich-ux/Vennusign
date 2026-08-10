/*
    Menus M3 - the two rules the builder is about to lean on, enforced where they
    are enforceable instead of only in a component.

    ------------------------------------------------------------------------
    1. A section is deleted, not archived.
    ------------------------------------------------------------------------

    Q96: deleting a section releases its items back to the library - nothing is
    lost, because the items were never IN the section; a placement put them
    there. dbo.MenuSections.IsActive was the old editor's archive flag, and
    "archive" is one of the four words criterion 5 bans from this area outright.
    After M3 nothing writes it.

    Leaving the column would be worse than dropping it. Three reads filter on
    IsActive = 1 (the working snapshot, the display board, screen targeting).
    A column that only ever holds 1, guarded by filters nobody has a reason to
    think about, is a loaded gun: the first future writer of 0 silently removes
    a section from live boards, and the filters make that look like correct
    behaviour rather than a bug.

    WHAT THIS DISCARDS (per AGENTS.md: a migration that discards data names it):

    - Every dbo.MenuSections row with IsActive = 0, and every dbo.Placements row
      on such a section. These sections are already invisible: all three readers
      filter them out, so no board, no snapshot and no screen has rendered one.
      The ITEMS survive - only the placements go, which is exactly what Q96 says
      deleting a section does.

    - The IsActive column itself, with its default constraint.

    The items released this way are not orphaned in any meaningful sense: an item
    with no placement is a library item, which is the normal resting state for
    anything not currently on a board.

    ------------------------------------------------------------------------
    2. An item appears at most once on a board.
    ------------------------------------------------------------------------

    Q112: an item already on this board is labelled as such, and picking it JUMPS
    to it instead of placing a second copy. The schema enforced that once per
    SECTION (UQ_Placements_SectionItem), which is a weaker rule than the product
    promises - the same item on two sections of one menu is legal today, and it
    would render twice on the same board.

    UQ_Placements_MenuItem (MenuId, ItemId) is the rule the product actually
    states. It strictly implies the old constraint, because
    FK_Placements_SectionOnMenu already guarantees a placement's section belongs
    to its menu, so the old one is replaced rather than kept alongside.

    WHAT THIS DISCARDS: duplicate placements of one item on one menu, keeping the
    first by section order, then placement order, then id - the copy a guest
    would read first on the board. No item row is touched.
*/

-------------------------------------------------------------------------------
-- 1. Archived sections
-------------------------------------------------------------------------------

DELETE p
FROM dbo.Placements p
INNER JOIN dbo.MenuSections s ON s.Id = p.MenuSectionId AND s.VenueId = p.VenueId
WHERE s.IsActive = 0;
GO

DELETE FROM dbo.MenuSections WHERE IsActive = 0;
GO

-- Generated names differ between a baseline-built and a chain-built database, so
-- the constraint is looked up rather than named (the 059 lesson).
DECLARE @SectionDefault SYSNAME =
(
    SELECT dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.object_id = dc.parent_object_id
       AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.MenuSections', N'U')
      AND c.name = N'IsActive'
);

IF @SectionDefault IS NOT NULL
BEGIN
    EXEC (N'ALTER TABLE dbo.MenuSections DROP CONSTRAINT ' + @SectionDefault + N';');
END;
GO

ALTER TABLE dbo.MenuSections DROP COLUMN IsActive;
GO

-------------------------------------------------------------------------------
-- 2. One placement per item per board
-------------------------------------------------------------------------------

WITH ranked AS
(
    SELECT p.Id,
           ROW_NUMBER() OVER (
               PARTITION BY p.MenuId, p.ItemId
               ORDER BY s.SortOrder, p.SortOrder, p.Id) AS Rank
    FROM dbo.Placements p
    INNER JOIN dbo.MenuSections s ON s.Id = p.MenuSectionId AND s.VenueId = p.VenueId
)
DELETE FROM dbo.Placements
WHERE Id IN (SELECT Id FROM ranked WHERE Rank > 1);
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = N'UQ_Placements_SectionItem')
BEGIN
    ALTER TABLE dbo.Placements DROP CONSTRAINT UQ_Placements_SectionItem;
END;
GO

ALTER TABLE dbo.Placements
    ADD CONSTRAINT UQ_Placements_MenuItem UNIQUE (MenuId, ItemId);
GO
