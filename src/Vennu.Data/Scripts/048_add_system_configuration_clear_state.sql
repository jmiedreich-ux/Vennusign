ALTER TABLE dbo.SystemConfigurationValues ALTER COLUMN ValuePayload NVARCHAR(MAX) NULL;
ALTER TABLE dbo.SystemConfigurationValues ADD IsDeleted BIT NOT NULL CONSTRAINT DF_SystemConfigurationValues_IsDeleted DEFAULT 0;
ALTER TABLE dbo.SystemConfigurationRevisions ALTER COLUMN ValuePayload NVARCHAR(MAX) NULL;
