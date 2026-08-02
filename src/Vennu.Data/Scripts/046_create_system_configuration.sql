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
