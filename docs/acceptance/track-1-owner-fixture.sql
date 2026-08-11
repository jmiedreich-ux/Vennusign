-- Track 1 owner-acceptance fixture. LOCAL DEVELOPMENT ONLY.
-- This script is idempotent for the deterministic records below and does not
-- delete unrelated local data. Run after the API has applied all DbUp scripts.

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> 'VennuSign'
    THROW 51010, 'Track 1 acceptance fixture must run only against the local VennuSign database.', 1;

IF OBJECT_ID('dbo.CapabilityDefinitions', 'U') IS NULL
    THROW 51011, 'Start the API once so DbUp applies the Track 1 schema before loading this fixture.', 1;

DECLARE @OwnerUserId uniqueidentifier = '71000000-0000-0000-0000-000000000001';
DECLARE @EditorUserId uniqueidentifier = '71000000-0000-0000-0000-000000000002';
DECLARE @PublisherUserId uniqueidentifier = '71000000-0000-0000-0000-000000000003';
DECLARE @CapacityUserId uniqueidentifier = '71000000-0000-0000-0000-000000000005';
DECLARE @OrganizationId uniqueidentifier = '72000000-0000-0000-0000-000000000001';
DECLARE @VenueId uniqueidentifier = '73000000-0000-0000-0000-000000000001';
DECLARE @ScreenId uniqueidentifier = '74000000-0000-0000-0000-000000000001';
DECLARE @MenuId uniqueidentifier = '75000000-0000-0000-0000-000000000001';
DECLARE @SectionId uniqueidentifier = '76000000-0000-0000-0000-000000000001';
DECLARE @ItemId uniqueidentifier = '77000000-0000-0000-0000-000000000001';
DECLARE @AllowanceId uniqueidentifier = '78000000-0000-0000-0000-000000000001';
DECLARE @RolloutId uniqueidentifier = '79000000-0000-0000-0000-000000000001';
DECLARE @CapacityVenueId uniqueidentifier = '73000000-0000-0000-0000-000000000003';
DECLARE @CapacityScreenId uniqueidentifier = '74000000-0000-0000-0000-000000000003';
DECLARE @CapacityAllowanceId uniqueidentifier = '78000000-0000-0000-0000-000000000003';

BEGIN TRANSACTION;

MERGE dbo.CustomerUsers AS target
USING (VALUES
    (@OwnerUserId, 'track1-owner@local.vennu.test', 'TRACK1-OWNER@LOCAL.VENNU.TEST', 'Track 1 Owner Review'),
    (@EditorUserId, 'track1-editor@local.vennu.test', 'TRACK1-EDITOR@LOCAL.VENNU.TEST', 'Track 1 Content Editor'),
    (@PublisherUserId, 'track1-publisher@local.vennu.test', 'TRACK1-PUBLISHER@LOCAL.VENNU.TEST', 'Track 1 Publisher'),
    (@CapacityUserId, 'track1-capacity@local.vennu.test', 'TRACK1-CAPACITY@LOCAL.VENNU.TEST', 'Track 1 Capacity Check')
) AS source (Id, Email, NormalizedEmail, DisplayName)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    Email = source.Email,
    NormalizedEmail = source.NormalizedEmail,
    DisplayName = source.DisplayName,
    Status = 1,
    EmailVerifiedUtc = COALESCE(target.EmailVerifiedUtc, SYSUTCDATETIME()),
    UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT
    (Id, Email, NormalizedEmail, DisplayName, Status, EmailVerifiedUtc)
    VALUES (source.Id, source.Email, source.NormalizedEmail, source.DisplayName, 1, SYSUTCDATETIME());

MERGE dbo.Organizations AS target
USING (VALUES (@OrganizationId, N'Track 1 Acceptance Organization', @OwnerUserId)) AS source (Id, Name, OwnerUserId)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Name = source.Name, OwnerUserId = source.OwnerUserId, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, Name, OwnerUserId) VALUES (source.Id, source.Name, source.OwnerUserId);

MERGE dbo.OrganizationMemberships AS target
USING (VALUES
    ('72100000-0000-0000-0000-000000000001', @OrganizationId, @OwnerUserId, 1),
    ('72100000-0000-0000-0000-000000000002', @OrganizationId, @EditorUserId, 3),
    ('72100000-0000-0000-0000-000000000003', @OrganizationId, @PublisherUserId, 3),
    ('72100000-0000-0000-0000-000000000005', @OrganizationId, @CapacityUserId, 3)
) AS source (Id, OrganizationId, UserId, Role)
ON target.OrganizationId = source.OrganizationId AND target.UserId = source.UserId
WHEN MATCHED THEN UPDATE SET Role = source.Role, RevokedUtc = NULL, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, OrganizationId, UserId, Role, JoinedUtc)
    VALUES (source.Id, source.OrganizationId, source.UserId, source.Role, SYSUTCDATETIME());

MERGE dbo.Venues AS target
USING (VALUES (@VenueId, N'Harbor Acceptance Venue', N'America/Los_Angeles', N'Bar', N'en', @OrganizationId))
    AS source (Id, Name, Timezone, Type, PrimaryLanguage, OrganizationId)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    Name = source.Name,
    Timezone = source.Timezone,
    Type = source.Type,
    PrimaryLanguage = source.PrimaryLanguage,
    OrganizationId = source.OrganizationId,
    UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, Name, Timezone, Type, PrimaryLanguage, OrganizationId)
    VALUES (source.Id, source.Name, source.Timezone, source.Type, source.PrimaryLanguage, source.OrganizationId);

MERGE dbo.Venues AS target
USING (VALUES (@CapacityVenueId, N'Capacity Check Venue', N'America/Los_Angeles', N'Bar', N'en', @OrganizationId))
    AS source (Id, Name, Timezone, Type, PrimaryLanguage, OrganizationId)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Name = source.Name, Timezone = source.Timezone, OrganizationId = source.OrganizationId, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, Name, Timezone, Type, PrimaryLanguage, OrganizationId)
    VALUES (source.Id, source.Name, source.Timezone, source.Type, source.PrimaryLanguage, source.OrganizationId);

-- A second venue, for the Menus shelf at scale (Q176: a 20-screen, 13-menu
-- check). It exists so that shelf can be deterministic: the default venue
-- accumulates menus from every spec that seeds, so nothing there can assert
-- "exactly this many menus" while the suite runs in parallel. Only the scale
-- seed writes here, and it clears the venue before each run.
MERGE dbo.Venues AS target
USING (VALUES ('73000000-0000-0000-0000-000000000002', N'Scale Check Venue', N'America/Los_Angeles', N'Bar', N'en', @OrganizationId))
    AS source (Id, Name, Timezone, Type, PrimaryLanguage, OrganizationId)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    Name = source.Name,
    Timezone = source.Timezone,
    OrganizationId = source.OrganizationId,
    UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, Name, Timezone, Type, PrimaryLanguage, OrganizationId)
    VALUES (source.Id, source.Name, source.Timezone, source.Type, source.PrimaryLanguage, source.OrganizationId);

MERGE dbo.Screens AS target
USING (VALUES (@ScreenId, @VenueId, N'sc-t1demo', N'Acceptance Screen', N'North wall'))
    AS source (Id, VenueId, ScreenKey, Name, Location)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    VenueId = source.VenueId,
    ScreenKey = source.ScreenKey,
    Name = source.Name,
    Location = source.Location,
    LastSeen = NULL,
    Status = N'Offline',
    Platform = N'web',
    AppVersion = N'track-1-review',
    UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, VenueId, ScreenKey, Name, Location, Status, Platform, AppVersion)
    VALUES (source.Id, source.VenueId, source.ScreenKey, source.Name, source.Location, N'Offline', N'web', N'track-1-review');

MERGE dbo.Screens AS target
USING (VALUES (@CapacityScreenId, @CapacityVenueId, N'sc-cap001', N'Capacity Existing Screen', N'Test wall'))
    AS source (Id, VenueId, ScreenKey, Name, Location)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET VenueId = source.VenueId, ScreenKey = source.ScreenKey, Name = source.Name, Location = source.Location,
    LastSeen = NULL, Status = N'Offline', Platform = N'web', AppVersion = N'ui-test', UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, VenueId, ScreenKey, Name, Location, Status, Platform, AppVersion)
    VALUES (source.Id, source.VenueId, source.ScreenKey, source.Name, source.Location, N'Offline', N'web', N'ui-test');

MERGE dbo.Menus AS target
USING (VALUES (@MenuId, @VenueId, N'Acceptance Menu')) AS source (Id, VenueId, Name)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Name = source.Name, IsActive = 1, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, VenueId, Name, IsActive) VALUES (source.Id, source.VenueId, source.Name, 1);

DECLARE @PageId UNIQUEIDENTIFIER = (SELECT TOP (1) Id FROM dbo.MenuPages WHERE MenuId=@MenuId AND VenueId=@VenueId ORDER BY SortOrder, Id);
IF @PageId IS NULL
BEGIN
    SET @PageId = '75500000-0000-0000-0000-000000000001';
    INSERT dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
    VALUES (@PageId, @VenueId, @MenuId, N'Page 1', 0, SYSUTCDATETIME(), SYSUTCDATETIME());
END;

MERGE dbo.MenuSections AS target
USING (VALUES (@SectionId, @VenueId, @MenuId, @PageId, N'Featured', 0)) AS source (Id, VenueId, MenuId, PageId, Name, SortOrder)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET PageId=source.PageId, Name = source.Name, SortOrder = source.SortOrder, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, VenueId, MenuId, PageId, Name, SortOrder)
    VALUES (source.Id, source.VenueId, source.MenuId, source.PageId, source.Name, source.SortOrder);

MERGE dbo.MenuItems AS target
USING (VALUES (@ItemId, @VenueId, @SectionId, N'Harbor Lemonade', N'Owner acceptance fixture', CAST(4.50 AS decimal(19,4)), 0))
    AS source (Id, VenueId, MenuSectionId, Name, Description, Price, SortOrder)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    Name = source.Name,
    Description = source.Description,
    Price = source.Price,
    IsAvailable = 1,
    SortOrder = source.SortOrder,
    IsActive = 1,
    UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT
    (Id, VenueId, MenuSectionId, Name, Description, Price, IsAvailable, SortOrder, IsActive)
    VALUES (source.Id, source.VenueId, source.MenuSectionId, source.Name, source.Description, source.Price, 1, source.SortOrder, 1);

DELETE usage
FROM dbo.CapabilityAllowanceUsage usage
INNER JOIN dbo.CapabilityAllowances allowance ON allowance.Id = usage.AllowanceId
WHERE allowance.OrganizationId = @OrganizationId
  AND allowance.VenueId = @VenueId
  AND allowance.CapabilityId = 'screen.device.pair';

DELETE dbo.CapabilityAllowances
WHERE OrganizationId = @OrganizationId
  AND VenueId = @VenueId
  AND CapabilityId = 'screen.device.pair';

INSERT dbo.CapabilityAllowances
    (Id, OrganizationId, VenueId, CapabilityId, LimitValue, StartsUtc)
VALUES
    (@AllowanceId, @OrganizationId, @VenueId, 'screen.device.pair', 100, DATEADD(minute, -1, SYSUTCDATETIME()));

DELETE usage
FROM dbo.CapabilityAllowanceUsage usage
INNER JOIN dbo.CapabilityAllowances allowance ON allowance.Id = usage.AllowanceId
WHERE allowance.OrganizationId = @OrganizationId AND allowance.VenueId = @CapacityVenueId
  AND allowance.CapabilityId = 'screen.device.pair';
DELETE dbo.CapabilityAllowances
WHERE OrganizationId = @OrganizationId AND VenueId = @CapacityVenueId AND CapabilityId = 'screen.device.pair';
INSERT dbo.CapabilityAllowances (Id, OrganizationId, VenueId, CapabilityId, LimitValue, StartsUtc)
VALUES (@CapacityAllowanceId, @OrganizationId, @CapacityVenueId, 'screen.device.pair', 1, DATEADD(minute, -1, SYSUTCDATETIME()));

DELETE dbo.CapabilityRollouts
WHERE OrganizationId = @OrganizationId
  AND VenueId = @VenueId
  AND CapabilityId = 'schedule.promotion.automate';

INSERT dbo.CapabilityRollouts
    (Id, CapabilityId, OrganizationId, VenueId, RolloutState, StartsUtc, RetryAfterUtc)
VALUES
    (@RolloutId, 'schedule.promotion.automate', @OrganizationId, @VenueId, 3,
     DATEADD(minute, -1, SYSUTCDATETIME()), DATEADD(hour, 1, SYSUTCDATETIME()));

COMMIT TRANSACTION;

SELECT
    @OrganizationId AS OrganizationId,
    @VenueId AS VenueId,
    @ScreenId AS ScreenId,
    'offline' AS InitialScreenState,
    100 AS ScreenPairAllowance;

-------------------------------------------------------------------------------
-- Menus M1 spine fixture. LOCAL DEVELOPMENT ONLY.
--
-- Migration 058 is a fresh start (Q45): it creates the library tables empty and
-- carries nothing across from the legacy tables. Seed and demo data therefore
-- belongs here, which is also what Q3 asks for -- a seeded menu that is already
-- assigned and published, so tests and demos have something real to walk.
--
-- This section RESTORES THE CANONICAL STATE on every run: it repairs drifted
-- values, removes demo leftovers for the acceptance venue's library, and
-- rebuilds the publish chain from scratch. Re-running after an edited or
-- partial run therefore always lands on the same state, rather than reporting
-- success against whatever was left behind.
-------------------------------------------------------------------------------
DECLARE @M1VenueId UNIQUEIDENTIFIER = '73000000-0000-0000-0000-000000000001';
DECLARE @M1MenuId UNIQUEIDENTIFIER = '75000000-0000-0000-0000-000000000001';
DECLARE @M1SectionId UNIQUEIDENTIFIER = '76000000-0000-0000-0000-000000000001';
DECLARE @M1SharedMenuId UNIQUEIDENTIFIER = '75000000-0000-0000-0000-000000000002';
DECLARE @M1SharedSectionId UNIQUEIDENTIFIER = '76000000-0000-0000-0000-000000000002';
DECLARE @M1ScreenId UNIQUEIDENTIFIER = '74000000-0000-0000-0000-000000000001';
DECLARE @M1ItemId UNIQUEIDENTIFIER = '77000000-0000-0000-0000-000000000001';
DECLARE @M1SecondItemId UNIQUEIDENTIFIER = '77000000-0000-0000-0000-000000000002';
DECLARE @M1Now DATETIME2(7) = SYSUTCDATETIME();
DECLARE @M1PageId UNIQUEIDENTIFIER = (SELECT TOP (1) Id FROM dbo.MenuPages WHERE MenuId=@M1MenuId AND VenueId=@M1VenueId ORDER BY SortOrder, Id);
DECLARE @M1SharedPageId UNIQUEIDENTIFIER;

BEGIN TRANSACTION;

-- The menu's own working values return to canonical, so a drifted theme or
-- dwell from an earlier demo cannot leak into the rebuilt publish snapshot.
-- Canonical for the theme is NO theme attached: that is a valid state (Q86), no
-- named look exists to attach, and migration 059 removed the 'coastal' this used
-- to set - a value the fixture would otherwise put straight back.
UPDATE dbo.Menus
SET Theme = NULL, DwellSeconds = 8, LoopWarningSeconds = 60, IsPutAway = 0, UpdatedUtc = @M1Now
WHERE Id = @M1MenuId AND VenueId = @M1VenueId;

-- M3 acceptance case 6 starts with one shared item on two menus. The owner judges
-- the price explanation; they do not have to manufacture the condition first.
MERGE dbo.Menus AS target
USING (VALUES (@M1SharedMenuId, @M1VenueId, N'Harbor Evening Menu')) AS source (Id, VenueId, Name)
    ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    VenueId = source.VenueId, Name = source.Name, IsActive = 1, IsPutAway = 0, UpdatedUtc = @M1Now
WHEN NOT MATCHED THEN
    INSERT (Id, VenueId, Name, IsActive, IsPutAway, CreatedUtc, UpdatedUtc)
    VALUES (source.Id, source.VenueId, source.Name, 1, 0, @M1Now, @M1Now);

SET @M1SharedPageId = (SELECT TOP (1) Id FROM dbo.MenuPages WHERE MenuId=@M1SharedMenuId AND VenueId=@M1VenueId ORDER BY SortOrder, Id);
IF @M1SharedPageId IS NULL
BEGIN
    SET @M1SharedPageId = '75500000-0000-0000-0000-000000000002';
    INSERT dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
    VALUES (@M1SharedPageId, @M1VenueId, @M1SharedMenuId, N'Page 1', 0, @M1Now, @M1Now);
END;

MERGE dbo.MenuSections AS target
USING (VALUES (@M1SharedSectionId, @M1VenueId, @M1SharedMenuId, @M1SharedPageId, N'Drinks', 0))
    AS source (Id, VenueId, MenuId, PageId, Name, SortOrder)
    ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    VenueId = source.VenueId, MenuId = source.MenuId, PageId = source.PageId, Name = source.Name,
    SortOrder = source.SortOrder, UpdatedUtc = @M1Now
WHEN NOT MATCHED THEN
    INSERT (Id, VenueId, MenuId, PageId, Name, SortOrder, CreatedUtc, UpdatedUtc)
    VALUES (source.Id, source.VenueId, source.MenuId, source.PageId, source.Name, source.SortOrder, @M1Now, @M1Now);

-- Prices are stored exactly as typed (Q115/Q190), so the fixture deliberately
-- includes a market price alongside a decimal one. Matched rows are repaired,
-- never trusted.
MERGE dbo.Items AS target
USING (VALUES
    (@M1ItemId, @M1VenueId, N'Harbor Lemonade', N'House lemonade, over crushed ice.', N'9.5'),
    (@M1SecondItemId, @M1VenueId, N'Market Oysters', N'Half dozen, whatever came in today.', N'MP')
) AS source (Id, VenueId, Name, Description, Price)
    ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET
    Name = source.Name,
    Description = source.Description,
    Price = source.Price,
    IsActive = 1,
    UpdatedUtc = @M1Now
WHEN NOT MATCHED THEN
    INSERT (Id, VenueId, Name, Description, Price, Source, IsActive, CreatedUtc, UpdatedUtc)
    VALUES (source.Id, source.VenueId, source.Name, source.Description, source.Price, N'manual', 1, @M1Now, @M1Now);

-- Exactly the two canonical placements on the acceptance menu; demo-created
-- placements on this menu go.
DELETE FROM dbo.Placements
WHERE MenuId = @M1MenuId AND ItemId NOT IN (@M1ItemId, @M1SecondItemId);

MERGE dbo.Placements AS target
USING (VALUES
    (@M1ItemId, 0),
    (@M1SecondItemId, 1)
) AS source (ItemId, SortOrder)
    ON target.MenuSectionId = @M1SectionId AND target.ItemId = source.ItemId
WHEN MATCHED THEN UPDATE SET SortOrder = source.SortOrder, UpdatedUtc = @M1Now
WHEN NOT MATCHED THEN
    INSERT (Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
    VALUES (NEWID(), @M1VenueId, @M1MenuId, @M1SectionId, source.ItemId, source.SortOrder, @M1Now, @M1Now);

-- Restore the second menu to exactly the one shared item on every fixture run.
DELETE FROM dbo.Placements
WHERE MenuId = @M1SharedMenuId AND ItemId <> @M1ItemId;

MERGE dbo.Placements AS target
USING (VALUES (@M1ItemId, 0)) AS source (ItemId, SortOrder)
    ON target.MenuSectionId = @M1SharedSectionId AND target.ItemId = source.ItemId
WHEN MATCHED THEN UPDATE SET SortOrder = source.SortOrder, UpdatedUtc = @M1Now
WHEN NOT MATCHED THEN
    INSERT (Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
    VALUES (NEWID(), @M1VenueId, @M1SharedMenuId, @M1SharedSectionId, source.ItemId, source.SortOrder, @M1Now, @M1Now);

-- Demo-created library items for this venue go with their availability rows,
-- so re-runs cannot accumulate lookalikes the demo might then pick up.
DELETE FROM dbo.ItemAvailability
WHERE VenueId = @M1VenueId AND ItemId NOT IN (@M1ItemId, @M1SecondItemId);

DELETE FROM dbo.Items
WHERE VenueId = @M1VenueId
  AND Id NOT IN (@M1ItemId, @M1SecondItemId)
  AND NOT EXISTS (SELECT 1 FROM dbo.Placements p WHERE p.ItemId = dbo.Items.Id);

-- Both canonical items start the workbook available, whatever the last run did.
MERGE dbo.ItemAvailability AS target
USING (VALUES (@M1ItemId), (@M1SecondItemId)) AS source (ItemId)
    ON target.VenueId = @M1VenueId AND target.ItemId = source.ItemId
WHEN MATCHED THEN UPDATE SET IsAvailable = 1, ChangedUtc = @M1Now, ChangedBy = N'fixture'
WHEN NOT MATCHED THEN
    INSERT (VenueId, ItemId, IsAvailable, ChangedUtc, ChangedBy)
    VALUES (@M1VenueId, source.ItemId, 1, @M1Now, N'fixture');

-- Q3: the seeded menu is already on a screen and already published, so a demo
-- does not have to perform a first publish before it can test anything. A
-- re-pointed or removed assignment is put back.
MERGE dbo.MenuScreenAssignments AS target
USING (SELECT @M1ScreenId AS ScreenId) AS source
    ON target.ScreenId = source.ScreenId
WHEN MATCHED THEN UPDATE SET
    VenueId = @M1VenueId, MenuId = @M1MenuId, PageId = @M1PageId, AssignedUtc = @M1Now, AssignedBy = N'fixture'
WHEN NOT MATCHED THEN
    INSERT (Id, VenueId, ScreenId, MenuId, PageId, AssignedUtc, AssignedBy)
    VALUES (NEWID(), @M1VenueId, @M1ScreenId, @M1MenuId, @M1PageId, @M1Now, N'fixture');

-- The publish chain is rebuilt from nothing every run: version 1 is the state
-- seeded above, its shipped set is an honest empty [], and its snapshot uses
-- JSON_QUERY exactly as the runtime does so it parses with the restore model.
DELETE FROM dbo.MenuHistoryEntries WHERE MenuId = @M1MenuId;
DELETE t FROM dbo.MenuPublishTargets t
INNER JOIN dbo.MenuPublishEvents e ON e.Id = t.PublishEventId
WHERE e.MenuId = @M1MenuId;
DELETE FROM dbo.MenuPublishEvents WHERE MenuId = @M1MenuId;

DECLARE @M1PublishId UNIQUEIDENTIFIER = NEWID();

INSERT dbo.MenuPublishEvents (Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot, ShippedChanges)
VALUES (@M1PublishId, @M1VenueId, @M1MenuId, 1, 0, N'fixture', @M1Now,
        (
            SELECT m.Id AS menuId, m.Name AS name, m.Theme AS theme,
                   m.DwellSeconds AS dwellSeconds, m.LoopWarningSeconds AS loopWarningSeconds,
                   JSON_QUERY((
                       SELECT CAST(a.ScreenId AS NVARCHAR(36)) AS screenId
                       FROM dbo.MenuScreenAssignments a
                       WHERE a.MenuId = m.Id AND a.VenueId = @M1VenueId
                       ORDER BY a.ScreenId
                       FOR JSON PATH
                   )) AS screens,
                   JSON_QUERY((
                       SELECT s.Id AS sectionId, s.Name AS name, s.SortOrder AS sortOrder,
                           JSON_QUERY((
                               SELECT p.ItemId AS itemId, i.Name AS name, i.Description AS description,
                                      i.Price AS price, p.SortOrder AS sortOrder
                               FROM dbo.Placements p
                               INNER JOIN dbo.Items i ON i.Id = p.ItemId AND i.VenueId = p.VenueId
                               WHERE p.MenuSectionId = s.Id AND p.VenueId = @M1VenueId
                               ORDER BY p.SortOrder, p.Id
                               FOR JSON PATH
                           )) AS items
                       FROM dbo.MenuSections s
                       WHERE s.MenuId = m.Id AND s.VenueId = @M1VenueId
                       ORDER BY s.SortOrder, s.Id
                       FOR JSON PATH
                   )) AS sections
            FROM dbo.Menus m
            WHERE m.Id = @M1MenuId AND m.VenueId = @M1VenueId
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        ),
        N'[]');

INSERT dbo.MenuPublishTargets (Id, VenueId, PublishEventId, ScreenId, State, UpdatedUtc)
VALUES (NEWID(), @M1VenueId, @M1PublishId, @M1ScreenId, N'Offline', @M1Now);

INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
VALUES (NEWID(), @M1VenueId, @M1MenuId, N'published', @M1PublishId, NULL, N'Seeded by the acceptance fixture.', N'fixture', @M1Now);

UPDATE dbo.Menus SET PublishedVersion = 1 WHERE Id = @M1MenuId;

COMMIT TRANSACTION;
GO
