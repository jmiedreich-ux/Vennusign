/* Menus 6-A3: replacement leases cover the complete working menu, not only dbo.Menus. */
ALTER TABLE dbo.MenuImportSessions ADD TargetWorkingFingerprint CHAR(64) NULL;
ALTER TABLE dbo.MenuImportReplacementSnapshots ADD ExpectedWorkingFingerprint CHAR(64) NULL;
GO
UPDATE dbo.MenuImportSessions SET TargetWorkingFingerprint=REPLICATE('0',64) WHERE Destination=N'replace' AND TargetWorkingFingerprint IS NULL;
UPDATE dbo.MenuImportReplacementSnapshots SET ExpectedWorkingFingerprint=REPLICATE('0',64) WHERE ExpectedWorkingFingerprint IS NULL;
GO
ALTER TABLE dbo.MenuImportReplacementSnapshots ALTER COLUMN ExpectedWorkingFingerprint CHAR(64) NOT NULL;
ALTER TABLE dbo.MenuImportSessions DROP CONSTRAINT CK_MenuImportSessions_Target;
ALTER TABLE dbo.MenuImportSessions DROP CONSTRAINT CK_MenuImportSessions_Completion;
ALTER TABLE dbo.MenuImportSessions ADD
 CONSTRAINT CK_MenuImportSessions_Target CHECK ((Destination=N'replace' AND TargetMenuId IS NOT NULL AND TargetUpdatedUtc IS NOT NULL AND TargetWorkingFingerprint IS NOT NULL) OR (Destination<>N'replace' AND TargetMenuId IS NULL AND TargetUpdatedUtc IS NULL AND TargetWorkingFingerprint IS NULL)),
 CONSTRAINT CK_MenuImportSessions_Completion CHECK ((CompletedMenuId IS NULL AND CompletedUtc IS NULL AND CompletedSnapshotId IS NULL) OR (CompletedMenuId IS NOT NULL AND CompletedUtc IS NOT NULL AND ((Destination=N'create' AND CompletedSnapshotId IS NULL) OR (Destination=N'replace' AND CompletedSnapshotId IS NOT NULL))));
GO
