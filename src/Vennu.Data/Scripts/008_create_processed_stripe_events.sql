CREATE TABLE dbo.ProcessedStripeEvents
(
    EventId NVARCHAR(255) NOT NULL CONSTRAINT PK_ProcessedStripeEvents PRIMARY KEY,
    EventType NVARCHAR(100) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    StartedUtc DATETIME2(7) NOT NULL,
    ProcessedUtc DATETIME2(7) NULL,
    FailureReason NVARCHAR(500) NULL,
    CONSTRAINT CK_ProcessedStripeEvents_Status CHECK (Status IN ('processing', 'processed', 'failed'))
);
GO

CREATE INDEX IX_ProcessedStripeEvents_Status_StartedUtc
    ON dbo.ProcessedStripeEvents (Status, StartedUtc);
GO
