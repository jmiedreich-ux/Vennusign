-- Menus M1 - the spine: item library, placements, availability, assignment,
-- draft/publish save model, attributable history and tier-configurable ceilings.
--
-- FRESH START (owner answer Q45). The new tables begin empty. Legacy menu data is
-- deliberately NOT carried across: the old tables stay untouched but unused, and a
-- carry script remains possible any time before they retire. Nothing here reads
-- dbo.MenuItems or dbo.MenuSections for content.
--
-- NOT CARRIED INTO THE LIBRARY, and named here because the decision is the
-- migration's to record (owner decision 6 + Q14-r2):
--   * MenuItems.HappyHourPrice      -- happy-hour pricing returns via Schedules-owned pricing
--   * MenuItems.QuantityAvailable   -- countdown inventory is not part of the item library
--   * MenuItems.Tags                -- free-text tags have no home in the new model
--   * MenuItems.IsPopular           -- "featured" waits for its board treatment (#678)
--   * MenuItems.AvailabilityResetUtc-- the auto-reset concept is removed entirely
--   * dbo.MenuItemTranslations      -- per-item translations dropped; English-only (#683)
--
-- PHYSICALLY DROPPED HERE: AvailabilityResetUtc and MenuItemTranslations. Both are
-- owner-killed concepts with no surviving reader once this milestone lands.
--
-- PHYSICALLY DEFERRED: HappyHourPrice, QuantityAvailable, Tags and IsPopular remain
-- as columns because live code still reads and writes them (Toast/Clover/Square
-- inventory sync, the display payload, the legacy item service). They are dropped by
-- the milestone that retires their last reader.
--
-- TENANCY IS A DATABASE INVARIANT. Every child carries VenueId and references its
-- parent through the parent's (Id, VenueId) unique key, following the pattern
-- established in 012_create_menu_domain.sql. A row therefore cannot point at another
-- venue's menu, section, item or screen even if a caller supplies a foreign id.

-------------------------------------------------------------------------------
-- Item library
-------------------------------------------------------------------------------
CREATE TABLE dbo.Items
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Items PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    -- Prices are stored exactly as typed and rendered exactly as typed (Q115/Q190):
    -- "12", "9.5" and "MP" are all valid and all round-trip unchanged. A numeric type
    -- would normalise the text and could not hold "MP" at all. Null means no price yet.
    Price NVARCHAR(40) NULL,
    ImageUrl NVARCHAR(2048) NULL,
    Source NVARCHAR(20) NOT NULL CONSTRAINT DF_Items_Source DEFAULT N'manual',
    IsActive BIT NOT NULL CONSTRAINT DF_Items_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_Items_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_Items_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT CK_Items_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_Items_Source CHECK (Source IN (N'manual', N'pos', N'import'))
);

CREATE INDEX IX_Items_VenueName ON dbo.Items (VenueId, Name);

-------------------------------------------------------------------------------
-- Placement: an item placed on a section of a menu, ordered. Item identity is
-- permanent and placements never re-mint items (Q43), so an 86 keeps its anchor
-- across every publish by construction.
-------------------------------------------------------------------------------
-- Placements prove section-in-menu through this key, so it must exist first.
ALTER TABLE dbo.MenuSections ADD CONSTRAINT UQ_MenuSections_Id_MenuId UNIQUE (Id, MenuId);
GO

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
    CONSTRAINT FK_Placements_Menus FOREIGN KEY (MenuId, VenueId) REFERENCES dbo.Menus (Id, VenueId),
    CONSTRAINT FK_Placements_Sections FOREIGN KEY (MenuSectionId, VenueId) REFERENCES dbo.MenuSections (Id, VenueId),
    -- The section must belong to the same menu as the placement, not merely the
    -- same venue: without this, menu A could reference a section of menu B and the
    -- placement would silently vanish from every snapshot of menu A.
    CONSTRAINT FK_Placements_SectionOnMenu FOREIGN KEY (MenuSectionId, MenuId) REFERENCES dbo.MenuSections (Id, MenuId),
    CONSTRAINT FK_Placements_Items FOREIGN KEY (ItemId, VenueId) REFERENCES dbo.Items (Id, VenueId),
    CONSTRAINT UQ_Placements_SectionItem UNIQUE (MenuSectionId, ItemId)
);

CREATE INDEX IX_Placements_MenuSectionOrder ON dbo.Placements (MenuId, MenuSectionId, SortOrder);
CREATE INDEX IX_Placements_ItemLookup ON dbo.Placements (VenueId, ItemId);

-------------------------------------------------------------------------------
-- Availability (86): item x venue. Instant, never queued, survives publish, and
-- stays off until a person turns it back on -- there is no reset column.
-------------------------------------------------------------------------------
CREATE TABLE dbo.ItemAvailability
(
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ItemId UNIQUEIDENTIFIER NOT NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_ItemAvailability_IsAvailable DEFAULT 1,
    ChangedUtc DATETIME2(7) NOT NULL,
    ChangedBy NVARCHAR(200) NULL,
    CONSTRAINT PK_ItemAvailability PRIMARY KEY (VenueId, ItemId),
    CONSTRAINT FK_ItemAvailability_Items FOREIGN KEY (ItemId, VenueId) REFERENCES dbo.Items (Id, VenueId)
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
    CONSTRAINT FK_MenuScreenAssignments_Screens FOREIGN KEY (VenueId, ScreenId) REFERENCES dbo.Screens (VenueId, Id),
    CONSTRAINT FK_MenuScreenAssignments_Menus FOREIGN KEY (MenuId, VenueId) REFERENCES dbo.Menus (Id, VenueId),
    CONSTRAINT UQ_MenuScreenAssignments_Screen UNIQUE (ScreenId)
);

CREATE INDEX IX_MenuScreenAssignments_Menu ON dbo.MenuScreenAssignments (VenueId, MenuId);

-------------------------------------------------------------------------------
-- There is deliberately NO draft table. The draft is derived (owner decision,
-- 2026-08-09): it is the computed difference between the working rows and the
-- latest published snapshot, so the count callers see is the CURRENT DIFF (Q182)
-- by construction and cannot be misreported by any writer.
-------------------------------------------------------------------------------

-------------------------------------------------------------------------------
-- Publish events + per-target delivery state. A publish is atomic (Q198): the
-- whole set ships or nothing does. Snapshot holds the published content itself,
-- so a version can be rendered and restored later without re-deriving it from a
-- queue that no longer exists (Q43, Q67).
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
    ShippedChanges NVARCHAR(MAX) NULL,
    CONSTRAINT FK_MenuPublishEvents_Menus FOREIGN KEY (MenuId, VenueId) REFERENCES dbo.Menus (Id, VenueId),
    CONSTRAINT UQ_MenuPublishEvents_MenuVersion UNIQUE (MenuId, Version),
    -- Children reference an event together with its venue (targets) or its menu
    -- and venue (history), so a child row cannot name another tenant's event.
    CONSTRAINT UQ_MenuPublishEvents_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_MenuPublishEvents_Id_MenuId_VenueId UNIQUE (Id, MenuId, VenueId),
    CONSTRAINT CK_MenuPublishEvents_ChangeCount CHECK (ChangeCount >= 0)
);

CREATE INDEX IX_MenuPublishEvents_MenuHistory
    ON dbo.MenuPublishEvents (VenueId, MenuId, PublishedUtc DESC);

CREATE TABLE dbo.MenuPublishTargets
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuPublishTargets PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    PublishEventId UNIQUEIDENTIFIER NOT NULL,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    State NVARCHAR(20) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    -- The event and the screen must both belong to the target's venue: a publish
    -- for venue A cannot name venue B's screen at the database layer.
    CONSTRAINT FK_MenuPublishTargets_Event FOREIGN KEY (PublishEventId, VenueId) REFERENCES dbo.MenuPublishEvents (Id, VenueId),
    CONSTRAINT FK_MenuPublishTargets_Screens FOREIGN KEY (VenueId, ScreenId) REFERENCES dbo.Screens (VenueId, Id),
    CONSTRAINT UQ_MenuPublishTargets_EventScreen UNIQUE (PublishEventId, ScreenId),
    CONSTRAINT CK_MenuPublishTargets_State CHECK (State IN (N'Pending', N'Delivered', N'Offline', N'Failed'))
);

-------------------------------------------------------------------------------
-- Attributable history. Publishes and the destructive-but-instant acts all land
-- here in the same transaction as the act itself, so nothing irreversible can
-- become anonymous through a partial failure (Q207).
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
    CONSTRAINT FK_MenuHistoryEntries_Menus FOREIGN KEY (MenuId, VenueId) REFERENCES dbo.Menus (Id, VenueId),
    -- A history entry that names a publish event must name one of its own menu's
    -- events in its own venue, never another tenant's.
    CONSTRAINT FK_MenuHistoryEntries_PublishEvent FOREIGN KEY (PublishEventId, MenuId, VenueId) REFERENCES dbo.MenuPublishEvents (Id, MenuId, VenueId),
    CONSTRAINT CK_MenuHistoryEntries_Kind CHECK (Kind IN (N'published', N'draft_discarded', N'put_away', N'put_back', N'taken_off_screens', N'restored', N'assigned'))
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
-- Capability definitions. Ceilings are typed allowances, so they need a
-- definition row before any allowance can reference them.
-- Domain 1 = content, 2 = publishing; Classification 1 = essential;
-- OperationKind 1 = read, 2 = write.
-------------------------------------------------------------------------------
INSERT dbo.CapabilityDefinitions (CapabilityId, Domain, Classification, OperationKind)
SELECT v.CapabilityId, v.Domain, v.Classification, v.OperationKind
FROM (VALUES
    ('content.menu.count', 1, 1, 2),
    ('content.menu.items', 1, 1, 2),
    ('content.menu.import.lines', 1, 1, 2),
    ('publishing.history.retention', 2, 1, 1),
    ('content.menu.manage', 1, 1, 2),
    ('content.menu.import', 1, 1, 2),
    ('publishing.history.view', 2, 1, 1)
) AS v (CapabilityId, Domain, Classification, OperationKind)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.CapabilityDefinitions existing
    WHERE existing.CapabilityId = v.CapabilityId);
GO

-- Ceilings for the venues that exist today. Venues created later are covered by
-- the documented defaults in code, which are used only when no allowance row
-- exists; a missing row is never read as "unlimited".
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
-- The three Menus permissions (Q24). Auto-granted to every role that already
-- edits items so gating can be wired now. The owner has since decided they must
-- become separately grantable; that needs a role-to-permission mapping and is
-- tracked as issue #686.
-------------------------------------------------------------------------------
INSERT dbo.AuthorityPermissions (PermissionId, CapabilityId)
SELECT v.PermissionId, v.PermissionId
FROM (VALUES
    ('content.menu.manage'),
    ('content.menu.import'),
    ('publishing.history.view')
) AS v (PermissionId)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.AuthorityPermissions existing
    WHERE existing.PermissionId = v.PermissionId);
GO

INSERT dbo.AuthorityRolePermissions (RoleKey, PermissionId)
SELECT DISTINCT existing.RoleKey, v.PermissionId
FROM dbo.AuthorityRolePermissions existing
CROSS JOIN (VALUES
    ('content.menu.manage'),
    ('content.menu.import'),
    ('publishing.history.view')
) AS v (PermissionId)
WHERE existing.PermissionId = 'content.item.update'
  AND NOT EXISTS (
    SELECT 1 FROM dbo.AuthorityRolePermissions already
    WHERE already.RoleKey = existing.RoleKey AND already.PermissionId = v.PermissionId);
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

-- Migration 013 indexed this column for the auto-reset sweep. The index has to
-- go first or the column cannot be dropped.
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_MenuItems_AvailabilityResetUtc'
      AND object_id = OBJECT_ID('dbo.MenuItems'))
    DROP INDEX IX_MenuItems_AvailabilityResetUtc ON dbo.MenuItems;
GO

IF COL_LENGTH('dbo.MenuItems', 'AvailabilityResetUtc') IS NOT NULL
    ALTER TABLE dbo.MenuItems DROP COLUMN AvailabilityResetUtc;
GO
