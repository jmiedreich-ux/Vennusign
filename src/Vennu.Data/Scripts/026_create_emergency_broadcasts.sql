CREATE TABLE dbo.EmergencyBroadcasts
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmergencyBroadcasts PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ScreenId UNIQUEIDENTIFIER NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(2000) NOT NULL,
    MediaUrl NVARCHAR(1000) NULL,
    StartsUtc DATETIME2(7) NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_EmergencyBroadcasts_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmergencyBroadcasts_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_EmergencyBroadcasts_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_EmergencyBroadcasts_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT FK_EmergencyBroadcasts_Screens FOREIGN KEY (VenueId, ScreenId) REFERENCES dbo.Screens (VenueId, Id) ON DELETE CASCADE,
    CONSTRAINT CK_EmergencyBroadcasts_Duration CHECK (ExpiresUtc > StartsUtc AND DATEDIFF(MINUTE, StartsUtc, ExpiresUtc) BETWEEN 1 AND 1440)
);
GO

CREATE INDEX IX_EmergencyBroadcasts_Active
    ON dbo.EmergencyBroadcasts (VenueId, ScreenId, IsActive, StartsUtc, ExpiresUtc);
GO
