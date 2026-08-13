using System.Text;
using Microsoft.Data.SqlClient;

namespace Vennu.Data.IntegrationTests.Fixtures;

/// <summary>
/// Things that must be true of the whole database, no matter what any one test was
/// written to prove.
///
/// Five consecutive independent reviews of Milestone 1 each found a real defect, and
/// the throughline was that a test proved the case its author had in mind and stopped
/// there. Review #6 is the clearest: it found a state the model says cannot exist -
/// a menu put away while a screen was still showing it - encoded in a test that
/// *passed*, because the test asserted the refusal it expected and never asked what
/// the screen was showing.
///
/// So these run after every integration test, against whatever state that test
/// happened to leave behind. A test written to check ceilings now also checks that
/// nothing it did put the menu model into an impossible shape. The sequences nobody
/// imagined get covered by tests written for other reasons, which is the only kind of
/// coverage that does not depend on somebody remembering.
///
/// Each rule names the review that paid for it. Add one whenever a defect turns out
/// to be "the model was in a state it should never be in".
/// </summary>
internal static class ModelInvariants
{
    private sealed record Invariant(string Name, string Why, string Sql);

    private static readonly Invariant[] All =
    [
        new(
            "A put-away menu is on no screen",
            "Review #6. A menu is off the shelf only when the published snapshot no longer names a screen it "
            + "could still release. Reading that from the working assignment let a menu be shelved with its "
            + "take-off unpublished, leaving a screen showing it and no act able to clear it.",
            """
            SELECT CONCAT('menu ', m.Id, ' is put away but its published version ', latest.Version,
                          ' still shows it on screen ', s.screenId) AS Offence
            FROM dbo.Menus m
            CROSS APPLY (
                SELECT TOP (1) e.Version, e.Snapshot
                FROM dbo.MenuPublishEvents e
                WHERE e.MenuId = m.Id AND e.VenueId = m.VenueId
                ORDER BY e.Version DESC
            ) latest
            CROSS APPLY OPENJSON(latest.Snapshot, '$.screens')
                WITH (screenId UNIQUEIDENTIFIER '$.screenId') s
            WHERE m.IsPutAway = 1
              AND s.screenId IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1 FROM dbo.MenuScreenAssignments taken
                  WHERE taken.ScreenId = s.screenId AND taken.MenuId <> m.Id);
            """),

        new(
            "A screen rotation contains each page once",
            "M3-A amendment A13 permits pages from multiple menus to share a screen rotation. The impossible "
            + "state is the same page appearing twice in one rotation.",
            """
            SELECT CONCAT('screen ', ScreenId, ' repeats page ', PageId, ' ', COUNT(*), ' times') AS Offence
            FROM dbo.MenuScreenAssignments
            GROUP BY ScreenId, PageId
            HAVING COUNT(*) > 1;
            """),

        new(
            "A published snapshot is readable",
            "Review #2. Snapshots were being stored in a shape the restore model could not parse, so going "
            + "back to a version would have failed on real data long after the publish looked fine.",
            """
            SELECT CONCAT('publish event ', Id, ' (menu ', MenuId, ' version ', Version, ') stored a snapshot that is not JSON') AS Offence
            FROM dbo.MenuPublishEvents
            WHERE Snapshot IS NULL OR ISJSON(Snapshot) = 0;
            """),

        new(
            "History describes the snapshot that shipped",
            "Reviews #2 and #3. The shipped set and the committed snapshot were separate observations, so "
            + "history could describe content that never went out (Q182).",
            """
            SELECT CONCAT('publish event ', e.Id, ' claims ', e.ChangeCount,
                          ' change(s) but its shipped set holds ', (SELECT COUNT(*) FROM OPENJSON(e.ShippedChanges))) AS Offence
            FROM dbo.MenuPublishEvents e
            WHERE e.ShippedChanges IS NOT NULL
              AND ISJSON(e.ShippedChanges) = 1
              AND e.ChangeCount <> (SELECT COUNT(*) FROM OPENJSON(e.ShippedChanges));
            """),

        new(
            "Published versions run 1, 2, 3 with no gaps",
            "Review #5. A version that skips or repeats means a publish committed against a base it had not "
            + "proved, which is exactly what a torn read of the published snapshot and its version produces.",
            """
            SELECT CONCAT('menu ', MenuId, ' has ', COUNT(*), ' publish events but versions run ',
                          MIN(Version), ' to ', MAX(Version)) AS Offence
            FROM dbo.MenuPublishEvents
            GROUP BY MenuId, VenueId
            HAVING MIN(Version) <> 1 OR MAX(Version) <> COUNT(*) OR COUNT(DISTINCT Version) <> COUNT(*);
            """),

        new(
            "Every publish left a record naming who did it",
            "Q207. A publish is an irreversible act; one that leaves no history entry is one nobody can be "
            + "held to.",
            """
            SELECT CONCAT('publish event ', e.Id, ' (menu ', e.MenuId, ' version ', e.Version, ') has no history entry') AS Offence
            FROM dbo.MenuPublishEvents e
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.MenuHistoryEntries h
                WHERE h.PublishEventId = e.Id AND h.Kind = N'published');
            """),

        new(
            "Nothing reaches across venues",
            "Review #2. Every row belongs to one venue, and a child that names a parent in another venue is a "
            + "tenant hole whatever the API happens to check.",
            """
            SELECT Offence FROM (
                SELECT CONCAT('section ', s.Id, ' is in venue ', s.VenueId, ' but its menu is in ', m.VenueId) AS Offence
                FROM dbo.MenuSections s INNER JOIN dbo.Menus m ON m.Id = s.MenuId WHERE s.VenueId <> m.VenueId
                UNION ALL
                SELECT CONCAT('placement ', p.Id, ' is in venue ', p.VenueId, ' but its menu is in ', m.VenueId)
                FROM dbo.Placements p INNER JOIN dbo.Menus m ON m.Id = p.MenuId WHERE p.VenueId <> m.VenueId
                UNION ALL
                SELECT CONCAT('assignment ', a.Id, ' is in venue ', a.VenueId, ' but its menu is in ', m.VenueId)
                FROM dbo.MenuScreenAssignments a INNER JOIN dbo.Menus m ON m.Id = a.MenuId WHERE a.VenueId <> m.VenueId
                UNION ALL
                SELECT CONCAT('publish event ', e.Id, ' is in venue ', e.VenueId, ' but its menu is in ', m.VenueId)
                FROM dbo.MenuPublishEvents e INNER JOIN dbo.Menus m ON m.Id = e.MenuId WHERE e.VenueId <> m.VenueId
                UNION ALL
                SELECT CONCAT('publish target ', t.Id, ' names screen ', t.ScreenId, ' in venue ', sc.VenueId,
                              ' but its publish is in ', t.VenueId)
                FROM dbo.MenuPublishTargets t INNER JOIN dbo.Screens sc ON sc.Id = t.ScreenId WHERE t.VenueId <> sc.VenueId
                UNION ALL
                SELECT CONCAT('history entry ', h.Id, ' is in venue ', h.VenueId, ' but its menu is in ', m.VenueId)
                FROM dbo.MenuHistoryEntries h INNER JOIN dbo.Menus m ON m.Id = h.MenuId WHERE h.VenueId <> m.VenueId
            ) breaches;
            """),

        new(
            "Page history names a page from its own menu and venue, unless that page was deleted",
            "M3-A Slice 2 keeps immutable page attribution after hard deletion. While the page still exists, "
            + "a history row may never borrow another menu's or venue's page identity.",
            """
            SELECT CONCAT('history entry ', h.Id, ' in menu ', h.MenuId,
                          ' names live page ', h.PageId, ' from menu ', p.MenuId,
                          ' and venue ', p.VenueId) AS Offence
            FROM dbo.MenuHistoryEntries h
            INNER JOIN dbo.MenuPages p ON p.Id=h.PageId
            WHERE h.PageId IS NOT NULL
              AND (p.MenuId<>h.MenuId OR p.VenueId<>h.VenueId);
            """),

        new(
            "A menu's published version is the version it last published",
            "Milestone 2. Two columns describing one fact, written by different statements and read by "
            + "different surfaces: the shelf card shows PublishedVersion, while the board it draws comes from "
            + "the latest publish event. A duplicate that copied its source's PublishedVersion, or a publish "
            + "that recorded an event without moving the menu on, would leave the card saying one version and "
            + "showing another - and nothing in the schema connects the two. A menu with no publish events has "
            + "never been published and says so with NULL.",
            """
            SELECT CONCAT('menu ', m.Id, ' says it published version ',
                          ISNULL(CAST(m.PublishedVersion AS NVARCHAR(20)), 'nothing'),
                          ' but its latest publish event is ',
                          ISNULL(CAST(latest.Version AS NVARCHAR(20)), 'none')) AS Offence
            FROM dbo.Menus m
            OUTER APPLY (
                SELECT MAX(e.Version) AS Version
                FROM dbo.MenuPublishEvents e
                WHERE e.MenuId = m.Id AND e.VenueId = m.VenueId
            ) latest
            WHERE ISNULL(m.PublishedVersion, -1) <> ISNULL(latest.Version, -1);
            """),

        new(
            "An item appears at most once on a page",
            "M3-A pages may share one library identity, including through Duplicate page, but an item may not "
            + "appear in two sections of the same rendered page. Migration 062 carries PageId onto placements "
            + "and makes that rule a database invariant under concurrent writes.",
            """
            SELECT CONCAT('page ', p.PageId, ' places item ', p.ItemId, ' ', COUNT(*), ' times') AS Offence
            FROM dbo.Placements p
            GROUP BY p.PageId, p.ItemId
            HAVING COUNT(*) > 1;
            """),

        new(
            "No two placements in a section share a sort order",
            "Milestone 3 readiness pass. Board order would then rest on a tiebreaker nobody chose, so the same "
            + "content could render in two different orders on two screens. The reorder write is the path that "
            + "produces this: it trusted the caller's list, and any placement omitted from that list kept a "
            + "stale sort order that could collide with a rewritten one.",
            """
            SELECT CONCAT('section ', p.MenuSectionId, ' has ', COUNT(*),
                          ' placements at sort order ', p.SortOrder) AS Offence
            FROM dbo.Placements p
            GROUP BY p.MenuSectionId, p.SortOrder
            HAVING COUNT(*) > 1;
            """),

        new(
            "A placement's section belongs to its menu",
            "Milestone 3 readiness pass. FK_Placements_SectionOnMenu already enforces this; the invariant exists "
            + "so a future schema edit that drops the constraint is caught by every integration test rather than "
            + "by a guest reading a board with an item that silently vanished from its own menu's snapshot.",
            """
            SELECT CONCAT('placement ', p.Id, ' is on menu ', p.MenuId,
                          ' but its section belongs to menu ', s.MenuId) AS Offence
            FROM dbo.Placements p
            INNER JOIN dbo.MenuSections s ON s.Id = p.MenuSectionId
            WHERE s.MenuId <> p.MenuId;
            """),

        new(
            "A deleted section leaves no placement behind",
            "Milestone 3 readiness pass. Sections are deleted rather than archived (Q96), and a delete that "
            + "released the section but not its placements would leave rows pointing at nothing - invisible to "
            + "every board read, and still counting against the items-per-menu ceiling.",
            """
            SELECT CONCAT('placement ', p.Id, ' names section ', p.MenuSectionId,
                          ', which does not exist') AS Offence
            FROM dbo.Placements p
            WHERE NOT EXISTS (SELECT 1 FROM dbo.MenuSections s WHERE s.Id = p.MenuSectionId);
            """),

        new(
            "Every item placement history fact retains page attribution",
            "M3-A Slice 3 writes item add, reorder, move and removal in the same transaction as the placement change. "
            + "A page-less fact cannot appear in the selected page timeline and loses the customer-visible context of the act.",
            """
            SELECT CONCAT('history ', h.Id, ' kind ', h.Kind, ' has no complete page attribution') AS Offence
            FROM dbo.MenuHistoryEntries h
            WHERE h.Kind IN (N'item_added',N'items_reordered',N'item_moved',N'item_removed')
              AND (h.PageId IS NULL OR NULLIF(LTRIM(RTRIM(h.PageName)),N'') IS NULL);
            """),
    ];

    /// <summary>
    /// Checks every rule against the whole database and throws once, naming each rule
    /// that broke and the rows that broke it.
    /// </summary>
    public static async Task AssertAllAsync(DatabaseFixture fixture, string testName)
    {
        var broken = new StringBuilder();

        await using var connection = new SqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();

        foreach (var invariant in All)
        {
            var offences = new List<string>();

            await using (var command = new SqlCommand(invariant.Sql, connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    offences.Add(reader.IsDBNull(0) ? "(unnamed row)" : reader.GetString(0));
                }
            }

            if (offences.Count == 0)
            {
                continue;
            }

            _ = broken.AppendLine().AppendLine($"  {invariant.Name}");
            _ = broken.AppendLine($"    {invariant.Why}");

            foreach (var offence in offences.Take(5))
            {
                _ = broken.AppendLine($"    - {offence}");
            }

            if (offences.Count > 5)
            {
                _ = broken.AppendLine($"    ...and {offences.Count - 5} more");
            }
        }

        if (broken.Length > 0)
        {
            throw new InvalidOperationException(
                $"The menu model is in a state it says cannot exist, after '{testName}'."
                + Environment.NewLine
                + "These rules hold for the whole database, so the state may have been left by an earlier test "
                + "in this run - the point is that it exists at all."
                + Environment.NewLine
                + broken);
        }
    }
}
