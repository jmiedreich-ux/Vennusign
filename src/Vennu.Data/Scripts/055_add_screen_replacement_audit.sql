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
