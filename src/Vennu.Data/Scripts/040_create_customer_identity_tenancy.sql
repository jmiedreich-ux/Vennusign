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
    CONSTRAINT FK_MembershipAuditEntries_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
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
