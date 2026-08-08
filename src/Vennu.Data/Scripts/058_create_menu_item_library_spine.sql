-- Menus M1 — the spine: item library, placements, availability, assignment,
-- draft/publish save model, attributable history and tier-configurable ceilings.
--
-- DISCARDED FROM THE ITEM LIBRARY (owner decision 6 + Q14-r2, recorded in
-- docs/features/menus/open-questions.md). NONE of the following are carried into
-- dbo.Items / dbo.Placements / dbo.ItemAvailability:
--
--   * MenuItems.HappyHourPrice      -- happy-hour pricing returns via Schedules-owned pricing
--   * MenuItems.QuantityAvailable   -- countdown inventory is not part of the item library
--   * MenuItems.Tags                -- free-text tags have no home in the new model
--   * MenuItems.IsPopular           -- "featured" is deferred until its board treatment exists (#678)
--   * MenuItems.AvailabilityResetUtc-- the auto-reset concept is removed; an 86 stays off
--                                      until a person turns it back on
--   * dbo.MenuItemTranslations      -- per-item translations are dropped; the Menus UI ships
--                                      English-only this milestone (#683)
--
-- PHYSICALLY DROPPED BY THIS SCRIPT: AvailabilityResetUtc and MenuItemTranslations.
-- Both are owner-killed concepts with no surviving reader once this milestone lands.
--
-- PHYSICALLY DEFERRED (owner decision, 2026-08-08): HappyHourPrice, QuantityAvailable,
-- Tags and IsPopular remain as columns for now because live code still reads and
-- writes them -- Toast/Clover/Square inventory sync, the display content payload,
-- and the legacy item-management service. They are dropped by the milestone that
-- retires their last reader (M4 display player, M6 quick update). Keeping them
-- until then is what lets this milestone leave master releasable; the library
-- above already behaves as though they do not exist.
--
-- Every other MenuItems value (Name, Description, Price, ImageUrl, IsActive,
-- SortOrder, IsAvailable, timestamps) is preserved into Items/Placements/ItemAvailability below.

-------------------------------------------------------------------------------
-- Item library
-------------------------------------------------------------------------------
CREATE TABLE dbo.Items
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Items PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(18, 2) NULL,
    ImageUrl NVARCHAR(2048) NULL,
    Source NVARCHAR(20) NOT NULL CONSTRAINT DF_Items_Source DEFAULT N'manual',
    IsActive BIT NOT NULL CONSTRAINT DF_Items_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_Items_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT CK_Items_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_Items_Source CHECK (Source IN (N'manual', N'pos', N'import'))
);

CREATE INDEX IX_Items_VenueName ON dbo.Items (VenueId, Name);

-------------------------------------------------------------------------------
-- Placement: an item placed on a section of a menu, ordered.
-- The same item can appear on several boards (decision: items are placed, not owned).
-------------------------------------------------------------------------------
CREATE TABLE dbo.Placements
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Placements PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    MenuSectionId UNIQUEIDENTIFIER NOT NULL,
    ItemId UNIQUEIDENTIFIER NOT NULL,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_Placements_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_Placements_Menus FOREIGN KEY (MenuId) REFERENCES dbo.Menus (Id),
    CONSTRAINT FK_Placements_Sections FOREIGN KEY (MenuSectionId) REFERENCES dbo.MenuSections (Id),
    CONSTRAINT FK_Placements_Items FOREIGN KEY (ItemId) REFERENCES dbo.Items (Id),
    CONSTRAINT UQ_Placements_SectionItem UNIQUE (MenuSectionId, ItemId)
);

CREATE INDEX IX_Placements_MenuSectionOrder ON dbo.Placements (MenuId, MenuSectionId, SortOrder);
CREATE INDEX IX_Placements_ItemLookup ON dbo.Placements (VenueId, ItemId);

-------------------------------------------------------------------------------
-- Availability (86): item x venue. Instant, never queued, survives publish,
-- and stays off until a person turns it back on -- there is no reset column.
-------------------------------------------------------------------------------
CREATE TABLE dbo.ItemAvailability
(
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ItemId UNIQUEIDENTIFIER NOT NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_ItemAvailability_IsAvailable DEFAULT 1,
    ChangedUtc DATETIME2(7) NOT NULL,
    ChangedBy NVARCHAR(200) NULL,
    CONSTRAINT PK_ItemAvailability PRIMARY KEY (VenueId, ItemId),
    CONSTRAINT FK_ItemAvailability_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_ItemAvailability_Items FOREIGN KEY (ItemId) REFERENCES dbo.Items (Id)
);

CREATE INDEX IX_ItemAvailability_OffRightNow
    ON dbo.ItemAvailability (VenueId, IsAvailable, ChangedUtc DESC);

-------------------------------------------------------------------------------
-- Menu -> screen assignment. Exactly one menu per screen this milestone (Q2),
-- kept as its own record so Schedules can point several menus at a screen later
-- without a migration.
-------------------------------------------------------------------------------
CREATE TABLE dbo.MenuScreenAssignments
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuScreenAssignments PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    AssignedUtc DATETIME2(7) NOT NULL,
    AssignedBy NVARCHAR(200) NULL,
    CONSTRAINT FK_MenuScreenAssignments_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_MenuScreenAssignments_Screens FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
    CONSTRAINT FK_MenuScreenAssignments_Menus FOREIGN KEY (MenuId) REFERENCES dbo.Menus (Id),
    CONSTRAINT UQ_MenuScreenAssignments_Screen UNIQUE (ScreenId)
);

CREATE INDEX IX_MenuScreenAssignments_Menu ON dbo.MenuScreenAssignments (VenueId, MenuId);

-------------------------------------------------------------------------------
-- Draft queue. One queue per menu; the count callers see is the CURRENT DIFF
-- (Q182), enforced by the unique key on the changed field.
-------------------------------------------------------------------------------
CREATE TABLE dbo.MenuDraftChanges
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuDraftChanges PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    TargetKind NVARCHAR(20) NOT NULL,
    TargetId UNIQUEIDENTIFIER NULL,
    Field NVARCHAR(100) NOT NULL,
    BeforeValue NVARCHAR(MAX) NULL,
    AfterValue NVARCHAR(MAX) NULL,
    Author NVARCHAR(200) NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_MenuDraftChanges_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_MenuDraftChanges_Menus FOREIGN KEY (MenuId) REFERENCES dbo.Menus (Id),
    CONSTRAINT CK_MenuDraftChanges_TargetKind CHECK (TargetKind IN (N'menu', N'section', N'placement', N'item', N'layout', N'theme'))
);

-- One row per (menu, target, field): re-editing the same field replaces the row
-- rather than adding a second change, so the queue is always the current diff.
CREATE UNIQUE INDEX UQ_MenuDraftChanges_CurrentDiff
    ON dbo.MenuDraftChanges (MenuId, TargetKind, TargetId, Field)
    WHERE TargetId IS NOT NULL;

CREATE UNIQUE INDEX UQ_MenuDraftChanges_CurrentDiff_MenuScope
    ON dbo.MenuDraftChanges (MenuId, TargetKind, Field)
    WHERE TargetId IS NULL;

-------------------------------------------------------------------------------
-- Publish events + per-target delivery state. A publish is atomic (Q198):
-- the whole set ships or nothing does.
-------------------------------------------------------------------------------
CREATE TABLE dbo.MenuPublishEvents
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuPublishEvents PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    Version BIGINT NOT NULL,
    ChangeCount INT NOT NULL,
    Author NVARCHAR(200) NULL,
    PublishedUtc DATETIME2(7) NOT NULL,
    Snapshot NVARCHAR(MAX) NULL,
    CONSTRAINT FK_MenuPublishEvents_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_MenuPublishEvents_Menus FOREIGN KEY (MenuId) REFERENCES dbo.Menus (Id),
    CONSTRAINT UQ_MenuPublishEvents_MenuVersion UNIQUE (MenuId, Version),
    CONSTRAINT CK_MenuPublishEvents_ChangeCount CHECK (ChangeCount >= 0)
);

CREATE INDEX IX_MenuPublishEvents_MenuHistory
    ON dbo.MenuPublishEvents (VenueId, MenuId, PublishedUtc DESC);

CREATE TABLE dbo.MenuPublishTargets
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuPublishTargets PRIMARY KEY,
    PublishEventId UNIQUEIDENTIFIER NOT NULL,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    State NVARCHAR(20) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_MenuPublishTargets_Event FOREIGN KEY (PublishEventId) REFERENCES dbo.MenuPublishEvents (Id),
    CONSTRAINT FK_MenuPublishTargets_Screens FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
    CONSTRAINT UQ_MenuPublishTargets_EventScreen UNIQUE (PublishEventId, ScreenId),
    CONSTRAINT CK_MenuPublishTargets_State CHECK (State IN (N'Pending', N'Delivered', N'Offline', N'Failed'))
);

-------------------------------------------------------------------------------
-- Attributable history. Publishes and the destructive-but-instant acts
-- (discard draft, put away, take off the screens) all land here so nothing
-- irreversible is anonymous -- provisional audit record per Q207 (#677).
-------------------------------------------------------------------------------
CREATE TABLE dbo.MenuHistoryEntries
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuHistoryEntries PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    Kind NVARCHAR(30) NOT NULL,
    PublishEventId UNIQUEIDENTIFIER NULL,
    ReplacedByVersion BIGINT NULL,
    Detail NVARCHAR(400) NULL,
    Author NVARCHAR(200) NULL,
    OccurredUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_MenuHistoryEntries_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_MenuHistoryEntries_Menus FOREIGN KEY (MenuId) REFERENCES dbo.Menus (Id),
    CONSTRAINT FK_MenuHistoryEntries_PublishEvent FOREIGN KEY (PublishEventId) REFERENCES dbo.MenuPublishEvents (Id),
    CONSTRAINT CK_MenuHistoryEntries_Kind CHECK (Kind IN (N'published', N'draft_discarded', N'put_away', N'taken_off_screens', N'restored', N'assigned'))
);

CREATE INDEX IX_MenuHistoryEntries_MenuTimeline
    ON dbo.MenuHistoryEntries (VenueId, MenuId, OccurredUtc DESC);

-------------------------------------------------------------------------------
-- Per-menu settings: dwell (Q9) and the board-too-long warning threshold (Q175),
-- both configurable rather than constants. "Put away" state (decision 16-r2).
-------------------------------------------------------------------------------
ALTER TABLE dbo.Menus ADD
    DwellSeconds INT NOT NULL CONSTRAINT DF_Menus_DwellSeconds DEFAULT 8,
    LoopWarningSeconds INT NOT NULL CONSTRAINT DF_Menus_LoopWarningSeconds DEFAULT 60,
    Theme NVARCHAR(40) NOT NULL CONSTRAINT DF_Menus_Theme DEFAULT N'coastal',
    IsPutAway BIT NOT NULL CONSTRAINT DF_Menus_IsPutAway DEFAULT 0,
    PublishedVersion BIGINT NULL;
GO

ALTER TABLE dbo.Menus ADD
    CONSTRAINT CK_Menus_DwellSeconds CHECK (DwellSeconds BETWEEN 2 AND 120),
    CONSTRAINT CK_Menus_LoopWarningSeconds CHECK (LoopWarningSeconds BETWEEN 10 AND 900);
GO

-------------------------------------------------------------------------------
-- Tier-configurable ceilings (Q201). Numbers are DEFAULTS, not constants:
-- they live in the existing capability allowance model so a tier can change them.
-------------------------------------------------------------------------------
-- Domain 1 = content, 2 = publishing; Classification 1 = essential; OperationKind 2 = write.
INSERT dbo.CapabilityDefinitions (CapabilityId, Domain, Classification, OperationKind)
SELECT v.CapabilityId, v.Domain, v.Classification, v.OperationKind
FROM (VALUES
    ('content.menu.count', 1, 1, 2),
    ('content.menu.items', 1, 1, 2),
    ('content.menu.import.lines', 1, 1, 2),
    ('publishing.history.retention', 2, 1, 1)
) AS v (CapabilityId, Domain, Classification, OperationKind)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CapabilityDefinitions existing
    WHERE existing.CapabilityId = v.CapabilityId);
GO

-- Default ceilings per venue; a tier may override any row. These numbers are
-- defaults, never constants -- the ceiling is always read from the allowance.
INSERT dbo.CapabilityAllowances (Id, OrganizationId, VenueId, CapabilityId, LimitValue, StartsUtc, EndsUtc)
SELECT NEWID(), venue.OrganizationId, venue.Id, v.CapabilityId, v.LimitValue, SYSUTCDATETIME(), NULL
FROM dbo.Venues venue
CROSS JOIN (VALUES
    ('content.menu.count', 50),
    ('content.menu.items', 500),
    ('content.menu.import.lines', 2000),
    ('publishing.history.retention', 50)
) AS v (CapabilityId, LimitValue)
WHERE venue.OrganizationId IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM dbo.CapabilityAllowances existing
    WHERE existing.VenueId = venue.Id
      AND existing.CapabilityId = v.CapabilityId);
GO

-------------------------------------------------------------------------------
-- Carry existing menu content into the item library.
-- Name/Description/Price/ImageUrl/IsActive/SortOrder/availability are preserved;
-- the fields listed at the top of this script are deliberately not carried.
-------------------------------------------------------------------------------
DECLARE @MigratedUtc DATETIME2(7) = SYSUTCDATETIME();

INSERT dbo.Items (Id, VenueId, Name, Description, Price, ImageUrl, Source, IsActive, CreatedUtc, UpdatedUtc)
SELECT
    mi.Id,
    mi.VenueId,
    LEFT(NULLIF(LTRIM(RTRIM(mi.Name)), N''), 200),
    LEFT(mi.Description, 1000),
    mi.Price,
    mi.ImageUrl,
    N'manual',
    mi.IsActive,
    mi.CreatedUtc,
    mi.UpdatedUtc
FROM dbo.MenuItems mi
WHERE NULLIF(LTRIM(RTRIM(mi.Name)), N'') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Items i WHERE i.Id = mi.Id);

INSERT dbo.Placements (Id, VenueId, MenuId, MenuSectionId, ItemId, SortOrder, CreatedUtc, UpdatedUtc)
SELECT
    NEWID(),
    mi.VenueId,
    ms.MenuId,
    mi.MenuSectionId,
    mi.Id,
    mi.SortOrder,
    mi.CreatedUtc,
    mi.UpdatedUtc
FROM dbo.MenuItems mi
INNER JOIN dbo.MenuSections ms ON ms.Id = mi.MenuSectionId
INNER JOIN dbo.Items i ON i.Id = mi.Id
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Placements p
    WHERE p.MenuSectionId = mi.MenuSectionId AND p.ItemId = mi.Id);

-- Availability carries over as a fact about the venue. AvailabilityResetUtc is
-- deliberately ignored: an 86 now stays off until a person turns it back on.
INSERT dbo.ItemAvailability (VenueId, ItemId, IsAvailable, ChangedUtc, ChangedBy)
SELECT mi.VenueId, mi.Id, mi.IsAvailable, mi.UpdatedUtc, N'migration'
FROM dbo.MenuItems mi
INNER JOIN dbo.Items i ON i.Id = mi.Id
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ItemAvailability a
    WHERE a.VenueId = mi.VenueId AND a.ItemId = mi.Id);

-- Q3: the system is not live, so migration only has to keep dev/acceptance
-- fixtures sensible. Each venue's active menus are marked published so seeded
-- data walks the API contract without a manual first publish.
INSERT dbo.MenuPublishEvents (Id, VenueId, MenuId, Version, ChangeCount, Author, PublishedUtc, Snapshot)
SELECT NEWID(), m.VenueId, m.Id, 1, 0, N'migration', @MigratedUtc, NULL
FROM dbo.Menus m
WHERE m.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM dbo.MenuPublishEvents e WHERE e.MenuId = m.Id);

UPDATE m
SET m.PublishedVersion = 1
FROM dbo.Menus m
INNER JOIN dbo.MenuPublishEvents e ON e.MenuId = m.Id AND e.Version = 1
WHERE m.PublishedVersion IS NULL;

INSERT dbo.MenuHistoryEntries (Id, VenueId, MenuId, Kind, PublishEventId, ReplacedByVersion, Detail, Author, OccurredUtc)
SELECT NEWID(), e.VenueId, e.MenuId, N'published', e.Id, NULL,
       N'Carried forward by the item-library migration.', N'migration', e.PublishedUtc
FROM dbo.MenuPublishEvents e
WHERE e.Author = N'migration'
  AND NOT EXISTS (SELECT 1 FROM dbo.MenuHistoryEntries h WHERE h.PublishEventId = e.Id);
GO

-------------------------------------------------------------------------------
-- Discard the owner-killed concepts. Availability is now a fact that stays true
-- until a person changes it, so the auto-reset column has no meaning; the Menus
-- UI ships English-only, so per-item translations have no home.
--
-- HappyHourPrice / QuantityAvailable / Tags / IsPopular are intentionally NOT
-- dropped here -- see the deferral note at the top of this script.
-------------------------------------------------------------------------------
IF OBJECT_ID('dbo.MenuItemTranslations', 'U') IS NOT NULL
    DROP TABLE dbo.MenuItemTranslations;
GO

IF COL_LENGTH('dbo.MenuItems', 'AvailabilityResetUtc') IS NOT NULL
    ALTER TABLE dbo.MenuItems DROP COLUMN AvailabilityResetUtc;
GO
