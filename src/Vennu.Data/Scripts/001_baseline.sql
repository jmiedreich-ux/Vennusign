-- Vennusign schema baseline.
--
-- This is scripts 001 through 058 in the order DbUp applied them, collapsed into one
-- file. It is not a rewrite: every statement below is a statement that already ran, so
-- a database built from this file and one built from the old chain are the same
-- database - and that is proved by diffing the two, not assumed.
--
-- Two pieces of dead work are gone. Script 012 created dbo.MenuItemTranslations and 058
-- dropped it; script 013 added MenuItems.AvailabilityResetUtc with its index and 058
-- dropped those too. A fresh database used to build both and then demolish them.
-- Removing each pair changes no end state.
--
-- A database that already ran the old chain never runs this file: DatabaseMigrator
-- records it as applied when it finds the old scripts in SchemaVersions, so an existing
-- database is left exactly as it is. See DatabaseMigrator.Run.
--
-- One knowingly accepted difference: eleven tables declare DEFAULT NEWID() inline
-- without naming the constraint, so SQL Server generates a name from the object id.
-- Creating one table fewer shifts those ids, so a database built from this baseline
-- carries different DF__ names than one built from the old chain. The columns,
-- defaults and every other object are identical - verified by diffing the two schemas
-- - and nothing in the codebase refers to a generated constraint name.
--
-- New migrations continue from 059.

GO

-- ===== 001_create_venues.sql =====

CREATE TABLE dbo.Venues
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Venues PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Timezone NVARCHAR(100) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    PrimaryLanguage NVARCHAR(10) NOT NULL,
    SecondaryLanguage NVARCHAR(10) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Venues_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Venues_UpdatedUtc DEFAULT SYSUTCDATETIME()
);
GO

GO

-- ===== 002_create_screens.sql =====

CREATE TABLE dbo.Screens
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Screens PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NULL,
    ScreenKey NVARCHAR(9) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Location NVARCHAR(200) NULL,
    WallGroup NVARCHAR(100) NULL,
    WallPosition INT NULL,
    LastSeen DATETIME2(7) NULL,
    Status NVARCHAR(30) NOT NULL CONSTRAINT DF_Screens_Status DEFAULT 'Offline',
    Platform NVARCHAR(50) NULL,
    AppVersion NVARCHAR(50) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Screens_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Screens_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Screens_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id)
);
GO

CREATE UNIQUE INDEX UX_Screens_ScreenKey ON dbo.Screens (ScreenKey);
GO

CREATE INDEX IX_Screens_VenueId ON dbo.Screens (VenueId);
GO

GO

-- ===== 003_create_screen_pairing_codes.sql =====

CREATE TABLE dbo.ScreenPairingCodes
(
    Code CHAR(6) NOT NULL CONSTRAINT PK_ScreenPairingCodes PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NULL,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    ExpiresAt DATETIME2(7) NOT NULL,
    IsClaimed BIT NOT NULL CONSTRAINT DF_ScreenPairingCodes_IsClaimed DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_ScreenPairingCodes_CreatedUtc DEFAULT SYSUTCDATETIME(),
    ClaimedAt DATETIME2(7) NULL,
    CONSTRAINT FK_ScreenPairingCodes_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_ScreenPairingCodes_Screens FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id)
);
GO

CREATE INDEX IX_ScreenPairingCodes_ScreenId ON dbo.ScreenPairingCodes (ScreenId);
GO

CREATE INDEX IX_ScreenPairingCodes_ExpiresAt_IsClaimed ON dbo.ScreenPairingCodes (ExpiresAt, IsClaimed);
GO

GO

-- ===== 004_create_feature_tier_core.sql =====

CREATE TABLE dbo.Features
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Features PRIMARY KEY CONSTRAINT DF_Features_Id DEFAULT NEWID(),
    [Key] NVARCHAR(100) NOT NULL,
    Label NVARCHAR(150) NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Features_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Features_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Features_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Features_Key UNIQUE ([Key])
);
GO

CREATE TABLE dbo.SubscriptionTiers
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SubscriptionTiers PRIMARY KEY CONSTRAINT DF_SubscriptionTiers_Id DEFAULT NEWID(),
    Name NVARCHAR(150) NOT NULL,
    Slug NVARCHAR(100) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    MaxScreens INT NOT NULL,
    IsPublic BIT NOT NULL CONSTRAINT DF_SubscriptionTiers_IsPublic DEFAULT 1,
    IsActive BIT NOT NULL CONSTRAINT DF_SubscriptionTiers_IsActive DEFAULT 1,
    StripeProductId NVARCHAR(100) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SubscriptionTiers_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SubscriptionTiers_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_SubscriptionTiers_Slug UNIQUE (Slug),
    CONSTRAINT CK_SubscriptionTiers_MaxScreens CHECK (MaxScreens = -1 OR MaxScreens > 0),
    CONSTRAINT CK_SubscriptionTiers_Price CHECK (Price >= 0)
);
GO

CREATE TABLE dbo.TierFeatures
(
    TierId UNIQUEIDENTIFIER NOT NULL,
    FeatureId UNIQUEIDENTIFIER NOT NULL,
    LimitValue NVARCHAR(100) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TierFeatures_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TierFeatures PRIMARY KEY (TierId, FeatureId),
    CONSTRAINT FK_TierFeatures_SubscriptionTiers FOREIGN KEY (TierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT FK_TierFeatures_Features FOREIGN KEY (FeatureId) REFERENCES dbo.Features (Id)
);
GO

CREATE INDEX IX_TierFeatures_FeatureId ON dbo.TierFeatures (FeatureId);
GO

CREATE TABLE dbo.VenueSubscriptions
(
    VenueId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VenueSubscriptions PRIMARY KEY,
    TierId UNIQUEIDENTIFIER NOT NULL,
    StripeSubscriptionId NVARCHAR(100) NULL,
    Status NVARCHAR(30) NOT NULL,
    TrialEndsAt DATETIME2(7) NULL,
    CurrentPeriodEnd DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueSubscriptions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueSubscriptions_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_VenueSubscriptions_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_VenueSubscriptions_SubscriptionTiers FOREIGN KEY (TierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT CK_VenueSubscriptions_Status CHECK (Status IN ('trialing', 'active', 'past_due', 'canceled'))
);
GO

CREATE UNIQUE INDEX UX_VenueSubscriptions_StripeSubscriptionId
    ON dbo.VenueSubscriptions (StripeSubscriptionId)
    WHERE StripeSubscriptionId IS NOT NULL;
GO

CREATE INDEX IX_VenueSubscriptions_TierId_Status ON dbo.VenueSubscriptions (TierId, Status);
GO

DECLARE @Starter UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000001';
DECLARE @RestaurantStarter UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000002';
DECLARE @Pro UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000003';
DECLARE @Business UNIQUEIDENTIFIER = '10000000-0000-0000-0000-000000000004';

INSERT dbo.SubscriptionTiers (Id, Name, Slug, Price, MaxScreens, IsPublic, IsActive)
VALUES
(@Starter, 'Starter', 'starter', 39.00, 2, 1, 1),
(@RestaurantStarter, 'Restaurant Starter', 'restaurant_starter', 49.00, 1, 1, 1),
(@Pro, 'Pro', 'pro', 89.00, 6, 1, 1),
(@Business, 'Business', 'business', 179.00, -1, 1, 1);
GO

INSERT dbo.Features (Id, [Key], Label, Category, IsActive)
VALUES
('20000000-0000-0000-0000-000000000001', 'photo_grid', 'Photo Grid', 'layouts', 1),
('20000000-0000-0000-0000-000000000002', 'classic_diner', 'Classic Diner', 'layouts', 1),
('20000000-0000-0000-0000-000000000003', 'basic_scheduling', 'Basic Scheduling', 'scheduling', 1),
('20000000-0000-0000-0000-000000000004', 'allergen_badges', 'Allergen Badges', 'content', 1),
('20000000-0000-0000-0000-000000000005', 'analytics', 'Analytics', 'analytics', 1),
('20000000-0000-0000-0000-000000000006', 'meal_periods', 'Meal Periods', 'scheduling', 1),
('20000000-0000-0000-0000-000000000007', 'bilingual_display', 'Bilingual Display', 'localization', 1),
('20000000-0000-0000-0000-000000000008', 'ai_translation', 'AI Translation', 'ai', 1),
('20000000-0000-0000-0000-000000000009', 'quick_update', 'Quick Update', 'operations', 1),
('20000000-0000-0000-0000-000000000010', 'all_layouts', 'All Layouts', 'layouts', 1),
('20000000-0000-0000-0000-000000000011', 'happy_hour', 'Happy Hour', 'scheduling', 1),
('20000000-0000-0000-0000-000000000012', 'pos_integration', 'POS Integration', 'integrations', 1),
('20000000-0000-0000-0000-000000000013', 'staff_app', 'Staff App', 'operations', 1),
('20000000-0000-0000-0000-000000000014', 'ai_custom_builder', 'AI Custom Builder', 'ai', 1),
('20000000-0000-0000-0000-000000000015', 'multi_location', 'Multi-location', 'operations', 1),
('20000000-0000-0000-0000-000000000016', 'white_label', 'White Label', 'branding', 1),
('20000000-0000-0000-0000-000000000017', 'html_editor', 'HTML Editor', 'content', 1);
GO

INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue)
SELECT '10000000-0000-0000-0000-000000000001', Id, NULL FROM dbo.Features WHERE [Key] IN ('photo_grid','classic_diner','basic_scheduling','allergen_badges','analytics');
INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue)
SELECT '10000000-0000-0000-0000-000000000002', Id, CASE WHEN [Key] = 'ai_translation' THEN '1' ELSE NULL END FROM dbo.Features WHERE [Key] IN ('photo_grid','classic_diner','basic_scheduling','allergen_badges','analytics','meal_periods','bilingual_display','ai_translation','quick_update');
INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue)
SELECT '10000000-0000-0000-0000-000000000003', Id, NULL FROM dbo.Features WHERE [Key] IN ('photo_grid','classic_diner','basic_scheduling','allergen_badges','analytics','meal_periods','bilingual_display','ai_translation','quick_update','all_layouts','happy_hour','pos_integration','staff_app');
INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue)
SELECT '10000000-0000-0000-0000-000000000004', Id, NULL FROM dbo.Features;
GO

GO

-- ===== 005_create_venue_feature_overrides.sql =====

CREATE TABLE dbo.VenueFeatureOverrides
(
    VenueId UNIQUEIDENTIFIER NOT NULL,
    FeatureId UNIQUEIDENTIFIER NOT NULL,
    Enabled BIT NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2(7) NULL,
    CreatedByAdminId UNIQUEIDENTIFIER NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueFeatureOverrides_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_VenueFeatureOverrides PRIMARY KEY (VenueId, FeatureId),
    CONSTRAINT FK_VenueFeatureOverrides_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_VenueFeatureOverrides_Features FOREIGN KEY (FeatureId) REFERENCES dbo.Features (Id),
    CONSTRAINT CK_VenueFeatureOverrides_Reason CHECK (LEN(LTRIM(RTRIM(Reason))) > 0)
);
GO

CREATE INDEX IX_VenueFeatureOverrides_ExpiresAt ON dbo.VenueFeatureOverrides (ExpiresAt) INCLUDE (VenueId, FeatureId, Enabled);
GO

GO

-- ===== 006_create_feature_usages.sql =====

CREATE TABLE dbo.FeatureUsages
(
    VenueId UNIQUEIDENTIFIER NOT NULL,
    FeatureId UNIQUEIDENTIFIER NOT NULL,
    PeriodStartUtc DATETIME2(7) NOT NULL,
    UsageCount INT NOT NULL CONSTRAINT DF_FeatureUsages_UsageCount DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_FeatureUsages_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_FeatureUsages_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_FeatureUsages PRIMARY KEY (VenueId, FeatureId, PeriodStartUtc),
    CONSTRAINT FK_FeatureUsages_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_FeatureUsages_Features FOREIGN KEY (FeatureId) REFERENCES dbo.Features (Id),
    CONSTRAINT CK_FeatureUsages_UsageCount CHECK (UsageCount >= 0),
    CONSTRAINT CK_FeatureUsages_PeriodStartUtc CHECK (
        DAY(PeriodStartUtc) = 1
        AND CONVERT(TIME, PeriodStartUtc) = '00:00:00'
    )
);
GO

CREATE INDEX IX_FeatureUsages_FeatureId_PeriodStartUtc
    ON dbo.FeatureUsages (FeatureId, PeriodStartUtc);
GO

GO

-- ===== 007_add_stripe_billing_catalog.sql =====

ALTER TABLE dbo.SubscriptionTiers
ADD StripeMonthlyPriceId NVARCHAR(100) NULL,
    StripeAnnualPriceId NVARCHAR(100) NULL;
GO

CREATE UNIQUE INDEX UX_SubscriptionTiers_StripeProductId
    ON dbo.SubscriptionTiers (StripeProductId)
    WHERE StripeProductId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_SubscriptionTiers_StripeMonthlyPriceId
    ON dbo.SubscriptionTiers (StripeMonthlyPriceId)
    WHERE StripeMonthlyPriceId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_SubscriptionTiers_StripeAnnualPriceId
    ON dbo.SubscriptionTiers (StripeAnnualPriceId)
    WHERE StripeAnnualPriceId IS NOT NULL;
GO

GO

-- ===== 008_create_processed_stripe_events.sql =====

CREATE TABLE dbo.ProcessedStripeEvents
(
    EventId NVARCHAR(255) NOT NULL CONSTRAINT PK_ProcessedStripeEvents PRIMARY KEY,
    EventType NVARCHAR(100) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    StartedUtc DATETIME2(7) NOT NULL,
    ProcessedUtc DATETIME2(7) NULL,
    FailureReason NVARCHAR(500) NULL,
    CONSTRAINT CK_ProcessedStripeEvents_Status CHECK (Status IN ('processing', 'processed', 'failed'))
);
GO

CREATE INDEX IX_ProcessedStripeEvents_Status_StartedUtc
    ON dbo.ProcessedStripeEvents (Status, StartedUtc);
GO

GO

-- ===== 009_create_feature_matrix_audit.sql =====

CREATE TABLE dbo.FeatureMatrixAudit
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FeatureMatrixAudit PRIMARY KEY,
    TierId UNIQUEIDENTIFIER NOT NULL,
    FeatureId UNIQUEIDENTIFIER NOT NULL,
    AdminId NVARCHAR(150) NOT NULL,
    PreviousEnabled BIT NOT NULL,
    NewEnabled BIT NOT NULL,
    ChangedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_FeatureMatrixAudit_SubscriptionTiers
        FOREIGN KEY (TierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT FK_FeatureMatrixAudit_Features
        FOREIGN KEY (FeatureId) REFERENCES dbo.Features (Id),
    CONSTRAINT CK_FeatureMatrixAudit_ValueChanged
        CHECK (PreviousEnabled <> NewEnabled)
);
GO

CREATE INDEX IX_FeatureMatrixAudit_ChangedUtc
    ON dbo.FeatureMatrixAudit (ChangedUtc DESC)
    INCLUDE (TierId, FeatureId, AdminId, PreviousEnabled, NewEnabled);
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplyFeatureMatrixChanges
    @ChangesJson NVARCHAR(MAX),
    @AdminId NVARCHAR(150),
    @ChangedUtc DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF ISJSON(@ChangesJson) <> 1
        THROW 50001, 'Feature matrix changes must be valid JSON.', 1;

    IF NULLIF(LTRIM(RTRIM(@AdminId)), '') IS NULL
        THROW 50002, 'Admin identifier is required.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        CREATE TABLE #RequestedChanges
        (
            TierId UNIQUEIDENTIFIER NOT NULL,
            FeatureId UNIQUEIDENTIFIER NOT NULL,
            Enabled BIT NOT NULL,
            PRIMARY KEY (TierId, FeatureId)
        );

        INSERT #RequestedChanges (TierId, FeatureId, Enabled)
        SELECT TierId, FeatureId, Enabled
        FROM OPENJSON(@ChangesJson)
        WITH
        (
            TierId UNIQUEIDENTIFIER '$.tierId',
            FeatureId UNIQUEIDENTIFIER '$.featureId',
            Enabled BIT '$.enabled'
        );

        IF EXISTS
        (
            SELECT 1
            FROM #RequestedChanges AS requested
            LEFT JOIN dbo.SubscriptionTiers AS tier ON tier.Id = requested.TierId
            LEFT JOIN dbo.Features AS feature ON feature.Id = requested.FeatureId AND feature.IsActive = 1
            WHERE tier.Id IS NULL OR feature.Id IS NULL
        )
            THROW 50003, 'Feature matrix changes contain an unknown tier or inactive feature.', 1;

        CREATE TABLE #EffectiveChanges
        (
            TierId UNIQUEIDENTIFIER NOT NULL,
            FeatureId UNIQUEIDENTIFIER NOT NULL,
            PreviousEnabled BIT NOT NULL,
            NewEnabled BIT NOT NULL,
            PRIMARY KEY (TierId, FeatureId)
        );

        INSERT #EffectiveChanges (TierId, FeatureId, PreviousEnabled, NewEnabled)
        SELECT
            requested.TierId,
            requested.FeatureId,
            CONVERT(BIT, CASE WHEN currentValue.TierId IS NULL THEN 0 ELSE 1 END),
            requested.Enabled
        FROM #RequestedChanges AS requested
        LEFT JOIN dbo.TierFeatures AS currentValue WITH (UPDLOCK, HOLDLOCK)
            ON currentValue.TierId = requested.TierId
           AND currentValue.FeatureId = requested.FeatureId
        WHERE (CASE WHEN currentValue.TierId IS NULL THEN 0 ELSE 1 END) <> requested.Enabled;

        INSERT dbo.FeatureMatrixAudit
            (Id, TierId, FeatureId, AdminId, PreviousEnabled, NewEnabled, ChangedUtc)
        SELECT NEWID(), TierId, FeatureId, @AdminId, PreviousEnabled, NewEnabled, @ChangedUtc
        FROM #EffectiveChanges;

        DELETE tierFeature
        FROM dbo.TierFeatures AS tierFeature
        INNER JOIN #EffectiveChanges AS change
            ON change.TierId = tierFeature.TierId
           AND change.FeatureId = tierFeature.FeatureId
        WHERE change.NewEnabled = 0;

        INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue, CreatedUtc)
        SELECT TierId, FeatureId, NULL, @ChangedUtc
        FROM #EffectiveChanges
        WHERE NewEnabled = 1;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT COUNT(*) AS ChangedCount
    FROM #EffectiveChanges;
END;
GO

GO

-- ===== 010_create_operational_events.sql =====

CREATE TABLE dbo.OperationalEvents
(
    Id UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    Summary NVARCHAR(1000) NOT NULL,
    OccurredUtc DATETIME2 NOT NULL,
    CONSTRAINT PK_OperationalEvents PRIMARY KEY (Id),
    CONSTRAINT FK_OperationalEvents_Venues
        FOREIGN KEY (VenueId) REFERENCES dbo.Venues(Id)
);

CREATE INDEX IX_OperationalEvents_OccurredUtc
    ON dbo.OperationalEvents (OccurredUtc DESC)
    INCLUDE (VenueId, EventType, Summary);

GO

-- ===== 011_create_revenue_daily_snapshots.sql =====

CREATE TABLE dbo.RevenueDailySnapshots
(
    SnapshotDateUtc DATE NOT NULL,
    Currency CHAR(3) NOT NULL,
    Mrr DECIMAL(19, 2) NOT NULL,
    Arr DECIMAL(19, 2) NOT NULL,
    AverageRevenuePerActiveSubscription DECIMAL(19, 2) NOT NULL,
    ActiveSubscriptions INT NOT NULL,
    CapturedUtc DATETIME2 NOT NULL,
    CONSTRAINT PK_RevenueDailySnapshots PRIMARY KEY (SnapshotDateUtc),
    CONSTRAINT CK_RevenueDailySnapshots_Currency CHECK (Currency = 'USD'),
    CONSTRAINT CK_RevenueDailySnapshots_Mrr CHECK (Mrr >= 0),
    CONSTRAINT CK_RevenueDailySnapshots_Arr CHECK (Arr >= 0),
    CONSTRAINT CK_RevenueDailySnapshots_AverageRevenue CHECK (AverageRevenuePerActiveSubscription >= 0),
    CONSTRAINT CK_RevenueDailySnapshots_ActiveSubscriptions CHECK (ActiveSubscriptions >= 0)
);

GO

-- ===== 012_create_menu_domain.sql =====

CREATE TABLE dbo.Menus
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Menus PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Menus_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Menus_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Menus_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Menus_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_Menus_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT CK_Menus_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
);
GO

CREATE INDEX IX_Menus_VenueId_Name ON dbo.Menus (VenueId, Name, Id);
GO

CREATE TABLE dbo.MenuSections
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuSections PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    SortOrder INT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_MenuSections_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuSections_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuSections_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MenuSections_Menus FOREIGN KEY (MenuId, VenueId)
        REFERENCES dbo.Menus (Id, VenueId) ON DELETE CASCADE,
    CONSTRAINT UQ_MenuSections_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_MenuSections_MenuId_SortOrder UNIQUE (MenuId, SortOrder),
    CONSTRAINT CK_MenuSections_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_MenuSections_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_MenuSections_VenueId_MenuId_Order
    ON dbo.MenuSections (VenueId, MenuId, SortOrder, Id);
GO

CREATE TABLE dbo.MenuItems
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuItems PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuSectionId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(19, 4) NOT NULL,
    HappyHourPrice DECIMAL(19, 4) NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_MenuItems_IsAvailable DEFAULT 1,
    QuantityAvailable INT NULL,
    Tags NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(2048) NULL,
    IsPopular BIT NOT NULL CONSTRAINT DF_MenuItems_IsPopular DEFAULT 0,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuItems_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuItems_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MenuItems_MenuSections FOREIGN KEY (MenuSectionId, VenueId)
        REFERENCES dbo.MenuSections (Id, VenueId) ON DELETE CASCADE,
    CONSTRAINT UQ_MenuItems_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_MenuItems_SectionId_SortOrder UNIQUE (MenuSectionId, SortOrder),
    CONSTRAINT CK_MenuItems_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_MenuItems_Price_NonNegative CHECK (Price >= 0),
    CONSTRAINT CK_MenuItems_HappyHourPrice_NonNegative CHECK (HappyHourPrice IS NULL OR HappyHourPrice >= 0),
    CONSTRAINT CK_MenuItems_QuantityAvailable_NonNegative CHECK (QuantityAvailable IS NULL OR QuantityAvailable >= 0),
    CONSTRAINT CK_MenuItems_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_MenuItems_VenueId_SectionId_Order
    ON dbo.MenuItems (VenueId, MenuSectionId, SortOrder, Id);
GO

GO

-- ===== 013_add_quick_update.sql =====

ALTER TABLE dbo.Menus
ADD DailySpecial NVARCHAR(240) NULL;
GO

GO

-- ===== 014_add_video_wall_feature.sql =====

IF NOT EXISTS (SELECT 1 FROM dbo.Features WHERE [Key] = 'video_wall')
BEGIN
    INSERT dbo.Features (Id, [Key], Label, Category, IsActive)
    VALUES ('20000000-0000-0000-0000-000000000018', 'video_wall', 'Video Wall', 'layouts', 1);
END;
GO

INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue)
SELECT tier.Id, feature.Id, NULL
FROM dbo.SubscriptionTiers tier
CROSS JOIN dbo.Features feature
WHERE tier.Slug IN ('pro', 'business')
  AND feature.[Key] = 'video_wall'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.TierFeatures existing
      WHERE existing.TierId = tier.Id
        AND existing.FeatureId = feature.Id
  );
GO

GO

-- ===== 015_add_photo_grid_density.sql =====

ALTER TABLE dbo.Screens
ADD PhotoGridDensity NVARCHAR(3) NOT NULL
    CONSTRAINT DF_Screens_PhotoGridDensity DEFAULT '3x2';
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_PhotoGridDensity
    CHECK (PhotoGridDensity IN ('2x2', '3x2', '4x2', '3x3'));
GO

GO

-- ===== 016_add_screen_display_layout.sql =====

ALTER TABLE dbo.Screens
ADD DisplayLayout NVARCHAR(30) NOT NULL
    CONSTRAINT DF_Screens_DisplayLayout DEFAULT 'photo_grid';
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
    CHECK (DisplayLayout IN ('photo_grid', 'classic_diner'));
GO

GO

-- ===== 017_create_venue_themes.sql =====

CREATE TABLE dbo.VenueThemes
(
    VenueId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VenueThemes PRIMARY KEY,
    BackgroundColor CHAR(7) NOT NULL,
    AccentColor CHAR(7) NOT NULL,
    FontFamily NVARCHAR(50) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueThemes_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_VenueThemes_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT CK_VenueThemes_BackgroundColor CHECK (BackgroundColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_AccentColor CHECK (AccentColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_FontFamily CHECK (FontFamily IN ('Inter', 'Georgia', 'Arial'))
);
GO

GO

-- ===== 018_add_advanced_venue_themes.sql =====

ALTER TABLE dbo.VenueThemes
ADD PresetKey NVARCHAR(30) NOT NULL
        CONSTRAINT DF_VenueThemes_PresetKey DEFAULT 'bar_classic',
    TitleColor CHAR(7) NOT NULL
        CONSTRAINT DF_VenueThemes_TitleColor DEFAULT '#F8F5E9',
    GlowColor CHAR(7) NOT NULL
        CONSTRAINT DF_VenueThemes_GlowColor DEFAULT '#00E5FF',
    BoardBackgroundColor CHAR(7) NOT NULL
        CONSTRAINT DF_VenueThemes_BoardBackgroundColor DEFAULT '#071013',
    SectionColors NVARCHAR(64) NOT NULL
        CONSTRAINT DF_VenueThemes_SectionColors DEFAULT '#00E5FF,#FF2BD6,#FFE66D,#7CFF6B',
    GlowIntensity DECIMAL(3, 2) NOT NULL
        CONSTRAINT DF_VenueThemes_GlowIntensity DEFAULT 1.00,
    TitleFont NVARCHAR(40) NOT NULL
        CONSTRAINT DF_VenueThemes_TitleFont DEFAULT 'Righteous',
    ItemFont NVARCHAR(40) NOT NULL
        CONSTRAINT DF_VenueThemes_ItemFont DEFAULT 'Caveat';
GO

ALTER TABLE dbo.VenueThemes
ADD CONSTRAINT CK_VenueThemes_PresetKey
        CHECK (PresetKey IN ('custom', 'bar_classic', 'violet_lounge', 'hot_summer', 'ocean_dive', 'rose_gold')),
    CONSTRAINT CK_VenueThemes_TitleColor
        CHECK (TitleColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_GlowColor
        CHECK (GlowColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_BoardBackgroundColor
        CHECK (BoardBackgroundColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_SectionColors
        CHECK (LEN(SectionColors) BETWEEN 7 AND 64),
    CONSTRAINT CK_VenueThemes_GlowIntensity
        CHECK (GlowIntensity BETWEEN 0.20 AND 2.00),
    CONSTRAINT CK_VenueThemes_TitleFont
        CHECK (TitleFont IN ('Pacifico', 'Lobster', 'Righteous', 'Fredoka One', 'Bungee', 'Permanent Marker')),
    CONSTRAINT CK_VenueThemes_ItemFont
        CHECK (ItemFont IN ('Caveat', 'Kalam', 'Patrick Hand', 'Permanent Marker'));
GO

GO

-- ===== 019_add_split_layout.sql =====

ALTER TABLE dbo.Screens
DROP CONSTRAINT CK_Screens_DisplayLayout;
GO

ALTER TABLE dbo.Screens
ADD SplitRatio NVARCHAR(5) NOT NULL
    CONSTRAINT DF_Screens_SplitRatio DEFAULT '40_60';
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
        CHECK (DisplayLayout IN ('photo_grid', 'classic_diner', 'neon_chalkboard', 'split_layout')),
    CONSTRAINT CK_Screens_SplitRatio
        CHECK (SplitRatio IN ('40_60', '50_50'));
GO

GO

-- ===== 020_add_daily_special_hero.sql =====

ALTER TABLE dbo.Screens
DROP CONSTRAINT CK_Screens_DisplayLayout;
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
    CHECK (DisplayLayout IN (
        'photo_grid',
        'classic_diner',
        'neon_chalkboard',
        'split_layout',
        'daily_special_hero'
    ));
GO

GO

-- ===== 021_add_hero_dwell_seconds.sql =====

ALTER TABLE dbo.Screens
ADD HeroDwellSeconds INT NOT NULL
    CONSTRAINT DF_Screens_HeroDwellSeconds DEFAULT 8,
    CONSTRAINT CK_Screens_HeroDwellSeconds CHECK (HeroDwellSeconds BETWEEN 4 AND 30);
GO

GO

-- ===== 022_create_meal_periods.sql =====

CREATE TABLE dbo.MealPeriods
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MealPeriods PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    StartLocalTime TIME(0) NOT NULL,
    EndLocalTime TIME(0) NOT NULL,
    ActiveDaysMask INT NOT NULL CONSTRAINT DF_MealPeriods_ActiveDaysMask DEFAULT 127,
    IsEnabled BIT NOT NULL CONSTRAINT DF_MealPeriods_IsEnabled DEFAULT 1,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MealPeriods_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MealPeriods_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MealPeriods_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_MealPeriods_VenueId_Name UNIQUE (VenueId, Name),
    CONSTRAINT UQ_MealPeriods_VenueId_SortOrder UNIQUE (VenueId, SortOrder),
    CONSTRAINT CK_MealPeriods_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_MealPeriods_DistinctTimes CHECK (StartLocalTime <> EndLocalTime),
    CONSTRAINT CK_MealPeriods_ActiveDaysMask CHECK (ActiveDaysMask BETWEEN 1 AND 127),
    CONSTRAINT CK_MealPeriods_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_MealPeriods_VenueId_Enabled_Order
    ON dbo.MealPeriods (VenueId, IsEnabled, SortOrder, Id);
GO

GO

-- ===== 023_add_meal_period_targets.sql =====

ALTER TABLE dbo.MealPeriods
ADD TargetLayout NVARCHAR(50) NULL,
    MenuFilter NVARCHAR(100) NULL,
    ThemePresetKey NVARCHAR(50) NULL;
GO

ALTER TABLE dbo.MealPeriods
ADD CONSTRAINT CK_MealPeriods_TargetLayout CHECK
(
    TargetLayout IS NULL OR TargetLayout IN
    ('photo_grid', 'classic_diner', 'neon_chalkboard', 'split_layout', 'daily_special_hero')
);
GO

GO

-- ===== 024_create_happy_hour_schedules.sql =====

CREATE TABLE dbo.HappyHourSchedules
(
    VenueId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HappyHourSchedules PRIMARY KEY,
    StartLocalTime TIME(0) NOT NULL,
    EndLocalTime TIME(0) NOT NULL,
    ActiveDaysMask INT NOT NULL CONSTRAINT DF_HappyHourSchedules_ActiveDaysMask DEFAULT 127,
    IsEnabled BIT NOT NULL CONSTRAINT DF_HappyHourSchedules_IsEnabled DEFAULT 1,
    OverrideMode NVARCHAR(20) NOT NULL CONSTRAINT DF_HappyHourSchedules_OverrideMode DEFAULT 'automatic',
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_HappyHourSchedules_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_HappyHourSchedules_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT CK_HappyHourSchedules_DistinctTimes CHECK (StartLocalTime <> EndLocalTime),
    CONSTRAINT CK_HappyHourSchedules_ActiveDaysMask CHECK (ActiveDaysMask BETWEEN 1 AND 127),
    CONSTRAINT CK_HappyHourSchedules_OverrideMode CHECK (OverrideMode IN ('automatic', 'force_on', 'force_off'))
);
GO

GO

-- ===== 025_create_playlist_slides.sql =====

CREATE UNIQUE INDEX UX_Screens_VenueId_Id ON dbo.Screens (VenueId, Id);
GO

CREATE TABLE dbo.PlaylistSlides
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PlaylistSlides PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    SlideType NVARCHAR(20) NOT NULL,
    Title NVARCHAR(200) NULL,
    Body NVARCHAR(1000) NULL,
    MediaUrl NVARCHAR(1000) NULL,
    DwellSeconds INT NOT NULL CONSTRAINT DF_PlaylistSlides_DwellSeconds DEFAULT 10,
    StartLocalTime TIME(0) NULL,
    EndLocalTime TIME(0) NULL,
    ActiveDaysMask INT NULL,
    IsEnabled BIT NOT NULL CONSTRAINT DF_PlaylistSlides_IsEnabled DEFAULT 1,
    SortOrder INT NOT NULL CONSTRAINT DF_PlaylistSlides_SortOrder DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PlaylistSlides_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PlaylistSlides_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PlaylistSlides_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT FK_PlaylistSlides_Screens FOREIGN KEY (VenueId, ScreenId) REFERENCES dbo.Screens (VenueId, Id) ON DELETE CASCADE,
    CONSTRAINT CK_PlaylistSlides_Type CHECK (SlideType IN ('menu', 'image', 'message')),
    CONSTRAINT CK_PlaylistSlides_Dwell CHECK (DwellSeconds BETWEEN 5 AND 120),
    CONSTRAINT CK_PlaylistSlides_Window CHECK
    (
        (StartLocalTime IS NULL AND EndLocalTime IS NULL AND ActiveDaysMask IS NULL)
        OR
        (StartLocalTime IS NOT NULL AND EndLocalTime IS NOT NULL AND StartLocalTime <> EndLocalTime AND ActiveDaysMask BETWEEN 1 AND 127)
    )
);
GO

CREATE INDEX IX_PlaylistSlides_Screen_SortOrder ON dbo.PlaylistSlides (VenueId, ScreenId, SortOrder, Id);
GO

GO

-- ===== 026_create_emergency_broadcasts.sql =====

CREATE TABLE dbo.EmergencyBroadcasts
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmergencyBroadcasts PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ScreenId UNIQUEIDENTIFIER NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(2000) NOT NULL,
    MediaUrl NVARCHAR(1000) NULL,
    StartsUtc DATETIME2(7) NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_EmergencyBroadcasts_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmergencyBroadcasts_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmergencyBroadcasts_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_EmergencyBroadcasts_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT FK_EmergencyBroadcasts_Screens FOREIGN KEY (VenueId, ScreenId) REFERENCES dbo.Screens (VenueId, Id) ON DELETE CASCADE,
    CONSTRAINT CK_EmergencyBroadcasts_Duration CHECK (ExpiresUtc > StartsUtc AND DATEDIFF(MINUTE, StartsUtc, ExpiresUtc) BETWEEN 1 AND 1440)
);
GO

CREATE INDEX IX_EmergencyBroadcasts_Active
    ON dbo.EmergencyBroadcasts (VenueId, ScreenId, IsActive, StartsUtc, ExpiresUtc);
GO

GO

-- ===== 027_create_date_range_promotions.sql =====

CREATE TABLE dbo.DateRangePromotions
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DateRangePromotions PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    StartLocalDate DATE NOT NULL,
    EndLocalDate DATE NOT NULL,
    TargetLayout NVARCHAR(80) NULL,
    Title NVARCHAR(200) NULL,
    Body NVARCHAR(1000) NULL,
    Priority INT NOT NULL CONSTRAINT DF_DateRangePromotions_Priority DEFAULT 0,
    IsEnabled BIT NOT NULL CONSTRAINT DF_DateRangePromotions_IsEnabled DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_DateRangePromotions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_DateRangePromotions_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_DateRangePromotions_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT CK_DateRangePromotions_DateRange CHECK (EndLocalDate >= StartLocalDate),
    CONSTRAINT CK_DateRangePromotions_Priority CHECK (Priority BETWEEN -1000 AND 1000)
);
GO

CREATE INDEX IX_DateRangePromotions_Resolution
    ON dbo.DateRangePromotions (VenueId, IsEnabled, StartLocalDate, EndLocalDate, Priority DESC);
GO

GO

-- ===== 028_create_tap_domain.sql =====

CREATE TABLE dbo.TapCategories
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TapCategories PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    CategoryPrice DECIMAL(19, 4) NULL,
    SortOrder INT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_TapCategories_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapCategories_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapCategories_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TapCategories_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TapCategories_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_TapCategories_VenueId_SortOrder UNIQUE (VenueId, SortOrder),
    CONSTRAINT CK_TapCategories_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_TapCategories_Price_NonNegative CHECK (CategoryPrice IS NULL OR CategoryPrice >= 0),
    CONSTRAINT CK_TapCategories_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_TapCategories_VenueId_Order ON dbo.TapCategories (VenueId, SortOrder, Id);
GO

CREATE TABLE dbo.TapItems
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TapItems PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    TapCategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Style NVARCHAR(160) NULL,
    Abv DECIMAL(5, 2) NULL,
    Ibu INT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(19, 4) NOT NULL,
    GlassColor CHAR(7) NULL,
    NameColor CHAR(7) NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_TapItems_IsAvailable DEFAULT 1,
    IsComingSoon BIT NOT NULL CONSTRAINT DF_TapItems_IsComingSoon DEFAULT 0,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapItems_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapItems_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TapItems_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_TapItems_Categories FOREIGN KEY (TapCategoryId, VenueId)
        REFERENCES dbo.TapCategories (Id, VenueId),
    CONSTRAINT UQ_TapItems_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_TapItems_VenueId_SortOrder UNIQUE (VenueId, SortOrder),
    CONSTRAINT CK_TapItems_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_TapItems_Abv_Range CHECK (Abv IS NULL OR Abv BETWEEN 0 AND 100),
    CONSTRAINT CK_TapItems_Ibu_Range CHECK (Ibu IS NULL OR Ibu BETWEEN 0 AND 1000),
    CONSTRAINT CK_TapItems_Price_NonNegative CHECK (Price >= 0),
    CONSTRAINT CK_TapItems_GlassColor CHECK (GlassColor IS NULL OR GlassColor LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'),
    CONSTRAINT CK_TapItems_NameColor CHECK (NameColor IS NULL OR NameColor LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'),
    CONSTRAINT CK_TapItems_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_TapItems_VenueId_Order ON dbo.TapItems (VenueId, SortOrder, Id);
GO

GO

-- ===== 029_add_classic_chalkboard_layout.sql =====

ALTER TABLE dbo.Screens
DROP CONSTRAINT CK_Screens_DisplayLayout;
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
    CHECK (DisplayLayout IN (
        'photo_grid',
        'classic_diner',
        'neon_chalkboard',
        'split_layout',
        'daily_special_hero',
        'classic_chalkboard'
    ));
GO

GO

-- ===== 030_add_tap_strips_layout.sql =====

ALTER TABLE dbo.Screens
DROP CONSTRAINT CK_Screens_DisplayLayout;
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
    CHECK (DisplayLayout IN (
        'photo_grid',
        'classic_diner',
        'neon_chalkboard',
        'split_layout',
        'daily_special_hero',
        'classic_chalkboard',
        'tap_strips'
    ));
GO

GO

-- ===== 031_add_digital_tap_board_layout.sql =====

ALTER TABLE dbo.Screens
DROP CONSTRAINT CK_Screens_DisplayLayout;
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
    CHECK (DisplayLayout IN (
        'photo_grid',
        'classic_diner',
        'neon_chalkboard',
        'split_layout',
        'daily_special_hero',
        'classic_chalkboard',
        'tap_strips',
        'digital_tap_board'
    ));
GO

GO

-- ===== 032_add_screen_pre_registration.sql =====

ALTER TABLE dbo.Screens
ADD DesiredAppVersion NVARCHAR(50) NULL,
    DeliveryReference NVARCHAR(100) NULL,
    PreRegistrationTokenHash CHAR(64) NULL,
    PreRegistrationExpiresUtc DATETIME2(7) NULL,
    PreRegisteredUtc DATETIME2(7) NULL;
GO

CREATE UNIQUE INDEX UX_Screens_PreRegistrationTokenHash
    ON dbo.Screens (PreRegistrationTokenHash)
    WHERE PreRegistrationTokenHash IS NOT NULL;
GO

GO

-- ===== 033_add_subscription_period_end_state.sql =====

ALTER TABLE dbo.VenueSubscriptions
ADD CancelAtPeriodEnd BIT NOT NULL
    CONSTRAINT DF_VenueSubscriptions_CancelAtPeriodEnd DEFAULT 0;
GO

GO

-- ===== 034_create_haas_contracts.sql =====

CREATE TABLE dbo.HaasContracts
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HaasContracts PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    BundleKey NVARCHAR(50) NOT NULL,
    TermMonths INT NOT NULL,
    MonthlyAmount DECIMAL(10,2) NOT NULL,
    StripeSubscriptionId NVARCHAR(100) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    StartedUtc DATETIME2(7) NOT NULL,
    ContractEndsUtc DATETIME2(7) NOT NULL,
    EndedUtc DATETIME2(7) NULL,
    CancelAtPeriodEnd BIT NOT NULL CONSTRAINT DF_HaasContracts_CancelAtPeriodEnd DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_HaasContracts_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_HaasContracts_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_HaasContracts_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_HaasContracts_StripeSubscriptionId UNIQUE (StripeSubscriptionId),
    CONSTRAINT CK_HaasContracts_BundleTerm CHECK
    (
        (BundleKey = 'starter_kit' AND TermMonths = 18) OR
        (BundleKey = 'bar_pack' AND TermMonths = 24) OR
        (BundleKey = 'full_house' AND TermMonths = 36)
    ),
    CONSTRAINT CK_HaasContracts_MonthlyAmount CHECK (MonthlyAmount > 0),
    CONSTRAINT CK_HaasContracts_Status CHECK (Status IN ('active', 'past_due', 'completed', 'canceled')),
    CONSTRAINT CK_HaasContracts_Dates CHECK (ContractEndsUtc > StartedUtc)
);
GO

CREATE INDEX IX_HaasContracts_VenueId_Status
    ON dbo.HaasContracts (VenueId, Status, ContractEndsUtc DESC);
GO

GO

-- ===== 035_create_pos_connections.sql =====

CREATE TABLE dbo.PosConnections
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PosConnections PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Provider INT NOT NULL,
    Status INT NOT NULL,
    ExternalMerchantId NVARCHAR(200) NOT NULL,
    ProtectedAccessToken NVARCHAR(MAX) NOT NULL,
    ProtectedRefreshToken NVARCHAR(MAX) NULL,
    AccessTokenExpiresUtc DATETIME2(7) NULL,
    LastSyncedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosConnections_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosConnections_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PosConnections_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_PosConnections_VenueId_Provider UNIQUE (VenueId, Provider),
    CONSTRAINT CK_PosConnections_Provider CHECK (Provider IN (1, 2, 3)),
    CONSTRAINT CK_PosConnections_Status CHECK (Status IN (0, 1, 2, 3)),
    CONSTRAINT CK_PosConnections_Merchant CHECK (LEN(LTRIM(RTRIM(ExternalMerchantId))) > 0),
    CONSTRAINT CK_PosConnections_ProtectedAccessToken CHECK (LEN(ProtectedAccessToken) > 0)
);
GO

CREATE INDEX IX_PosConnections_VenueId_Status
    ON dbo.PosConnections (VenueId, Status, Provider);
GO

GO

-- ===== 036_create_pos_catalog_mappings.sql =====

CREATE TABLE dbo.PosCatalogMappings
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PosCatalogMappings PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Provider INT NOT NULL,
    EntityType INT NOT NULL,
    ExternalId NVARCHAR(300) NOT NULL,
    LocalEntityId UNIQUEIDENTIFIER NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosCatalogMappings_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosCatalogMappings_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PosCatalogMappings_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_PosCatalogMappings_Source UNIQUE (VenueId, Provider, EntityType, ExternalId),
    CONSTRAINT CK_PosCatalogMappings_Provider CHECK (Provider IN (1, 2, 3)),
    CONSTRAINT CK_PosCatalogMappings_EntityType CHECK (EntityType IN (1, 2, 3, 4)),
    CONSTRAINT CK_PosCatalogMappings_ExternalId CHECK (LEN(LTRIM(RTRIM(ExternalId))) > 0)
);
GO

CREATE INDEX IX_PosCatalogMappings_LocalEntity
    ON dbo.PosCatalogMappings (VenueId, Provider, EntityType, LocalEntityId);
GO

GO

-- ===== 037_create_pos_webhook_events.sql =====

CREATE TABLE dbo.PosWebhookEvents
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PosWebhookEvents PRIMARY KEY,
    Provider INT NOT NULL,
    ProviderEventId NVARCHAR(300) NOT NULL,
    EventType NVARCHAR(200) NOT NULL,
    ExternalMerchantId NVARCHAR(200) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    Status INT NOT NULL CONSTRAINT DF_PosWebhookEvents_Status DEFAULT 0,
    AttemptCount INT NOT NULL CONSTRAINT DF_PosWebhookEvents_AttemptCount DEFAULT 0,
    ReceivedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosWebhookEvents_ReceivedUtc DEFAULT SYSUTCDATETIME(),
    StartedUtc DATETIME2(7) NULL,
    ProcessedUtc DATETIME2(7) NULL,
    NextAttemptUtc DATETIME2(7) NULL,
    FailureReason NVARCHAR(500) NULL,
    CONSTRAINT UQ_PosWebhookEvents_ProviderEvent UNIQUE (Provider, ProviderEventId),
    CONSTRAINT CK_PosWebhookEvents_Provider CHECK (Provider IN (1, 2, 3)),
    CONSTRAINT CK_PosWebhookEvents_Status CHECK (Status IN (0, 1, 2, 3)),
    CONSTRAINT CK_PosWebhookEvents_AttemptCount CHECK (AttemptCount >= 0),
    CONSTRAINT CK_PosWebhookEvents_EventId CHECK (LEN(LTRIM(RTRIM(ProviderEventId))) > 0),
    CONSTRAINT CK_PosWebhookEvents_EventType CHECK (LEN(LTRIM(RTRIM(EventType))) > 0),
    CONSTRAINT CK_PosWebhookEvents_Merchant CHECK (LEN(LTRIM(RTRIM(ExternalMerchantId))) > 0)
);
GO

CREATE INDEX IX_PosWebhookEvents_WorkQueue
    ON dbo.PosWebhookEvents (Status, NextAttemptUtc, ReceivedUtc, Id);
GO

GO

-- ===== 038_add_pos_sync_health.sql =====

ALTER TABLE dbo.PosConnections
ADD LastSyncAttemptUtc DATETIME2(7) NULL,
    ConsecutiveSyncFailures INT NOT NULL CONSTRAINT DF_PosConnections_ConsecutiveSyncFailures DEFAULT 0,
    NextSyncAttemptUtc DATETIME2(7) NULL,
    LastSyncErrorCode NVARCHAR(80) NULL;
GO

ALTER TABLE dbo.PosConnections
ADD CONSTRAINT CK_PosConnections_ConsecutiveSyncFailures
    CHECK (ConsecutiveSyncFailures >= 0);
GO

CREATE INDEX IX_PosConnections_Provider_NextSyncAttemptUtc
    ON dbo.PosConnections (Provider, Status, NextSyncAttemptUtc)
    INCLUDE (VenueId, LastSyncedUtc, ConsecutiveSyncFailures);
GO

GO

-- ===== 039_add_pos_refresh_token_expiration.sql =====

IF COL_LENGTH('dbo.PosConnections', 'RefreshTokenExpiresUtc') IS NULL
BEGIN
    ALTER TABLE dbo.PosConnections
        ADD RefreshTokenExpiresUtc DATETIME2(7) NULL;
END;

GO

-- ===== 040_create_customer_identity_tenancy.sql =====

CREATE TABLE dbo.CustomerUsers
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerUsers PRIMARY KEY,
    Email NVARCHAR(320) NOT NULL,
    NormalizedEmail NVARCHAR(320) NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    Status INT NOT NULL,
    EmailVerifiedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_CustomerUsers_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_CustomerUsers_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_CustomerUsers_NormalizedEmail UNIQUE (NormalizedEmail),
    CONSTRAINT CK_CustomerUsers_Status CHECK (Status IN (1, 2, 3)),
    CONSTRAINT CK_CustomerUsers_Email CHECK (LEN(LTRIM(RTRIM(Email))) > 0),
    CONSTRAINT CK_CustomerUsers_NormalizedEmail CHECK (NormalizedEmail = UPPER(LTRIM(RTRIM(Email))))
);
GO

CREATE TABLE dbo.ExternalIdentities
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ExternalIdentities PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Provider INT NOT NULL,
    ProviderSubject NVARCHAR(255) NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_ExternalIdentities_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_ExternalIdentities_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ExternalIdentities_CustomerUsers FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT UQ_ExternalIdentities_Provider_Subject UNIQUE (Provider, ProviderSubject),
    CONSTRAINT UQ_ExternalIdentities_User_Provider UNIQUE (UserId, Provider),
    CONSTRAINT CK_ExternalIdentities_Provider CHECK (Provider IN (1, 2)),
    CONSTRAINT CK_ExternalIdentities_Subject CHECK (LEN(LTRIM(RTRIM(ProviderSubject))) > 0)
);
GO

CREATE TABLE dbo.Organizations
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Organizations PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    OwnerUserId UNIQUEIDENTIFIER NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Organizations_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Organizations_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Organizations_OwnerUser FOREIGN KEY (OwnerUserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT CK_Organizations_Name CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
);
GO

CREATE TABLE dbo.OrganizationMemberships
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OrganizationMemberships PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Role INT NOT NULL,
    JoinedUtc DATETIME2(7) NOT NULL,
    RevokedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_OrganizationMemberships_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_OrganizationMemberships_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_OrganizationMemberships_Organizations FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations (Id),
    CONSTRAINT FK_OrganizationMemberships_CustomerUsers FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT UQ_OrganizationMemberships_Organization_User UNIQUE (OrganizationId, UserId),
    CONSTRAINT CK_OrganizationMemberships_Role CHECK (Role IN (1, 2, 3)),
    CONSTRAINT CK_OrganizationMemberships_Revoke CHECK (RevokedUtc IS NULL OR RevokedUtc >= JoinedUtc)
);
GO

CREATE UNIQUE INDEX UX_OrganizationMemberships_ActiveOwner
    ON dbo.OrganizationMemberships (OrganizationId)
    WHERE Role = 1 AND RevokedUtc IS NULL;
GO

ALTER TABLE dbo.Venues ADD OrganizationId UNIQUEIDENTIFIER NULL;
GO

ALTER TABLE dbo.Venues ADD CONSTRAINT FK_Venues_Organizations
    FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations (Id);
GO

CREATE UNIQUE INDEX UX_Venues_Id_OrganizationId
    ON dbo.Venues (Id, OrganizationId);
GO

CREATE TABLE dbo.VenueMemberships
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VenueMemberships PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Role INT NOT NULL,
    GrantedUtc DATETIME2(7) NOT NULL,
    RevokedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueMemberships_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueMemberships_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_VenueMemberships_Venues FOREIGN KEY (VenueId, OrganizationId)
        REFERENCES dbo.Venues (Id, OrganizationId),
    CONSTRAINT FK_VenueMemberships_OrganizationMemberships FOREIGN KEY (OrganizationId, UserId)
        REFERENCES dbo.OrganizationMemberships (OrganizationId, UserId),
    CONSTRAINT UQ_VenueMemberships_Venue_User UNIQUE (VenueId, UserId),
    CONSTRAINT CK_VenueMemberships_Role CHECK (Role IN (1, 2, 3)),
    CONSTRAINT CK_VenueMemberships_Revoke CHECK (RevokedUtc IS NULL OR RevokedUtc >= GrantedUtc)
);
GO

CREATE TABLE dbo.MembershipAuditEntries
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MembershipAuditEntries PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NULL,
    ActorUserId UNIQUEIDENTIFIER NOT NULL,
    SubjectUserId UNIQUEIDENTIFIER NOT NULL,
    Scope INT NOT NULL,
    Action INT NOT NULL,
    PreviousRole NVARCHAR(30) NULL,
    NewRole NVARCHAR(30) NULL,
    OccurredUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_MembershipAuditEntries_Organizations FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations (Id),
    CONSTRAINT FK_MembershipAuditEntries_Venues FOREIGN KEY (VenueId, OrganizationId)
        REFERENCES dbo.Venues (Id, OrganizationId),
    CONSTRAINT FK_MembershipAuditEntries_Actor FOREIGN KEY (ActorUserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT FK_MembershipAuditEntries_Subject FOREIGN KEY (SubjectUserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT CK_MembershipAuditEntries_Scope CHECK (Scope IN (1, 2)),
    CONSTRAINT CK_MembershipAuditEntries_Action CHECK (Action BETWEEN 1 AND 9),
    CONSTRAINT CK_MembershipAuditEntries_VenueScope CHECK
    (
        (Scope = 1 AND VenueId IS NULL) OR
        (Scope = 2 AND VenueId IS NOT NULL)
    )
);
GO

CREATE INDEX IX_MembershipAuditEntries_Organization_Occurred
    ON dbo.MembershipAuditEntries (OrganizationId, OccurredUtc DESC, Id);
GO

CREATE TRIGGER dbo.TR_MembershipAuditEntries_Immutable
ON dbo.MembershipAuditEntries
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    THROW 51004, 'Membership audit entries are immutable.', 1;
END;
GO

GO

-- ===== 041_create_customer_authentication.sql =====

CREATE TABLE dbo.CustomerAuthSessions
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerAuthSessions PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash CHAR(64) NOT NULL,
    AuthenticationMethod INT NOT NULL,
    AuthenticatedUtc DATETIME2(7) NOT NULL,
    LastSeenUtc DATETIME2(7) NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    RevokedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_CustomerAuthSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT UQ_CustomerAuthSessions_TokenHash UNIQUE (TokenHash),
    CONSTRAINT CK_CustomerAuthSessions_Method CHECK (AuthenticationMethod IN (1, 2, 3)),
    CONSTRAINT CK_CustomerAuthSessions_Lifetime CHECK
        (ExpiresUtc > CreatedUtc AND LastSeenUtc >= CreatedUtc AND LastSeenUtc <= ExpiresUtc),
    CONSTRAINT CK_CustomerAuthSessions_Revoke CHECK (RevokedUtc IS NULL OR RevokedUtc >= CreatedUtc)
);
GO

CREATE INDEX IX_CustomerAuthSessions_User_Active
    ON dbo.CustomerAuthSessions (UserId, ExpiresUtc DESC)
    WHERE RevokedUtc IS NULL;
GO

CREATE TABLE dbo.EmailLoginTokens
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmailLoginTokens PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash CHAR(64) NOT NULL,
    ReturnPath NVARCHAR(500) NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    ConsumedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_EmailLoginTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT UQ_EmailLoginTokens_TokenHash UNIQUE (TokenHash),
    CONSTRAINT CK_EmailLoginTokens_ReturnPath CHECK
        (LEFT(ReturnPath, 1) = '/' AND LEFT(ReturnPath, 2) <> '//'),
    CONSTRAINT CK_EmailLoginTokens_Lifetime CHECK (ExpiresUtc > CreatedUtc),
    CONSTRAINT CK_EmailLoginTokens_Consumed CHECK
        (ConsumedUtc IS NULL OR (ConsumedUtc >= CreatedUtc AND ConsumedUtc <= ExpiresUtc))
);
GO

CREATE INDEX IX_EmailLoginTokens_User_Active
    ON dbo.EmailLoginTokens (UserId, ExpiresUtc DESC)
    WHERE ConsumedUtc IS NULL;
GO

GO

-- ===== 042_add_customer_strong_authentication.sql =====

ALTER TABLE dbo.CustomerAuthSessions
ADD Assurance INT NOT NULL CONSTRAINT DF_CustomerAuthSessions_Assurance DEFAULT (1),
    StepUpUtc DATETIME2 NULL;
GO

CREATE TABLE dbo.CustomerPasskeyCredentials (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerPasskeyCredentials PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CredentialId VARBINARY(1024) NOT NULL,
    PublicKey VARBINARY(MAX) NOT NULL,
    UserHandle VARBINARY(64) NOT NULL,
    SignatureCounter BIGINT NOT NULL CONSTRAINT DF_CustomerPasskeys_Counter DEFAULT (0),
    DisplayName NVARCHAR(100) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL,
    LastUsedUtc DATETIME2 NULL,
    RevokedUtc DATETIME2 NULL,
    CONSTRAINT FK_CustomerPasskeys_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id),
    CONSTRAINT UQ_CustomerPasskeys_Credential UNIQUE (CredentialId)
);
GO

CREATE TABLE dbo.CustomerTotpAuthenticators (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerTotpAuthenticators PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ProtectedSecret NVARCHAR(2000) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL,
    VerifiedUtc DATETIME2 NULL,
    LastAcceptedCounter BIGINT NULL,
    RevokedUtc DATETIME2 NULL,
    CONSTRAINT FK_CustomerTotp_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id)
);
GO
CREATE UNIQUE INDEX UX_CustomerTotp_ActiveUser ON dbo.CustomerTotpAuthenticators(UserId) WHERE RevokedUtc IS NULL;
GO

CREATE TABLE dbo.CustomerRecoveryCodes (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerRecoveryCodes PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CodeHash CHAR(64) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL,
    UsedUtc DATETIME2 NULL,
    CONSTRAINT FK_CustomerRecoveryCodes_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id),
    CONSTRAINT UQ_CustomerRecoveryCodes_Hash UNIQUE (CodeHash)
);
GO

CREATE TABLE dbo.CustomerAuthenticationChallenges (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerAuthenticationChallenges PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type INT NOT NULL,
    ProtectedOptions NVARCHAR(MAX) NOT NULL,
    ExpiresUtc DATETIME2 NOT NULL,
    ConsumedUtc DATETIME2 NULL,
    CreatedUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_CustomerAuthenticationChallenges_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id),
    CONSTRAINT CK_CustomerAuthenticationChallenges_Type CHECK (Type IN (1, 2))
);
GO
CREATE INDEX IX_CustomerAuthenticationChallenges_Expiry ON dbo.CustomerAuthenticationChallenges(ExpiresUtc, ConsumedUtc);
GO

GO

-- ===== 043_add_tier_trial_entitlements.sql =====

ALTER TABLE dbo.SubscriptionTiers
ADD MaxVenues INT NOT NULL CONSTRAINT DF_SubscriptionTiers_MaxVenues DEFAULT (1),
    TrialDays INT NOT NULL CONSTRAINT DF_SubscriptionTiers_TrialDays DEFAULT (14),
    TrialExpiryBehavior NVARCHAR(20) NOT NULL CONSTRAINT DF_SubscriptionTiers_TrialExpiryBehavior DEFAULT ('disable'),
    CONSTRAINT CK_SubscriptionTiers_MaxVenues CHECK (MaxVenues = -1 OR MaxVenues > 0),
    CONSTRAINT CK_SubscriptionTiers_TrialDays CHECK (TrialDays BETWEEN 0 AND 90),
    CONSTRAINT CK_SubscriptionTiers_TrialExpiryBehavior CHECK (TrialExpiryBehavior IN ('disable', 'read_only'));
GO

GO

-- ===== 044_create_organization_subscriptions.sql =====

CREATE TABLE dbo.OrganizationSubscriptions
(
    OrganizationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OrganizationSubscriptions PRIMARY KEY,
    TierId UNIQUEIDENTIFIER NOT NULL,
    StripeCustomerId NVARCHAR(100) NULL,
    StripeSubscriptionId NVARCHAR(100) NULL,
    Status NVARCHAR(30) NOT NULL,
    TrialEndsAt DATETIME2(7) NULL,
    CurrentPeriodEnd DATETIME2(7) NULL,
    CancelAtPeriodEnd BIT NOT NULL CONSTRAINT DF_OrganizationSubscriptions_CancelAtPeriodEnd DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_OrganizationSubscriptions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_OrganizationSubscriptions_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_OrganizationSubscriptions_Organizations FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations (Id),
    CONSTRAINT FK_OrganizationSubscriptions_SubscriptionTiers FOREIGN KEY (TierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT CK_OrganizationSubscriptions_Status CHECK (Status IN ('trialing', 'active', 'past_due', 'canceled'))
);
GO

CREATE UNIQUE INDEX UX_OrganizationSubscriptions_StripeCustomerId
    ON dbo.OrganizationSubscriptions (StripeCustomerId)
    WHERE StripeCustomerId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_OrganizationSubscriptions_StripeSubscriptionId
    ON dbo.OrganizationSubscriptions (StripeSubscriptionId)
    WHERE StripeSubscriptionId IS NOT NULL;
GO

CREATE INDEX IX_OrganizationSubscriptions_TierId_Status
    ON dbo.OrganizationSubscriptions (TierId, Status);
GO

;WITH UnambiguousLegacySubscription AS
(
    SELECT V.OrganizationId
    FROM dbo.Venues V
    INNER JOIN dbo.VenueSubscriptions VS ON VS.VenueId = V.Id
    WHERE V.OrganizationId IS NOT NULL
    GROUP BY V.OrganizationId
    HAVING COUNT_BIG(*) = 1
)
INSERT dbo.OrganizationSubscriptions
(
    OrganizationId,
    TierId,
    StripeSubscriptionId,
    Status,
    TrialEndsAt,
    CurrentPeriodEnd,
    CancelAtPeriodEnd,
    CreatedUtc,
    UpdatedUtc
)
SELECT
    Legacy.OrganizationId,
    VS.TierId,
    VS.StripeSubscriptionId,
    VS.Status,
    VS.TrialEndsAt,
    VS.CurrentPeriodEnd,
    VS.CancelAtPeriodEnd,
    VS.CreatedUtc,
    VS.UpdatedUtc
FROM UnambiguousLegacySubscription Legacy
INNER JOIN dbo.Venues V ON V.OrganizationId = Legacy.OrganizationId
INNER JOIN dbo.VenueSubscriptions VS ON VS.VenueId = V.Id;
GO

GO

-- ===== 045_create_customer_onboarding_states.sql =====

CREATE TABLE dbo.CustomerOnboardingStates
(
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerOnboardingStates PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NULL,
    SelectedTierId UNIQUEIDENTIFIER NULL,
    VenueId UNIQUEIDENTIFIER NULL,
    FirstScreenId UNIQUEIDENTIFIER NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_CustomerOnboardingStates_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_CustomerOnboardingStates_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CustomerOnboardingStates_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT FK_CustomerOnboardingStates_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations (Id),
    CONSTRAINT FK_CustomerOnboardingStates_Tier FOREIGN KEY (SelectedTierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT FK_CustomerOnboardingStates_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_CustomerOnboardingStates_FirstScreen FOREIGN KEY (FirstScreenId) REFERENCES dbo.Screens (Id)
);

CREATE UNIQUE INDEX UX_CustomerOnboardingStates_OrganizationId
    ON dbo.CustomerOnboardingStates (OrganizationId)
    WHERE OrganizationId IS NOT NULL;

CREATE INDEX IX_CustomerOnboardingStates_UpdatedUtc
    ON dbo.CustomerOnboardingStates (UpdatedUtc DESC);

GO

-- ===== 046_create_system_configuration.sql =====

CREATE TABLE dbo.SystemConfigurationDefinitions
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SystemConfigurationDefinitions PRIMARY KEY,
    [Key] NVARCHAR(300) NOT NULL,
    ApplicationScope NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(1000) NOT NULL,
    ValueType NVARCHAR(30) NOT NULL,
    IsRequired BIT NOT NULL CONSTRAINT DF_SystemConfigurationDefinitions_IsRequired DEFAULT 0,
    IsSecret BIT NOT NULL CONSTRAINT DF_SystemConfigurationDefinitions_IsSecret DEFAULT 0,
    DefaultValue NVARCHAR(MAX) NULL,
    ValidationPattern NVARCHAR(1000) NULL,
    RequiresRestart BIT NOT NULL CONSTRAINT DF_SystemConfigurationDefinitions_RequiresRestart DEFAULT 0,
    ExportPolicy NVARCHAR(30) NOT NULL CONSTRAINT DF_SystemConfigurationDefinitions_ExportPolicy DEFAULT N'Include',
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SystemConfigurationDefinitions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SystemConfigurationDefinitions_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    Version ROWVERSION NOT NULL,
    CONSTRAINT UQ_SystemConfigurationDefinitions_Scope_Key UNIQUE (ApplicationScope, [Key]),
    CONSTRAINT CK_SystemConfigurationDefinitions_Scope CHECK (ApplicationScope IN (N'Shared', N'API', N'Admin', N'VenueAdmin', N'Display', N'Background')),
    CONSTRAINT CK_SystemConfigurationDefinitions_ValueType CHECK (ValueType IN (N'String', N'Boolean', N'Integer', N'Decimal', N'Uri', N'Json')),
    CONSTRAINT CK_SystemConfigurationDefinitions_SecretDefault CHECK (IsSecret = 0 OR DefaultValue IS NULL)
);

CREATE TABLE dbo.SystemConfigurationValues
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SystemConfigurationValues PRIMARY KEY,
    DefinitionId UNIQUEIDENTIFIER NOT NULL,
    EnvironmentName NVARCHAR(30) NOT NULL,
    ValuePayload NVARCHAR(MAX) NOT NULL,
    IsEncrypted BIT NOT NULL,
    UpdatedBy NVARCHAR(256) NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SystemConfigurationValues_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SystemConfigurationValues_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    Version ROWVERSION NOT NULL,
    CONSTRAINT FK_SystemConfigurationValues_Definition FOREIGN KEY (DefinitionId) REFERENCES dbo.SystemConfigurationDefinitions(Id),
    CONSTRAINT UQ_SystemConfigurationValues_Definition_Environment UNIQUE (DefinitionId, EnvironmentName),
    CONSTRAINT CK_SystemConfigurationValues_Environment CHECK (EnvironmentName IN (N'Development', N'Test', N'Staging', N'Production'))
);

CREATE TABLE dbo.SystemConfigurationRevisions
(
    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SystemConfigurationRevisions PRIMARY KEY,
    ConfigurationValueId UNIQUEIDENTIFIER NOT NULL,
    RevisionNumber INT NOT NULL,
    ValuePayload NVARCHAR(MAX) NOT NULL,
    ValueFingerprint CHAR(64) NOT NULL,
    IsEncrypted BIT NOT NULL,
    ChangedBy NVARCHAR(256) NOT NULL,
    ChangeSource NVARCHAR(100) NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SystemConfigurationRevisions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_SystemConfigurationRevisions_Value FOREIGN KEY (ConfigurationValueId) REFERENCES dbo.SystemConfigurationValues(Id),
    CONSTRAINT UQ_SystemConfigurationRevisions_Value_Revision UNIQUE (ConfigurationValueId, RevisionNumber)
);

CREATE TABLE dbo.SystemConfigurationAudit
(
    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SystemConfigurationAudit PRIMARY KEY,
    EnvironmentName NVARCHAR(30) NOT NULL,
    ApplicationScope NVARCHAR(50) NOT NULL,
    SettingKey NVARCHAR(300) NOT NULL,
    ActionName NVARCHAR(50) NOT NULL,
    Actor NVARCHAR(256) NOT NULL,
    ChangeSource NVARCHAR(100) NOT NULL,
    PreviousFingerprint CHAR(64) NULL,
    NewFingerprint CHAR(64) NULL,
    ImportOperationId UNIQUEIDENTIFIER NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_SystemConfigurationAudit_CreatedUtc DEFAULT SYSUTCDATETIME()
);

CREATE INDEX IX_SystemConfigurationValues_Environment ON dbo.SystemConfigurationValues(EnvironmentName, DefinitionId);
CREATE INDEX IX_SystemConfigurationAudit_Environment_Created ON dbo.SystemConfigurationAudit(EnvironmentName, CreatedUtc DESC);

GO

-- ===== 047_seed_system_configuration_definitions.sql =====

DECLARE @Definitions TABLE
(
    [Key] NVARCHAR(300), ApplicationScope NVARCHAR(50), [Description] NVARCHAR(1000), ValueType NVARCHAR(30),
    IsRequired BIT, IsSecret BIT, DefaultValue NVARCHAR(MAX), ValidationPattern NVARCHAR(1000), RequiresRestart BIT, ExportPolicy NVARCHAR(30)
);

INSERT INTO @Definitions VALUES
(N'CustomerAuthentication:Google:Enabled', N'API', N'Enables Google customer sign-in.', N'Boolean', 0, 0, N'false', NULL, 1, N'Include'),
(N'CustomerAuthentication:Google:ClientId', N'API', N'Google OAuth client identifier.', N'String', 0, 0, NULL, NULL, 1, N'Include'),
(N'CustomerAuthentication:Google:ClientSecret', N'API', N'Google OAuth client secret.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'CustomerAuthentication:Apple:Enabled', N'API', N'Enables Apple customer sign-in.', N'Boolean', 0, 0, N'false', NULL, 1, N'Include'),
(N'CustomerAuthentication:Apple:ClientId', N'API', N'Apple Services ID.', N'String', 0, 0, NULL, NULL, 1, N'Include'),
(N'CustomerAuthentication:Apple:ClientSecret', N'API', N'Apple client-secret JWT.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'CustomerAuthentication:EmailDelivery:Enabled', N'API', N'Enables customer email-link delivery.', N'Boolean', 0, 0, N'false', NULL, 1, N'Include'),
(N'CustomerAuthentication:EmailDelivery:Endpoint', N'API', N'HTTPS customer email delivery endpoint.', N'Uri', 0, 0, NULL, N'^https://', 1, N'Include'),
(N'CustomerAuthentication:EmailDelivery:ApiKey', N'API', N'Customer email delivery API key.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'Stripe:SecretKey', N'API', N'Stripe server API secret.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'StripeWebhook:SigningSecret', N'API', N'Stripe webhook signing secret.', N'String', 0, 1, NULL, N'^whsec_', 1, N'Exclude');

MERGE dbo.SystemConfigurationDefinitions AS target
USING @Definitions AS source
ON target.ApplicationScope = source.ApplicationScope AND target.[Key] = source.[Key]
WHEN MATCHED THEN UPDATE SET
    [Description] = source.[Description], ValueType = source.ValueType, IsRequired = source.IsRequired,
    IsSecret = source.IsSecret, DefaultValue = source.DefaultValue, ValidationPattern = source.ValidationPattern,
    RequiresRestart = source.RequiresRestart, ExportPolicy = source.ExportPolicy, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT
    (Id, [Key], ApplicationScope, [Description], ValueType, IsRequired, IsSecret, DefaultValue, ValidationPattern, RequiresRestart, ExportPolicy)
VALUES
    (NEWID(), source.[Key], source.ApplicationScope, source.[Description], source.ValueType, source.IsRequired,
     source.IsSecret, source.DefaultValue, source.ValidationPattern, source.RequiresRestart, source.ExportPolicy);

GO

-- ===== 048_add_system_configuration_clear_state.sql =====

ALTER TABLE dbo.SystemConfigurationValues ALTER COLUMN ValuePayload NVARCHAR(MAX) NULL;
ALTER TABLE dbo.SystemConfigurationValues ADD IsDeleted BIT NOT NULL CONSTRAINT DF_SystemConfigurationValues_IsDeleted DEFAULT 0;
ALTER TABLE dbo.SystemConfigurationRevisions ALTER COLUMN ValuePayload NVARCHAR(MAX) NULL;

GO

-- ===== 049_seed_provider_configuration_definitions.sql =====

IF EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'StripeWebhook:SigningSecret')
AND NOT EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:Webhook:SigningSecret')
    UPDATE dbo.SystemConfigurationDefinitions SET [Key]=N'Stripe:Webhook:SigningSecret' WHERE ApplicationScope=N'API' AND [Key]=N'StripeWebhook:SigningSecret';
IF EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:SecretKey')
AND NOT EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:Revenue:ApiKey')
    UPDATE dbo.SystemConfigurationDefinitions SET [Key]=N'Stripe:Revenue:ApiKey' WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:SecretKey';

DECLARE @Definitions TABLE
(
    [Key] NVARCHAR(300), [Description] NVARCHAR(1000), ValueType NVARCHAR(30), IsSecret BIT,
    DefaultValue NVARCHAR(MAX), ValidationPattern NVARCHAR(1000), RequiresRestart BIT, ExportPolicy NVARCHAR(30)
);
INSERT INTO @Definitions VALUES
(N'SuperAdmin:ApiKey',N'Super Admin API access key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'CustomerAuthentication:Google:Enabled',N'Enables Google customer sign-in.',N'Boolean',0,N'false',NULL,1,N'Include'),
(N'CustomerAuthentication:Google:ClientId',N'Google OAuth client identifier.',N'String',0,NULL,NULL,1,N'Include'),
(N'CustomerAuthentication:Google:ClientSecret',N'Google OAuth client secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'CustomerAuthentication:Apple:Enabled',N'Enables Apple customer sign-in.',N'Boolean',0,N'false',NULL,1,N'Include'),
(N'CustomerAuthentication:Apple:ClientId',N'Apple Services ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'CustomerAuthentication:Apple:ClientSecret',N'Apple client-secret JWT.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'CustomerAuthentication:EmailDelivery:Enabled',N'Enables customer email delivery.',N'Boolean',0,N'false',NULL,1,N'Include'),
(N'CustomerAuthentication:EmailDelivery:Endpoint',N'Customer email delivery HTTPS endpoint.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'CustomerAuthentication:EmailDelivery:ApiKey',N'Customer email delivery API key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Stripe:Revenue:ApiKey',N'Stripe server API key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Stripe:Webhook:SigningSecret',N'Stripe webhook signing secret.',N'String',1,NULL,N'^whsec_',1,N'Exclude'),
(N'Stripe:Webhook:ToleranceSeconds',N'Stripe webhook signature tolerance.',N'Integer',0,N'300',N'^[0-9]+$',0,N'Include'),
(N'Stripe:Checkout:SuccessUrl',N'Stripe checkout success URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:Checkout:CancelUrl',N'Stripe checkout cancellation URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:BillingPortal:ReturnUrl',N'Stripe billing portal return URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:HaasCheckout:SuccessUrl',N'HaaS checkout success URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:HaasCheckout:CancelUrl',N'HaaS checkout cancellation URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:HaasCheckout:PriceIds:starter_kit',N'Stripe price ID for the Starter Kit HaaS bundle.',N'String',0,NULL,N'^price_',1,N'Include'),
(N'Stripe:HaasCheckout:PriceIds:bar_pack',N'Stripe price ID for the Bar Pack HaaS bundle.',N'String',0,NULL,N'^price_',1,N'Include'),
(N'Stripe:HaasCheckout:PriceIds:full_house',N'Stripe price ID for the Full House HaaS bundle.',N'String',0,NULL,N'^price_',1,N'Include'),
(N'Square:OAuth:ApplicationId',N'Square OAuth application ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'Square:OAuth:ApplicationSecret',N'Square OAuth application secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Square:OAuth:AuthorizationEndpoint',N'Square OAuth authorization endpoint.',N'Uri',0,N'https://connect.squareup.com/oauth2/authorize',N'^https://',1,N'Include'),
(N'Square:OAuth:TokenEndpoint',N'Square OAuth token endpoint.',N'Uri',0,N'https://connect.squareup.com/oauth2/token',N'^https://',1,N'Include'),
(N'Square:OAuth:RevokeEndpoint',N'Square OAuth revoke endpoint.',N'Uri',0,N'https://connect.squareup.com/oauth2/revoke',N'^https://',1,N'Include'),
(N'Square:OAuth:CallbackUrl',N'Square OAuth callback URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Square:OAuth:VenueAdminReturnUrl',N'Square Venue Admin return URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Square:OAuth:ApiVersion',N'Square API version.',N'String',0,N'2026-07-15',NULL,1,N'Include'),
(N'Square:OAuth:Scopes:0',N'First Square OAuth scope.',N'String',0,N'MERCHANT_PROFILE_READ',NULL,1,N'Include'),
(N'Square:OAuth:Scopes:1',N'Second Square OAuth scope.',N'String',0,N'ITEMS_READ',NULL,1,N'Include'),
(N'Square:OAuth:Scopes:2',N'Third Square OAuth scope.',N'String',0,N'INVENTORY_READ',NULL,1,N'Include'),
(N'Square:Catalog:Endpoint',N'Square catalog endpoint.',N'Uri',0,N'https://connect.squareup.com/v2/catalog/list',N'^https://',0,N'Include'),
(N'Square:Catalog:ApiVersion',N'Square catalog API version.',N'String',0,N'2026-07-15',NULL,0,N'Include'),
(N'Square:Webhooks:SignatureKey',N'Square webhook signature key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Square:Webhooks:NotificationUrl',N'Square webhook notification URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Toast:Catalog:Endpoint',N'Toast menu endpoint.',N'Uri',0,N'https://ws-api.toasttab.com/menus/v2/menus',N'^https://',0,N'Include'),
(N'Toast:Catalog:CurrencyCode',N'Toast catalog currency.',N'String',0,N'USD',N'^[A-Z]{3}$',0,N'Include'),
(N'Toast:Inventory:Endpoint',N'Toast inventory endpoint.',N'Uri',0,N'https://ws-api.toasttab.com/stock/v1/inventory/search',N'^https://',0,N'Include'),
(N'Toast:Inventory:MaximumItemsPerRequest',N'Toast inventory batch size.',N'Integer',0,N'100',N'^[0-9]+$',0,N'Include'),
(N'Toast:Polling:PollInterval',N'Toast polling interval.',N'String',0,N'01:00:00',NULL,0,N'Include'),
(N'Toast:Polling:InterConnectionDelay',N'Delay between Toast connection polls.',N'String',0,N'00:00:00.250',NULL,0,N'Include'),
(N'Toast:Polling:InitialFailureBackoff',N'Toast initial retry backoff.',N'String',0,N'00:05:00',NULL,0,N'Include'),
(N'Toast:Polling:MaximumFailureBackoff',N'Toast maximum retry backoff.',N'String',0,N'01:00:00',NULL,0,N'Include'),
(N'Toast:Webhooks:MenusSecret',N'Toast menu webhook secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Toast:Webhooks:StockSecret',N'Toast stock webhook secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Clover:OAuth:ClientId',N'Clover OAuth client ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'Clover:OAuth:ClientSecret',N'Clover OAuth client secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Clover:OAuth:AuthorizationEndpoint',N'Clover OAuth authorization endpoint.',N'Uri',0,N'https://www.clover.com/oauth/v2/authorize',N'^https://',1,N'Include'),
(N'Clover:OAuth:TokenEndpoint',N'Clover OAuth token endpoint.',N'Uri',0,N'https://api.clover.com/oauth/v2/token',N'^https://',1,N'Include'),
(N'Clover:OAuth:CallbackUrl',N'Clover OAuth callback URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Clover:OAuth:VenueAdminReturnUrl',N'Clover Venue Admin return URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Clover:Catalog:BaseUrl',N'Clover API base URL.',N'Uri',0,N'https://api.clover.com',N'^https://',0,N'Include'),
(N'Clover:Catalog:CurrencyCode',N'Clover catalog currency.',N'String',0,N'USD',N'^[A-Z]{3}$',0,N'Include'),
(N'Clover:Catalog:PageSize',N'Clover catalog page size.',N'Integer',0,N'1000',N'^[0-9]+$',0,N'Include'),
(N'Clover:Webhooks:AppId',N'Clover webhook app ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'Clover:Webhooks:AuthCode',N'Clover webhook authorization code.',N'String',1,NULL,NULL,1,N'Exclude');

MERGE dbo.SystemConfigurationDefinitions AS target
USING @Definitions AS source ON target.ApplicationScope=N'API' AND target.[Key]=source.[Key]
WHEN MATCHED THEN UPDATE SET [Description]=source.[Description],ValueType=source.ValueType,IsSecret=source.IsSecret,DefaultValue=source.DefaultValue,ValidationPattern=source.ValidationPattern,RequiresRestart=source.RequiresRestart,ExportPolicy=source.ExportPolicy,UpdatedUtc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT(Id,[Key],ApplicationScope,[Description],ValueType,IsRequired,IsSecret,DefaultValue,ValidationPattern,RequiresRestart,ExportPolicy)
VALUES(NEWID(),source.[Key],N'API',source.[Description],source.ValueType,0,source.IsSecret,source.DefaultValue,source.ValidationPattern,source.RequiresRestart,source.ExportPolicy);

GO

-- ===== 050_add_configuration_rotation_metadata.sql =====

ALTER TABLE dbo.SystemConfigurationDefinitions ADD RotationReminderDays INT NULL;
EXEC sys.sp_executesql N'ALTER TABLE dbo.SystemConfigurationDefinitions ADD CONSTRAINT CK_SystemConfigurationDefinitions_RotationReminderDays CHECK (RotationReminderDays IS NULL OR RotationReminderDays > 0);';
EXEC sys.sp_executesql N'UPDATE dbo.SystemConfigurationDefinitions SET RotationReminderDays = 90 WHERE IsSecret = 1;';

GO

-- ===== 051_seed_customer_frontend_origin.sql =====

IF NOT EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope = N'Api' AND [Key] = N'CustomerAuthentication:FrontendOrigin')
BEGIN
    INSERT dbo.SystemConfigurationDefinitions
        (Id, ApplicationScope, [Key], [Description], ValueType, IsRequired, IsSecret, DefaultValue, ValidationPattern, RequiresRestart, ExportPolicy, RotationReminderDays)
    VALUES
        (NEWID(), N'Api', N'CustomerAuthentication:FrontendOrigin', N'Trusted HTTPS origin used after customer authentication callbacks.', N'Uri', 1, 0, NULL, N'^https://[^/]+(?::[0-9]+)?$', 1, N'Include', NULL);
END;

GO

-- ===== 052_migrate_administrative_identity.sql =====

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SystemConfigurationDefinitions', N'U') IS NOT NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.SystemConfigurationDefinitions legacy
        JOIN dbo.SystemConfigurationDefinitions canonical
          ON canonical.Id <> legacy.Id
         AND canonical.ApplicationScope = CASE legacy.ApplicationScope
              WHEN N'Admin' THEN N'PlatformOperations'
              WHEN N'VenueAdmin' THEN N'BackOffice'
              ELSE legacy.ApplicationScope
          END
         AND canonical.[Key] = CASE legacy.[Key]
              WHEN N'SuperAdmin:ApiKey' THEN N'PlatformOperations:ApiKey'
              WHEN N'Square:OAuth:VenueAdminReturnUrl' THEN N'Square:OAuth:BackOfficeReturnUrl'
              WHEN N'Clover:OAuth:VenueAdminReturnUrl' THEN N'Clover:OAuth:BackOfficeReturnUrl'
              ELSE legacy.[Key]
          END
        WHERE legacy.ApplicationScope IN (N'Admin', N'VenueAdmin')
           OR legacy.[Key] IN
           (
               N'SuperAdmin:ApiKey',
               N'Square:OAuth:VenueAdminReturnUrl',
               N'Clover:OAuth:VenueAdminReturnUrl'
           )
    )
        THROW 51020, 'Administrative identity migration found a canonical duplicate.', 1;

    ALTER TABLE dbo.SystemConfigurationDefinitions
        DROP CONSTRAINT CK_SystemConfigurationDefinitions_Scope;

    UPDATE dbo.SystemConfigurationDefinitions
    SET ApplicationScope = CASE ApplicationScope
        WHEN N'Admin' THEN N'PlatformOperations'
        WHEN N'VenueAdmin' THEN N'BackOffice'
        ELSE ApplicationScope
    END,
    UpdatedUtc = SYSUTCDATETIME()
    WHERE ApplicationScope IN (N'Admin', N'VenueAdmin');

    UPDATE dbo.SystemConfigurationDefinitions
    SET [Key] = CASE [Key]
        WHEN N'SuperAdmin:ApiKey' THEN N'PlatformOperations:ApiKey'
        WHEN N'Square:OAuth:VenueAdminReturnUrl' THEN N'Square:OAuth:BackOfficeReturnUrl'
        WHEN N'Clover:OAuth:VenueAdminReturnUrl' THEN N'Clover:OAuth:BackOfficeReturnUrl'
        ELSE [Key]
    END,
    UpdatedUtc = SYSUTCDATETIME()
    WHERE [Key] IN
    (
        N'SuperAdmin:ApiKey',
        N'Square:OAuth:VenueAdminReturnUrl',
        N'Clover:OAuth:VenueAdminReturnUrl'
    );

    ALTER TABLE dbo.SystemConfigurationDefinitions WITH CHECK
        ADD CONSTRAINT CK_SystemConfigurationDefinitions_Scope CHECK
        (
            ApplicationScope IN
            (
                N'Shared', N'API', N'PlatformOperations', N'BackOffice', N'Display', N'Background'
            )
        );

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SystemConfigurationDefinitions
        WHERE ApplicationScope IN (N'Admin', N'VenueAdmin')
           OR [Key] IN
           (
               N'SuperAdmin:ApiKey',
               N'Square:OAuth:VenueAdminReturnUrl',
               N'Clover:OAuth:VenueAdminReturnUrl'
           )
    )
        THROW 51022, 'Administrative identity migration verification failed.', 1;
END;

COMMIT TRANSACTION;

GO

-- ===== 053_add_menu_item_lifecycle.sql =====

IF COL_LENGTH('dbo.MenuItems', 'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.MenuItems
        ADD IsActive BIT NOT NULL
            CONSTRAINT DF_MenuItems_IsActive DEFAULT 1 WITH VALUES;
END;

GO

-- ===== 053_create_scoped_authority.sql =====

CREATE TABLE dbo.AuthorityPermissions
(
    PermissionId NVARCHAR(120) NOT NULL CONSTRAINT PK_AuthorityPermissions PRIMARY KEY,
    CapabilityId NVARCHAR(120) NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_AuthorityPermissions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_AuthorityPermissions_Canonical CHECK (PermissionId = LOWER(PermissionId) AND PermissionId LIKE '%.%.%')
);
GO

CREATE TABLE dbo.AuthorityRoles
(
    RoleKey NVARCHAR(80) NOT NULL CONSTRAINT PK_AuthorityRoles PRIMARY KEY,
    NameMessageKey NVARCHAR(200) NOT NULL,
    IsSystem BIT NOT NULL,
    IsProtected BIT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_AuthorityRoles_CreatedUtc DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE dbo.AuthorityRolePermissions
(
    RoleKey NVARCHAR(80) NOT NULL,
    PermissionId NVARCHAR(120) NOT NULL,
    CONSTRAINT PK_AuthorityRolePermissions PRIMARY KEY (RoleKey, PermissionId),
    CONSTRAINT FK_AuthorityRolePermissions_Role FOREIGN KEY (RoleKey) REFERENCES dbo.AuthorityRoles (RoleKey),
    CONSTRAINT FK_AuthorityRolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.AuthorityPermissions (PermissionId)
);
GO

INSERT dbo.AuthorityPermissions (PermissionId, CapabilityId)
VALUES
('content.item.create', 'content.item.create'),
('content.item.update', 'content.item.update'),
('content.item.archive', 'content.item.archive'),
('content.item.availability_update', 'content.item.availability_update'),
('content.item.dietary_information_manage', 'content.item.dietary_information_manage'),
('content.collection.bulk_update', 'content.collection.bulk_update'),
('content.source.synchronize', 'content.source.synchronize'),
('publishing.release.preview', 'publishing.release.preview'),
('publishing.release.publish', 'publishing.release.publish'),
('publishing.release.confirm', 'publishing.release.confirm'),
('publishing.release.replace', 'publishing.release.replace'),
('publishing.release.unpublish', 'publishing.release.unpublish'),
('publishing.delivery.retry', 'publishing.delivery.retry'),
('publishing.delivery.restore', 'publishing.delivery.restore'),
('screen.device.view', 'screen.device.view'),
('screen.device.pair', 'screen.device.pair'),
('screen.device.unpair', 'screen.device.unpair'),
('screen.content.target', 'screen.content.target'),
('screen.delivery.view', 'screen.delivery.view'),
('screen.delivery.recover', 'screen.delivery.recover'),
('screen.wall.coordinate', 'screen.wall.coordinate'),
('schedule.entry.manage', 'schedule.entry.manage'),
('schedule.rotation.manage', 'schedule.rotation.manage'),
('schedule.promotion.automate', 'schedule.promotion.automate'),
('schedule.conflict.resolve', 'schedule.conflict.resolve'),
('workflow.approval.request', 'workflow.approval.request'),
('workflow.approval.review', 'workflow.approval.review'),
('workflow.assignment.manage', 'workflow.assignment.manage'),
('organization.venue.create', 'organization.venue.create'),
('organization.venue.manage', 'organization.venue.manage'),
('organization.content.bulk_publish', 'organization.content.bulk_publish'),
('organization.template.manage', 'organization.template.manage'),
('localization.variant.manage', 'localization.variant.manage'),
('localization.variant.review', 'localization.variant.review'),
('localization.translation.automate', 'localization.translation.automate'),
('analytics.delivery_health.view', 'analytics.delivery_health.view'),
('analytics.operations.view', 'analytics.operations.view'),
('analytics.portfolio.view', 'analytics.portfolio.view'),
('analytics.report.export', 'analytics.report.export'),
('branding.theme.manage', 'branding.theme.manage'),
('branding.layout.manage', 'branding.layout.manage'),
('branding.standard.manage', 'branding.standard.manage'),
('branding.custom_content.manage', 'branding.custom_content.manage'),
('account.profile.manage', 'account.profile.manage'),
('account.security.manage', 'account.security.manage'),
('account.billing.view', 'account.billing.view'),
('account.billing.manage', 'account.billing.manage'),
('account.member.manage', 'account.member.manage'),
('support.context.enter', 'support.context.enter'),
('support.entitlement.override', 'support.entitlement.override'),
('support.allowance.override', 'support.allowance.override');
GO

INSERT dbo.AuthorityRoles (RoleKey, NameMessageKey, IsSystem, IsProtected)
VALUES
('organization_owner', 'roles.organization_owner.name', 1, 1),
('organization_administrator', 'roles.organization_administrator.name', 1, 1),
('venue_administrator', 'roles.venue_administrator.name', 1, 1),
('content_manager', 'roles.content_manager.name', 1, 1),
('content_editor', 'roles.content_editor.name', 1, 1),
('publisher', 'roles.publisher.name', 1, 1),
('viewer', 'roles.viewer.name', 1, 1),
('support_operator', 'roles.support_operator.name', 1, 1);
GO

INSERT dbo.AuthorityRolePermissions (RoleKey, PermissionId)
SELECT 'organization_owner', PermissionId FROM dbo.AuthorityPermissions WHERE PermissionId NOT LIKE 'support.%'
UNION ALL
SELECT 'organization_administrator', PermissionId FROM dbo.AuthorityPermissions
    WHERE PermissionId NOT LIKE 'support.%' AND PermissionId <> 'account.security.manage'
UNION ALL
SELECT 'venue_administrator', PermissionId FROM dbo.AuthorityPermissions
    WHERE PermissionId LIKE 'content.%' OR PermissionId LIKE 'publishing.%' OR PermissionId LIKE 'screen.%'
       OR PermissionId LIKE 'schedule.%' OR PermissionId LIKE 'localization.%'
       OR PermissionId LIKE 'analytics.delivery_health.%' OR PermissionId LIKE 'branding.theme.%'
UNION ALL
SELECT 'content_manager', PermissionId FROM dbo.AuthorityPermissions
    WHERE PermissionId LIKE 'content.%' OR PermissionId LIKE 'publishing.%' OR PermissionId LIKE 'schedule.%'
       OR PermissionId LIKE 'localization.%' OR PermissionId LIKE 'branding.%'
UNION ALL
SELECT 'content_editor', PermissionId FROM dbo.AuthorityPermissions
    WHERE PermissionId LIKE 'content.%' OR PermissionId LIKE 'localization.variant.%'
       OR PermissionId LIKE 'branding.theme.%'
       OR PermissionId IN ('publishing.release.preview', 'analytics.delivery_health.view')
UNION ALL
SELECT 'publisher', PermissionId FROM dbo.AuthorityPermissions
    WHERE PermissionId IN
    (
        'publishing.release.preview', 'publishing.release.publish', 'publishing.release.confirm',
        'publishing.release.replace', 'publishing.release.unpublish', 'publishing.delivery.retry',
        'publishing.delivery.restore', 'screen.device.view', 'screen.delivery.view', 'screen.delivery.recover'
    )
UNION ALL
SELECT 'viewer', PermissionId FROM dbo.AuthorityPermissions
    WHERE PermissionId IN
    (
        'publishing.release.preview', 'screen.device.view', 'screen.delivery.view',
        'analytics.delivery_health.view', 'account.billing.view'
    )
UNION ALL
SELECT 'support_operator', PermissionId FROM dbo.AuthorityPermissions WHERE PermissionId LIKE 'support.%';
GO

CREATE TABLE dbo.ScopedRoleAssignments
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ScopedRoleAssignments PRIMARY KEY,
    ActorUserId UNIQUEIDENTIFIER NOT NULL,
    RoleKey NVARCHAR(80) NOT NULL,
    ScopeType INT NOT NULL,
    ScopeId UNIQUEIDENTIFIER NOT NULL,
    StartsUtc DATETIME2(7) NOT NULL,
    ExpiresUtc DATETIME2(7) NULL,
    RevokedUtc DATETIME2(7) NULL,
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_ScopedRoleAssignments_Role FOREIGN KEY (RoleKey) REFERENCES dbo.AuthorityRoles (RoleKey),
    CONSTRAINT CK_ScopedRoleAssignments_Scope CHECK (ScopeType BETWEEN 1 AND 6),
    CONSTRAINT CK_ScopedRoleAssignments_Window CHECK (ExpiresUtc IS NULL OR ExpiresUtc > StartsUtc)
);
GO

CREATE INDEX IX_ScopedRoleAssignments_ActorActive
    ON dbo.ScopedRoleAssignments (ActorUserId, RevokedUtc, StartsUtc, ExpiresUtc)
    INCLUDE (RoleKey, ScopeType, ScopeId);
GO

CREATE TABLE dbo.SupportAccessGrants
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SupportAccessGrants PRIMARY KEY,
    SupportUserId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NULL,
    Reason NVARCHAR(1000) NOT NULL,
    StartsUtc DATETIME2(7) NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    ApprovedByUserId UNIQUEIDENTIFIER NOT NULL,
    RevokedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT CK_SupportAccessGrants_Reason CHECK (LEN(LTRIM(RTRIM(Reason))) > 0),
    CONSTRAINT CK_SupportAccessGrants_Window CHECK (ExpiresUtc > StartsUtc AND DATEDIFF(MINUTE, StartsUtc, ExpiresUtc) <= 480)
);
GO

CREATE INDEX IX_SupportAccessGrants_Active
    ON dbo.SupportAccessGrants (SupportUserId, OrganizationId, VenueId, RevokedUtc, StartsUtc, ExpiresUtc);
GO

CREATE TABLE dbo.SupportAccessAuditEntries
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SupportAccessAuditEntries PRIMARY KEY,
    GrantId UNIQUEIDENTIFIER NULL,
    ActorUserId UNIQUEIDENTIFIER NOT NULL,
    OrganizationId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NULL,
    Action INT NOT NULL,
    Reason NVARCHAR(1000) NOT NULL,
    CorrelationId NVARCHAR(100) NOT NULL,
    OccurredUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_SupportAccessAuditEntries_Grant FOREIGN KEY (GrantId) REFERENCES dbo.SupportAccessGrants (Id),
    CONSTRAINT CK_SupportAccessAuditEntries_Action CHECK (Action BETWEEN 1 AND 5),
    CONSTRAINT CK_SupportAccessAuditEntries_Reason CHECK (LEN(LTRIM(RTRIM(Reason))) > 0)
);
GO

CREATE INDEX IX_SupportAccessAuditEntries_Context
    ON dbo.SupportAccessAuditEntries (OrganizationId, VenueId, OccurredUtc DESC)
    INCLUDE (ActorUserId, Action, GrantId, CorrelationId);
GO

GO

-- ===== 054_add_organization_profile.sql =====

ALTER TABLE dbo.Organizations ADD
    LegalName NVARCHAR(200) NULL,
    PrimaryContactName NVARCHAR(200) NULL,
    ContactEmail NVARCHAR(320) NULL,
    ContactPhone NVARCHAR(50) NULL,
    MailingAddress NVARCHAR(500) NULL;
GO

ALTER TABLE dbo.Organizations ADD CONSTRAINT CK_Organizations_Profile
CHECK (
    (LegalName IS NULL OR LEN(LTRIM(RTRIM(LegalName))) > 0) AND
    (PrimaryContactName IS NULL OR LEN(LTRIM(RTRIM(PrimaryContactName))) > 0) AND
    (ContactEmail IS NULL OR LEN(LTRIM(RTRIM(ContactEmail))) > 0) AND
    (ContactPhone IS NULL OR LEN(LTRIM(RTRIM(ContactPhone))) > 0) AND
    (MailingAddress IS NULL OR LEN(LTRIM(RTRIM(MailingAddress))) > 0)
);
GO

GO

-- ===== 055_add_screen_replacement_audit.sql =====

CREATE TABLE dbo.ScreenReplacementAudits
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ScreenReplacementAudits PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    TargetScreenId UNIQUEIDENTIFIER NOT NULL,
    SourceScreenId UNIQUEIDENTIFIER NOT NULL,
    PairingCode CHAR(6) NOT NULL,
    Actor NVARCHAR(200) NOT NULL,
    PreviousPlatform NVARCHAR(50) NULL,
    PreviousAppVersion NVARCHAR(50) NULL,
    ReplacementPlatform NVARCHAR(50) NULL,
    ReplacementAppVersion NVARCHAR(50) NULL,
    OccurredUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_ScreenReplacementAudits_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_ScreenReplacementAudits_TargetScreens FOREIGN KEY (TargetScreenId) REFERENCES dbo.Screens (Id),
    CONSTRAINT FK_ScreenReplacementAudits_SourceScreens FOREIGN KEY (SourceScreenId) REFERENCES dbo.Screens (Id),
    CONSTRAINT UQ_ScreenReplacementAudits_PairingCode UNIQUE (PairingCode)
);

CREATE INDEX IX_ScreenReplacementAudits_TargetScreenId_OccurredUtc
    ON dbo.ScreenReplacementAudits (TargetScreenId, OccurredUtc DESC);

GO

-- ===== 056_create_screen_content_deliveries.sql =====

CREATE TABLE dbo.ScreenContentDeliveries
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ScreenContentDeliveries PRIMARY KEY,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Revision BIGINT NOT NULL,
    State NVARCHAR(20) NOT NULL,
    RequestedUtc DATETIME2(7) NOT NULL,
    ReceivedUtc DATETIME2(7) NULL,
    AppliedUtc DATETIME2(7) NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    PlayerVersion NVARCHAR(50) NULL,
    ShellVersion NVARCHAR(50) NULL,
    Platform NVARCHAR(50) NULL,
    FailureCode NVARCHAR(50) NULL,
    FailureDetail NVARCHAR(240) NULL,
    CONSTRAINT FK_ScreenContentDeliveries_Screens FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
    CONSTRAINT FK_ScreenContentDeliveries_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_ScreenContentDeliveries_ScreenRevision UNIQUE (ScreenId, Revision),
    CONSTRAINT CK_ScreenContentDeliveries_State CHECK (State IN (N'Requested',N'Received',N'Applied',N'Failed',N'Superseded',N'Recovered'))
);

CREATE INDEX IX_ScreenContentDeliveries_VenueScreenRevision
    ON dbo.ScreenContentDeliveries (VenueId, ScreenId, Revision DESC);

GO

-- ===== 057_create_typed_capability_access.sql =====

CREATE TABLE dbo.CapabilityDefinitions
(
    CapabilityId varchar(120) NOT NULL CONSTRAINT PK_CapabilityDefinitions PRIMARY KEY,
    Domain tinyint NOT NULL,
    Classification tinyint NOT NULL,
    OperationKind tinyint NOT NULL,
    CONSTRAINT CK_CapabilityDefinitions_Id CHECK (CapabilityId LIKE '%.%.%'),
    CONSTRAINT CK_CapabilityDefinitions_Classification CHECK (Classification BETWEEN 1 AND 4)
);

CREATE TABLE dbo.CapabilityRollouts
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_CapabilityRollouts PRIMARY KEY,
    CapabilityId varchar(120) NOT NULL,
    OrganizationId uniqueidentifier NULL,
    VenueId uniqueidentifier NULL,
    RolloutState tinyint NOT NULL,
    StartsUtc datetime2 NOT NULL,
    EndsUtc datetime2 NULL,
    RetryAfterUtc datetime2 NULL,
    CreatedUtc datetime2 NOT NULL CONSTRAINT DF_CapabilityRollouts_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CapabilityRollouts_Definition FOREIGN KEY (CapabilityId) REFERENCES dbo.CapabilityDefinitions(CapabilityId),
    CONSTRAINT CK_CapabilityRollouts_State CHECK (RolloutState BETWEEN 1 AND 3),
    CONSTRAINT CK_CapabilityRollouts_Window CHECK (EndsUtc IS NULL OR EndsUtc > StartsUtc)
);

CREATE TABLE dbo.OrganizationCapabilityEntitlements
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_OrganizationCapabilityEntitlements PRIMARY KEY,
    OrganizationId uniqueidentifier NOT NULL,
    CapabilityId varchar(120) NOT NULL,
    Source varchar(80) NOT NULL,
    StartsUtc datetime2 NOT NULL,
    EndsUtc datetime2 NULL,
    RevokedUtc datetime2 NULL,
    CreatedUtc datetime2 NOT NULL CONSTRAINT DF_OrganizationCapabilityEntitlements_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_OrganizationCapabilityEntitlements_Definition FOREIGN KEY (CapabilityId) REFERENCES dbo.CapabilityDefinitions(CapabilityId),
    CONSTRAINT CK_OrganizationCapabilityEntitlements_Window CHECK (EndsUtc IS NULL OR EndsUtc > StartsUtc)
);

CREATE UNIQUE INDEX UX_OrganizationCapabilityEntitlements_Active
    ON dbo.OrganizationCapabilityEntitlements(OrganizationId, CapabilityId)
    WHERE RevokedUtc IS NULL AND EndsUtc IS NULL;

CREATE TABLE dbo.CapabilityAddOnAttachments
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_CapabilityAddOnAttachments PRIMARY KEY,
    OrganizationId uniqueidentifier NOT NULL,
    CapabilityId varchar(120) NOT NULL,
    AddOnKey varchar(80) NOT NULL,
    AttachedUtc datetime2 NOT NULL,
    DetachedUtc datetime2 NULL,
    CONSTRAINT FK_CapabilityAddOnAttachments_Definition FOREIGN KEY (CapabilityId) REFERENCES dbo.CapabilityDefinitions(CapabilityId)
);

CREATE TABLE dbo.CapabilityAllowances
(
    Id uniqueidentifier NOT NULL CONSTRAINT PK_CapabilityAllowances PRIMARY KEY,
    OrganizationId uniqueidentifier NOT NULL,
    VenueId uniqueidentifier NULL,
    CapabilityId varchar(120) NOT NULL,
    LimitValue int NOT NULL,
    StartsUtc datetime2 NOT NULL,
    EndsUtc datetime2 NULL,
    CONSTRAINT FK_CapabilityAllowances_Definition FOREIGN KEY (CapabilityId) REFERENCES dbo.CapabilityDefinitions(CapabilityId),
    CONSTRAINT CK_CapabilityAllowances_Limit CHECK (LimitValue >= 0),
    CONSTRAINT CK_CapabilityAllowances_Window CHECK (EndsUtc IS NULL OR EndsUtc > StartsUtc)
);

CREATE TABLE dbo.CapabilityAllowanceUsage
(
    AllowanceId uniqueidentifier NOT NULL CONSTRAINT PK_CapabilityAllowanceUsage PRIMARY KEY,
    UsedValue int NOT NULL,
    UpdatedUtc datetime2 NOT NULL,
    CONSTRAINT FK_CapabilityAllowanceUsage_Allowance FOREIGN KEY (AllowanceId) REFERENCES dbo.CapabilityAllowances(Id),
    CONSTRAINT CK_CapabilityAllowanceUsage_Value CHECK (UsedValue >= 0)
);

CREATE TABLE dbo.LayoutTemplates
(
    TemplateKey varchar(80) NOT NULL CONSTRAINT PK_LayoutTemplates PRIMARY KEY,
    NameMessageKey varchar(160) NOT NULL,
    RequiredCapabilityId varchar(120) NULL,
    IsSystem bit NOT NULL,
    IsActive bit NOT NULL,
    CreatedUtc datetime2 NOT NULL CONSTRAINT DF_LayoutTemplates_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LayoutTemplates_Definition FOREIGN KEY (RequiredCapabilityId) REFERENCES dbo.CapabilityDefinitions(CapabilityId)
);

INSERT dbo.CapabilityDefinitions (CapabilityId, Domain, Classification, OperationKind)
VALUES
('content.item.create',1,1,2),('content.item.update',1,1,2),('content.item.archive',1,1,2),
('content.item.availability_update',1,1,2),('content.item.dietary_information_manage',1,1,2),
('content.collection.bulk_update',1,2,2),('content.source.synchronize',1,2,2),
('publishing.release.preview',2,1,1),('publishing.release.publish',2,1,2),
('publishing.release.confirm',2,1,1),('publishing.release.replace',2,1,2),
('publishing.release.unpublish',2,1,2),('publishing.delivery.retry',2,1,2),
('publishing.delivery.restore',2,1,2),('screen.device.view',3,1,1),('screen.device.pair',3,1,2),
('screen.device.unpair',3,1,2),('screen.content.target',3,1,2),('screen.delivery.view',3,1,1),
('screen.delivery.recover',3,1,2),('screen.wall.coordinate',3,2,2),
('schedule.entry.manage',4,1,2),('schedule.rotation.manage',4,2,2),
('schedule.promotion.automate',4,2,2),('schedule.conflict.resolve',4,2,2),
('workflow.approval.request',5,2,2),('workflow.approval.review',5,2,2),
('workflow.assignment.manage',5,2,3),('organization.venue.create',6,1,2),
('organization.venue.manage',6,3,3),('organization.content.bulk_publish',6,3,2),
('organization.template.manage',6,3,3),('localization.variant.manage',7,1,2),
('localization.variant.review',7,2,2),('localization.translation.automate',7,2,2),
('analytics.delivery_health.view',8,1,1),('analytics.operations.view',8,2,1),
('analytics.portfolio.view',8,3,1),('analytics.report.export',8,3,1),
('branding.theme.manage',9,1,2),('branding.layout.manage',9,2,2),
('branding.standard.manage',9,3,3),('branding.custom_content.manage',9,4,2),
('account.profile.manage',10,1,2),('account.security.manage',10,1,2),
('account.billing.view',10,1,1),('account.billing.manage',10,1,3),
('account.member.manage',10,3,3),('support.context.enter',11,3,3),
('support.entitlement.override',11,3,3),('support.allowance.override',11,3,3);

INSERT dbo.LayoutTemplates (TemplateKey, NameMessageKey, RequiredCapabilityId, IsSystem, IsActive)
VALUES
('default', 'layouts.default.name', NULL, 1, 1),
('classic_diner', 'layouts.classic_diner.name', 'branding.layout.manage', 1, 1),
('neon_chalkboard', 'layouts.neon_chalkboard.name', 'branding.layout.manage', 1, 1),
('split_layout', 'layouts.split_layout.name', 'branding.layout.manage', 1, 1),
('daily_special_hero', 'layouts.daily_special_hero.name', 'branding.layout.manage', 1, 1),
('classic_chalkboard', 'layouts.classic_chalkboard.name', 'branding.layout.manage', 1, 1),
('tap_strips', 'layouts.tap_strips.name', 'branding.layout.manage', 1, 1),
('digital_tap_board', 'layouts.digital_tap_board.name', 'branding.layout.manage', 1, 1);

-- Convert the unchanged billing tier quantity into the new typed allowance. The
-- generic tier-feature matrix is deliberately not copied: it is no longer an
-- action-authority source. Unlimited tiers need no allowance row.
INSERT dbo.CapabilityAllowances
    (Id, OrganizationId, VenueId, CapabilityId, LimitValue, StartsUtc, EndsUtc)
SELECT NEWID(), v.OrganizationId, v.Id, 'screen.device.pair', tier.MaxScreens, SYSUTCDATETIME(), NULL
FROM dbo.Venues v
LEFT JOIN dbo.OrganizationSubscriptions os ON os.OrganizationId = v.OrganizationId
LEFT JOIN dbo.VenueSubscriptions vs ON vs.VenueId = v.Id AND os.OrganizationId IS NULL
INNER JOIN dbo.SubscriptionTiers tier ON tier.Id = COALESCE(os.TierId, vs.TierId)
WHERE v.OrganizationId IS NOT NULL AND tier.MaxScreens >= 0;

GO

CREATE OR ALTER PROCEDURE dbo.SyncScreenPairAllowanceForOrganization
    @OrganizationId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    DELETE usage
    FROM dbo.CapabilityAllowanceUsage usage
    INNER JOIN dbo.CapabilityAllowances allowance ON allowance.Id = usage.AllowanceId
    WHERE allowance.OrganizationId = @OrganizationId
      AND allowance.CapabilityId = 'screen.device.pair';

    DELETE allowance
    FROM dbo.CapabilityAllowances allowance
    WHERE allowance.OrganizationId = @OrganizationId
      AND allowance.CapabilityId = 'screen.device.pair';

    INSERT dbo.CapabilityAllowances
        (Id, OrganizationId, VenueId, CapabilityId, LimitValue, StartsUtc, EndsUtc)
    SELECT NEWID(), venue.OrganizationId, venue.Id, 'screen.device.pair', tier.MaxScreens, SYSUTCDATETIME(), NULL
    FROM dbo.Venues venue
    INNER JOIN dbo.OrganizationSubscriptions subscription ON subscription.OrganizationId = venue.OrganizationId
    INNER JOIN dbo.SubscriptionTiers tier ON tier.Id = subscription.TierId
    WHERE venue.OrganizationId = @OrganizationId AND tier.MaxScreens >= 0;
END;

GO

CREATE TRIGGER dbo.TR_OrganizationSubscriptions_SyncScreenPairAllowance
ON dbo.OrganizationSubscriptions
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OrganizationId uniqueidentifier;
    DECLARE changed CURSOR LOCAL FAST_FORWARD FOR SELECT DISTINCT OrganizationId FROM inserted;
    OPEN changed;
    FETCH NEXT FROM changed INTO @OrganizationId;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.SyncScreenPairAllowanceForOrganization @OrganizationId;
        FETCH NEXT FROM changed INTO @OrganizationId;
    END;
    CLOSE changed;
    DEALLOCATE changed;
END;

GO

CREATE TRIGGER dbo.TR_Venues_SyncScreenPairAllowance
ON dbo.Venues
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OrganizationId uniqueidentifier;
    DECLARE changed CURSOR LOCAL FAST_FORWARD FOR
        SELECT DISTINCT OrganizationId FROM inserted WHERE OrganizationId IS NOT NULL;
    OPEN changed;
    FETCH NEXT FROM changed INTO @OrganizationId;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.SyncScreenPairAllowanceForOrganization @OrganizationId;
        FETCH NEXT FROM changed INTO @OrganizationId;
    END;
    CLOSE changed;
    DEALLOCATE changed;
END;

GO

-- ===== 058_create_menu_item_library_spine.sql =====

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
-- NEVER BUILT: AvailabilityResetUtc and MenuItemTranslations. Both are owner-killed
-- concepts with no surviving reader. The old chain created them in scripts 013 and 012
-- and dropped them again here; this baseline simply does not create them. A database
-- that ran the old chain had them and lost them, and ends up in the same place.
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
-- Migration 013 indexed this column for the auto-reset sweep. The index has to
-- go first or the column cannot be dropped.
GO
