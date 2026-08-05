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
DECLARE @OrganizationId uniqueidentifier = '72000000-0000-0000-0000-000000000001';
DECLARE @VenueId uniqueidentifier = '73000000-0000-0000-0000-000000000001';
DECLARE @ScreenId uniqueidentifier = '74000000-0000-0000-0000-000000000001';
DECLARE @MenuId uniqueidentifier = '75000000-0000-0000-0000-000000000001';
DECLARE @SectionId uniqueidentifier = '76000000-0000-0000-0000-000000000001';
DECLARE @ItemId uniqueidentifier = '77000000-0000-0000-0000-000000000001';
DECLARE @AllowanceId uniqueidentifier = '78000000-0000-0000-0000-000000000001';
DECLARE @RolloutId uniqueidentifier = '79000000-0000-0000-0000-000000000001';

BEGIN TRANSACTION;

MERGE dbo.CustomerUsers AS target
USING (VALUES
    (@OwnerUserId, 'track1-owner@local.vennu.test', 'TRACK1-OWNER@LOCAL.VENNU.TEST', 'Track 1 Owner Review'),
    (@EditorUserId, 'track1-editor@local.vennu.test', 'TRACK1-EDITOR@LOCAL.VENNU.TEST', 'Track 1 Content Editor'),
    (@PublisherUserId, 'track1-publisher@local.vennu.test', 'TRACK1-PUBLISHER@LOCAL.VENNU.TEST', 'Track 1 Publisher')
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
    ('72100000-0000-0000-0000-000000000003', @OrganizationId, @PublisherUserId, 3)
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

MERGE dbo.Menus AS target
USING (VALUES (@MenuId, @VenueId, N'Acceptance Menu')) AS source (Id, VenueId, Name)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Name = source.Name, IsActive = 1, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, VenueId, Name, IsActive) VALUES (source.Id, source.VenueId, source.Name, 1);

MERGE dbo.MenuSections AS target
USING (VALUES (@SectionId, @VenueId, @MenuId, N'Featured', 0)) AS source (Id, VenueId, MenuId, Name, SortOrder)
ON target.Id = source.Id
WHEN MATCHED THEN UPDATE SET Name = source.Name, SortOrder = source.SortOrder, IsActive = 1, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT (Id, VenueId, MenuId, Name, SortOrder, IsActive)
    VALUES (source.Id, source.VenueId, source.MenuId, source.Name, source.SortOrder, 1);

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
    (@AllowanceId, @OrganizationId, @VenueId, 'screen.device.pair', 1, DATEADD(minute, -1, SYSUTCDATETIME()));

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
    1 AS ScreenPairAllowance;
