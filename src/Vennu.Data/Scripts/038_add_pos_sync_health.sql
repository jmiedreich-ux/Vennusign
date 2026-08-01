ALTER TABLE dbo.PosConnections
ADD LastSyncAttemptUtc DATETIME2(7) NULL,
    ConsecutiveSyncFailures INT NOT NULL CONSTRAINT DF_PosConnections_ConsecutiveSyncFailures DEFAULT 0,
    NextSyncAttemptUtc DATETIME2(7) NULL,
    LastSyncErrorCode NVARCHAR(80) NULL;
GO

ALTER TABLE dbo.PosConnections
ADD CONSTRAINT CK_PosConnections_ConsecutiveSyncFailures
    CHECK (ConsecutiveSyncFailures >= 0);
GO

CREATE INDEX IX_PosConnections_Provider_NextSyncAttemptUtc
    ON dbo.PosConnections (Provider, Status, NextSyncAttemptUtc)
    INCLUDE (VenueId, LastSyncedUtc, ConsecutiveSyncFailures);
GO
