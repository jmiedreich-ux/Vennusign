/*
    Removes the rows that integration test runs wrote into the dev product database.

    Background: tests were pointed at dev by VENU_TEST_AZURE_SQL_CONNECTION_STRING.
    dbo.TestRecordTrace - written by the integration fixture, created by no migration -
    recorded exactly what they wrote, keyed by table and record. That trace is the only
    map of which rows to remove, which is why it is deleted last and only once the rows
    it points at are gone. See issue #745 and docs/reports/database-schema-audit-2026-08-20.md.

    Safe by default: this ROLLS BACK unless @Commit is set to 1. Run it once to read the
    report, check the numbers, then set @Commit = 1 and run it again.

    It only ever deletes rows named in the trace. It does not pattern-match on names, and
    it does not touch anything the trace does not account for.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Commit BIT = 0;   -- <<< set to 1 to actually apply

IF OBJECT_ID(N'dbo.TestRecordTrace', N'U') IS NULL
BEGIN
    RAISERROR('dbo.TestRecordTrace does not exist; there is nothing to clean up from.', 16, 1);
    RETURN;
END;

DECLARE @TracedVenues TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @TracedScreens TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY);
DECLARE @TracedCodes TABLE (Code CHAR(6) PRIMARY KEY);

INSERT INTO @TracedVenues (Id)
SELECT DISTINCT TRY_CONVERT(UNIQUEIDENTIFIER, RecordKey)
FROM dbo.TestRecordTrace
WHERE TableName = 'Venues' AND TRY_CONVERT(UNIQUEIDENTIFIER, RecordKey) IS NOT NULL;

INSERT INTO @TracedScreens (Id)
SELECT DISTINCT TRY_CONVERT(UNIQUEIDENTIFIER, RecordKey)
FROM dbo.TestRecordTrace
WHERE TableName = 'Screens' AND TRY_CONVERT(UNIQUEIDENTIFIER, RecordKey) IS NOT NULL;

INSERT INTO @TracedCodes (Code)
SELECT DISTINCT RecordKey
FROM dbo.TestRecordTrace
WHERE TableName = 'ScreenPairingCodes' AND LEN(RecordKey) = 6;

/*
    Refuse rather than guess. If anything real has attached itself to a traced row
    since the test run, this is no longer a self-contained island and a human decides.
    Verified clear on 2026-08-21: zero menus, items, assignments, onboarding states or
    subscriptions on traced venues, and zero traced screens on a non-traced venue.
*/
DECLARE @Entangled INT =
      (SELECT COUNT(*) FROM dbo.Menus m JOIN @TracedVenues v ON v.Id = m.VenueId)
    + (SELECT COUNT(*) FROM dbo.Items i JOIN @TracedVenues v ON v.Id = i.VenueId)
    + (SELECT COUNT(*) FROM dbo.MenuScreenAssignments a JOIN @TracedScreens s ON s.Id = a.ScreenId)
    + (SELECT COUNT(*) FROM dbo.CustomerOnboardingStates o JOIN @TracedVenues v ON v.Id = o.VenueId)
    + (SELECT COUNT(*) FROM dbo.VenueSubscriptions vs JOIN @TracedVenues v ON v.Id = vs.VenueId)
    + (SELECT COUNT(*) FROM dbo.Screens s JOIN @TracedScreens ts ON ts.Id = s.Id
       WHERE s.VenueId IS NOT NULL AND s.VenueId NOT IN (SELECT Id FROM @TracedVenues))
    + (SELECT COUNT(*) FROM dbo.Screens s JOIN @TracedVenues v ON v.Id = s.VenueId
       WHERE s.Id NOT IN (SELECT Id FROM @TracedScreens));

SELECT
    (SELECT COUNT(*) FROM dbo.ScreenPairingCodes p JOIN @TracedCodes c ON c.Code = p.Code) AS PairingCodesToDelete,
    (SELECT COUNT(*) FROM dbo.Screens s JOIN @TracedScreens t ON t.Id = s.Id)              AS ScreensToDelete,
    (SELECT COUNT(*) FROM dbo.Venues v JOIN @TracedVenues t ON t.Id = v.Id)                AS VenuesToDelete,
    (SELECT COUNT(*) FROM dbo.TestRecordTrace)                                             AS TraceRowsToDelete,
    @Entangled                                                                             AS EntangledRows,
    (SELECT COUNT(*) FROM dbo.Venues)  AS VenuesBefore,
    (SELECT COUNT(*) FROM dbo.Screens) AS ScreensBefore;

IF @Entangled > 0
BEGIN
    RAISERROR('Traced rows now have real data attached (%d rows). Refusing to delete; investigate before rerunning.', 16, 1, @Entangled);
    RETURN;
END;

BEGIN TRANSACTION;

    DELETE p FROM dbo.ScreenPairingCodes p JOIN @TracedCodes c ON c.Code = p.Code;
    DELETE s FROM dbo.Screens s           JOIN @TracedScreens t ON t.Id = s.Id;
    DELETE v FROM dbo.Venues v            JOIN @TracedVenues t ON t.Id = v.Id;

    -- Last, and only now: the map is worthless once the rows it points at are gone,
    -- and it is not product schema - no migration creates it. The integration fixture
    -- recreates it on demand wherever it legitimately runs.
    DROP TABLE dbo.TestRecordTrace;

    SELECT (SELECT COUNT(*) FROM dbo.Venues) AS VenuesAfter,
           (SELECT COUNT(*) FROM dbo.Screens) AS ScreensAfter,
           (SELECT COUNT(*) FROM dbo.ScreenPairingCodes) AS CodesAfter;

IF @Commit = 1
BEGIN
    COMMIT TRANSACTION;
    PRINT 'Committed.';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'Rolled back - this was a dry run. Set @Commit = 1 to apply.';
END;
