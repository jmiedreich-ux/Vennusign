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
