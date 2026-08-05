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
