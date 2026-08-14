/* Menus 6-A3: atomic replacement checkpoints and destination state. */

ALTER TABLE dbo.MenuImportSessions DROP CONSTRAINT CK_MenuImportSessions_Destination;
ALTER TABLE dbo.MenuImportSessions DROP CONSTRAINT CK_MenuImportSessions_Completion;
ALTER TABLE dbo.MenuImportSessions ADD
    TargetMenuId UNIQUEIDENTIFIER NULL,
    TargetUpdatedUtc DATETIME2(7) NULL,
    TargetMenuName NVARCHAR(200) NULL,
    TargetHadPublishedVersion BIT NULL,
    TargetWorkingItemCount INT NULL,
    TargetPublishedItemCount INT NULL,
    TargetAddedCount INT NULL,
    TargetRemovedCount INT NULL,
    TargetChangedCount INT NULL,
    CompletedSnapshotId UNIQUEIDENTIFIER NULL;
GO

ALTER TABLE dbo.MenuImportSessions ADD
    CONSTRAINT FK_MenuImportSessions_TargetMenu FOREIGN KEY (TargetMenuId, VenueId) REFERENCES dbo.Menus (Id, VenueId),
    CONSTRAINT CK_MenuImportSessions_Destination CHECK (Destination IS NULL OR Destination IN (N'create',N'replace')),
    CONSTRAINT CK_MenuImportSessions_Target CHECK
      ((Destination=N'replace' AND TargetMenuId IS NOT NULL AND TargetUpdatedUtc IS NOT NULL) OR
       (Destination<>N'replace' AND TargetMenuId IS NULL AND TargetUpdatedUtc IS NULL)),
    CONSTRAINT CK_MenuImportSessions_Completion CHECK
      ((CompletedMenuId IS NULL AND CompletedUtc IS NULL AND CompletedSnapshotId IS NULL) OR
       (CompletedMenuId IS NOT NULL AND CompletedUtc IS NOT NULL AND Destination IN (N'create',N'replace')));
GO

CREATE TABLE dbo.MenuImportReplacementSnapshots
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuImportReplacementSnapshots PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    SessionId UNIQUEIDENTIFIER NOT NULL,
    SnapshotJson NVARCHAR(MAX) NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    CreatedBy NVARCHAR(320) NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    ExpectedMenuUpdatedUtc DATETIME2(7) NOT NULL,
    RestoredUtc DATETIME2(7) NULL,
    RestoredBy NVARCHAR(320) NULL,
    CONSTRAINT FK_MenuImportReplacementSnapshots_Menu FOREIGN KEY (MenuId,VenueId) REFERENCES dbo.Menus(Id,VenueId),
    CONSTRAINT UQ_MenuImportReplacementSnapshots_Session UNIQUE(SessionId),
    CONSTRAINT CK_MenuImportReplacementSnapshots_Json CHECK (ISJSON(SnapshotJson)=1),
    CONSTRAINT CK_MenuImportReplacementSnapshots_Expiry CHECK (ExpiresUtc>CreatedUtc)
);
CREATE INDEX IX_MenuImportReplacementSnapshots_Menu ON dbo.MenuImportReplacementSnapshots(VenueId,MenuId,CreatedUtc DESC);
GO

ALTER TABLE dbo.MenuImportSessions ADD CONSTRAINT FK_MenuImportSessions_CompletedSnapshot
  FOREIGN KEY (CompletedSnapshotId) REFERENCES dbo.MenuImportReplacementSnapshots(Id);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.CapabilityDefinitions WHERE CapabilityId=N'content.menu.import.snapshot_retention_days')
 INSERT dbo.CapabilityDefinitions(CapabilityId,Domain,Classification,OperationKind) VALUES(N'content.menu.import.snapshot_retention_days',1,1,2);
IF NOT EXISTS (SELECT 1 FROM dbo.CapabilityDefinitions WHERE CapabilityId=N'content.menu.import.restore_enabled')
 INSERT dbo.CapabilityDefinitions(CapabilityId,Domain,Classification,OperationKind) VALUES(N'content.menu.import.restore_enabled',1,1,2);
GO

INSERT dbo.CapabilityAllowances(Id,OrganizationId,VenueId,CapabilityId,LimitValue,StartsUtc,EndsUtc)
SELECT NEWID(),v.OrganizationId,v.Id,c.CapabilityId,c.LimitValue,SYSUTCDATETIME(),NULL
FROM dbo.Venues v CROSS JOIN (VALUES(N'content.menu.import.snapshot_retention_days',30),(N'content.menu.import.restore_enabled',1)) c(CapabilityId,LimitValue)
WHERE v.OrganizationId IS NOT NULL AND NOT EXISTS(SELECT 1 FROM dbo.CapabilityAllowances a WHERE a.VenueId=v.Id AND a.CapabilityId=c.CapabilityId);
GO
