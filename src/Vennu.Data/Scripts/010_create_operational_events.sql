CREATE TABLE dbo.OperationalEvents
(
    Id UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    Summary NVARCHAR(500) NOT NULL,
    OccurredUtc DATETIME2 NOT NULL,
    CONSTRAINT PK_OperationalEvents PRIMARY KEY (Id),
    CONSTRAINT FK_OperationalEvents_Venues
        FOREIGN KEY (VenueId) REFERENCES dbo.Venues(Id)
);

CREATE INDEX IX_OperationalEvents_OccurredUtc
    ON dbo.OperationalEvents (OccurredUtc DESC)
    INCLUDE (VenueId, EventType, Summary);
