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
