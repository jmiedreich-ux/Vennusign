CREATE TABLE dbo.ScreenContentDeliveries
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ScreenContentDeliveries PRIMARY KEY,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Revision BIGINT NOT NULL,
    State NVARCHAR(20) NOT NULL,
    RequestedUtc DATETIME2(7) NOT NULL,
    ReceivedUtc DATETIME2(7) NULL,
    AppliedUtc DATETIME2(7) NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    PlayerVersion NVARCHAR(50) NULL,
    ShellVersion NVARCHAR(50) NULL,
    Platform NVARCHAR(50) NULL,
    FailureCode NVARCHAR(50) NULL,
    FailureDetail NVARCHAR(240) NULL,
    CONSTRAINT FK_ScreenContentDeliveries_Screens FOREIGN KEY (ScreenId) REFERENCES dbo.Screens (Id),
    CONSTRAINT FK_ScreenContentDeliveries_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_ScreenContentDeliveries_ScreenRevision UNIQUE (ScreenId, Revision),
    CONSTRAINT CK_ScreenContentDeliveries_State CHECK (State IN (N'Requested',N'Received',N'Applied',N'Failed',N'Superseded',N'Recovered'))
);

CREATE INDEX IX_ScreenContentDeliveries_VenueScreenRevision
    ON dbo.ScreenContentDeliveries (VenueId, ScreenId, Revision DESC);
