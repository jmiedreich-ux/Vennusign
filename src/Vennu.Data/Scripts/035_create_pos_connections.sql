CREATE TABLE dbo.PosConnections
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PosConnections PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Provider INT NOT NULL,
    Status INT NOT NULL,
    ExternalMerchantId NVARCHAR(200) NOT NULL,
    ProtectedAccessToken NVARCHAR(MAX) NOT NULL,
    ProtectedRefreshToken NVARCHAR(MAX) NULL,
    AccessTokenExpiresUtc DATETIME2(7) NULL,
    LastSyncedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosConnections_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosConnections_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PosConnections_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_PosConnections_VenueId_Provider UNIQUE (VenueId, Provider),
    CONSTRAINT CK_PosConnections_Provider CHECK (Provider IN (1, 2, 3)),
    CONSTRAINT CK_PosConnections_Status CHECK (Status IN (0, 1, 2, 3)),
    CONSTRAINT CK_PosConnections_Merchant CHECK (LEN(LTRIM(RTRIM(ExternalMerchantId))) > 0),
    CONSTRAINT CK_PosConnections_ProtectedAccessToken CHECK (LEN(ProtectedAccessToken) > 0)
);
GO

CREATE INDEX IX_PosConnections_VenueId_Status
    ON dbo.PosConnections (VenueId, Status, Provider);
GO
