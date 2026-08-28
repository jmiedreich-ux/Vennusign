-- A venue's library holds a dish once. Owner ruling, 2026-08-28: "duplicates should not be
-- allowed in the library."
--
-- This settles A21 and A22 by removing the situation they argued about. A21 gave two identical
-- candidates a provenance line so an operator could tell them apart; A22 stopped asking when that
-- line came out identical. Neither is needed if the library cannot hold the dish twice.
--
-- WHERE THEY CAME FROM: the import created a new item per pasted line, so a menu naming a dish
-- twice minted it twice. The parser fix (#938) stops NEW ones. This is the other half - the ones
-- already there, and the rule that keeps them from coming back.
--
-- WHAT THIS DISCARDS (per AGENTS.md), stated exactly:
--
--   * Duplicate dbo.Items rows are DELETED. For each (VenueId, canonical name) the OLDEST row
--     survives - the original a later import split from - and the rest merge into it. Their names,
--     descriptions and prices are discarded and the survivor's kept. Nothing an operator can
--     distinguish is lost, because canonical equality is exactly what made them indistinguishable.
--   * PLACEMENTS collapse to ONE per page. "An item appears at most once on a page" is a model
--     invariant (M3-A, migration 062) that the owner reaffirmed on 2026-08-28, so where a merge
--     would put the survivor on a page twice the extra placements are deleted. The survivor's own
--     placement is preferred; otherwise the earliest duplicate's is kept, and it carries its
--     ImportedPriceOverride, so a per-menu price (A19) survives the merge.
--   * MenuImportCreatedLines rows pointing at a deleted placement go with it. They record what a
--     finished import created; the placement they name no longer exists.
--   * ItemAvailability rows for a duplicate are dropped where the survivor already has one, and
--     repointed otherwise. An 86 is a fact about a dish, and the survivor is that dish.
--   * MenuImportCandidates rows naming a duplicate are deleted. They are review-screen snapshots
--     inside a live import session, they expire on their own, and a candidate offering a row that
--     no longer exists is worse than one question re-asked.
--   * Published snapshot JSON is NOT touched. What was published stays exactly as published.
--
-- Inactive items are excluded: IsActive=0 is the product's tombstone, and a reinstated name should
-- be refused at that point rather than blocking this migration.

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- One survivor per venue and canonical name: the oldest, then by id so it is deterministic.
CREATE TABLE #Merge (Loser UNIQUEIDENTIFIER PRIMARY KEY, Survivor UNIQUEIDENTIFIER NOT NULL, VenueId UNIQUEIDENTIFIER NOT NULL);

INSERT #Merge (Loser, Survivor, VenueId)
SELECT ranked.Id, keep.Id, ranked.VenueId
FROM (
    SELECT i.Id, i.VenueId, dbo.CanonicalItemName(i.Name) AS Canonical,
           ROW_NUMBER() OVER (PARTITION BY i.VenueId, dbo.CanonicalItemName(i.Name)
                              ORDER BY i.CreatedUtc, i.Id) AS Position
    FROM dbo.Items i WHERE i.IsActive = 1
) ranked
JOIN (
    SELECT i.Id, i.VenueId, dbo.CanonicalItemName(i.Name) AS Canonical,
           ROW_NUMBER() OVER (PARTITION BY i.VenueId, dbo.CanonicalItemName(i.Name)
                              ORDER BY i.CreatedUtc, i.Id) AS Position
    FROM dbo.Items i WHERE i.IsActive = 1
) keep ON keep.VenueId = ranked.VenueId AND keep.Canonical = ranked.Canonical AND keep.Position = 1
WHERE ranked.Position > 1;

/*
    Exactly one placement survives per (page, survivor).

    Checking each duplicate against the SURVIVOR alone is not enough: two duplicates of the same
    dish can sit on one page with the survivor nowhere near it, and repointing both then collides
    them with each other rather than with the survivor. UQ_Placements_PageItem catches it, the
    migration rolls back, and the reason is not obvious from the error.

    So every placement that will end up naming a survivor is ranked together - the survivor's own
    first, then the earliest duplicate - and only the winner is kept.
*/
CREATE TABLE #KeepPlacement (PlacementId UNIQUEIDENTIFIER PRIMARY KEY);

INSERT #KeepPlacement (PlacementId)
SELECT PlacementId FROM (
    SELECT p.Id AS PlacementId,
           ROW_NUMBER() OVER (
               PARTITION BY p.PageId, COALESCE(m.Survivor, p.ItemId)
               ORDER BY CASE WHEN m.Loser IS NULL THEN 0 ELSE 1 END, p.CreatedUtc, p.Id) AS Position
    FROM dbo.Placements p
    LEFT JOIN #Merge m ON m.Loser = p.ItemId
    WHERE m.Loser IS NOT NULL
       OR EXISTS (SELECT 1 FROM #Merge survivors WHERE survivors.Survivor = p.ItemId)
) ranked
WHERE ranked.Position = 1;

-- The created-lines rows name placements, so they go before the placements do.
DELETE createdLine
FROM dbo.MenuImportCreatedLines createdLine
JOIN dbo.Placements p ON p.Id = createdLine.PlacementId
JOIN #Merge m ON m.Loser = p.ItemId
WHERE NOT EXISTS (SELECT 1 FROM #KeepPlacement k WHERE k.PlacementId = p.Id);

DELETE p
FROM dbo.Placements p
JOIN #Merge m ON m.Loser = p.ItemId
WHERE NOT EXISTS (SELECT 1 FROM #KeepPlacement k WHERE k.PlacementId = p.Id);

UPDATE p SET ItemId = m.Survivor
FROM dbo.Placements p JOIN #Merge m ON m.Loser = p.ItemId;

-- Availability is keyed (VenueId, ItemId): drop the duplicate's row where the survivor has one.
DELETE a
FROM dbo.ItemAvailability a
JOIN #Merge m ON m.Loser = a.ItemId
WHERE EXISTS (SELECT 1 FROM dbo.ItemAvailability survivor
              WHERE survivor.VenueId = a.VenueId AND survivor.ItemId = m.Survivor);

UPDATE a SET ItemId = m.Survivor
FROM dbo.ItemAvailability a JOIN #Merge m ON m.Loser = a.ItemId;

DELETE c FROM dbo.MenuImportCandidates c JOIN #Merge m ON m.Loser = c.ItemId;

DELETE i FROM dbo.Items i JOIN #Merge m ON m.Loser = i.Id;

DROP TABLE #KeepPlacement;
DROP TABLE #Merge;
COMMIT;
GO

-- The rule itself. Persisted so it can be indexed; filtered so a tombstoned name never blocks a
-- live one.
ALTER TABLE dbo.Items
    ADD CanonicalName AS dbo.CanonicalItemName(Name) PERSISTED;
GO

CREATE UNIQUE INDEX UX_Items_VenueId_CanonicalName
    ON dbo.Items (VenueId, CanonicalName)
    WHERE IsActive = 1;
GO
