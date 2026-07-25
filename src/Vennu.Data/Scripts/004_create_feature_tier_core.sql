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
