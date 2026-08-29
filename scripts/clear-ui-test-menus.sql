/*
    Clear the menus the UI suite leaves behind in the local test database.

    WHY THIS EXISTS: the suite creates menus faster than it puts them away, and the venue it runs
    in has a menu ceiling. Once the ceiling is reached, anything that creates a menu through the UI
    is refused - correctly - and specs fail for a reason that has nothing to do with what they test
    (see #953, and quick-update's blank-creation case). #926 raised the ceiling for SEEDED menus and
    added per-test put-away; neither covers a menu created by clicking, or one left by a run that
    was killed before its cleanup.

    WHAT THIS KEEPS: the two menus the fixture owns, BY ID - it merges them under fixed ids, and
    acceptance cases assert on them. Anything else a person made by hand is kept too, because the
    rule below names what the machine generates rather than guessing at what it does not.

    Keeping them by NAME was the first attempt and was wrong: the suite leaves behind copies
    wearing the fixture's names, and ninety-six of them survived a run that was supposed to clear
    the venue. A fixture menu is the one with the fixture's id; everything else calling itself
    "Acceptance Menu" is residue.

    WHAT THIS DISCARDS: menus the seed generated ("<label> menu <8 hex>", "Scale menu NN", and
    their " copy" duplicates) and blank menus the UI created ("Blank <timestamp>"), together with
    their pages, sections, placements, history, publish events, and any import session that
    produced or targeted one. A session is deleted rather than detached: its status and its
    completed menu are tied by CK_MenuImportSessions_Completion, so a session whose menu is gone
    cannot be left behind honestly.

    NOT FOR dev OR stage. This targets (localdb)\MSSQLLocalDB and the venues the fixture creates.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

CREATE TABLE #Doomed (MenuId UNIQUEIDENTIFIER PRIMARY KEY);

INSERT #Doomed (MenuId)
SELECT m.Id
FROM dbo.Menus m
WHERE m.Id NOT IN ('75000000-0000-0000-0000-000000000001',   -- fixture: Acceptance Menu
                   '75000000-0000-0000-0000-000000000002')   -- fixture: Harbor Evening Menu
  AND (
        m.Name LIKE N'% menu [0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f]%'
     OR m.Name LIKE N'Scale menu %'
     OR m.Name LIKE N'Blank [0-9]%'
     OR m.Name LIKE N'Untitled menu%'
     OR m.Name LIKE N'm3 gate [0-9]%'
     OR m.Name LIKE N'Northside Social [0-9a-f][0-9a-f][0-9a-f][0-9a-f]%'  -- suffixed copies only
     OR m.Name IN (N'Acceptance Menu', N'Harbor Evening Menu')   -- copies wearing the fixture's names
      );

SELECT COUNT(*) AS WillDelete FROM #Doomed;

-- Any import session that produced or targeted one of these menus goes with it.
CREATE TABLE #DoomedSession (SessionId UNIQUEIDENTIFIER PRIMARY KEY);
INSERT #DoomedSession (SessionId)
SELECT s.Id FROM dbo.MenuImportSessions s
JOIN #Doomed d ON d.MenuId = s.CompletedMenuId OR d.MenuId = s.TargetMenuId;

DELETE x FROM dbo.MenuImportCreatedLines    x JOIN #DoomedSession s ON s.SessionId = x.SessionId;
DELETE x FROM dbo.MenuImportAnswers         x JOIN #DoomedSession s ON s.SessionId = x.SessionId;
DELETE x FROM dbo.MenuImportCandidates      x JOIN #DoomedSession s ON s.SessionId = x.SessionId;
DELETE x FROM dbo.MenuImportQuestionLines   x JOIN #DoomedSession s ON s.SessionId = x.SessionId;
DELETE x FROM dbo.MenuImportReviewQuestions x JOIN #DoomedSession s ON s.SessionId = x.SessionId;
DELETE x FROM dbo.MenuImportSourceLines     x JOIN #DoomedSession s ON s.SessionId = x.SessionId;

-- The session names its completed snapshot (FK_MenuImportSessions_CompletedSnapshot), so the
-- session goes first and the snapshots it pointed at go after it.
DELETE x FROM dbo.MenuImportSessions        x JOIN #DoomedSession s ON s.SessionId = x.Id;
DELETE x FROM dbo.MenuImportReplacementSnapshots x JOIN #DoomedSession s ON s.SessionId = x.SessionId;

-- Created-lines can also name a doomed menu from a session that survives.
DELETE x FROM dbo.MenuImportCreatedLines x JOIN #Doomed d ON d.MenuId = x.MenuId;
DELETE x FROM dbo.MenuImportReplacementSnapshots x JOIN #Doomed d ON d.MenuId = x.MenuId;

DELETE x FROM dbo.MenuPublishTargets x JOIN dbo.MenuPublishEvents e ON e.Id = x.PublishEventId JOIN #Doomed d ON d.MenuId = e.MenuId;
DELETE x FROM dbo.MenuHistoryEntries x JOIN #Doomed d ON d.MenuId = x.MenuId;
DELETE x FROM dbo.MenuPublishEvents  x JOIN #Doomed d ON d.MenuId = x.MenuId;

DELETE x FROM dbo.MenuScreenAssignments x JOIN #Doomed d ON d.MenuId = x.MenuId;
DELETE x FROM dbo.Placements            x JOIN #Doomed d ON d.MenuId = x.MenuId;
DELETE x FROM dbo.MenuItems             x JOIN dbo.MenuSections s ON s.Id = x.MenuSectionId JOIN #Doomed d ON d.MenuId = s.MenuId;
DELETE x FROM dbo.MenuSections          x JOIN #Doomed d ON d.MenuId = x.MenuId;
DELETE x FROM dbo.MenuPages             x JOIN #Doomed d ON d.MenuId = x.MenuId;
DELETE x FROM dbo.Menus                 x JOIN #Doomed d ON d.MenuId = x.Id;

DROP TABLE #DoomedSession;
DROP TABLE #Doomed;
COMMIT;

SELECT v.Name AS Venue, COUNT(m.Id) AS MenusLeft
FROM dbo.Venues v LEFT JOIN dbo.Menus m ON m.VenueId = v.Id AND m.IsActive = 1
GROUP BY v.Name ORDER BY COUNT(m.Id) DESC;
