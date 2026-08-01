CREATE TABLE dbo.PosWebhookEvents
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PosWebhookEvents PRIMARY KEY,
    Provider INT NOT NULL,
    ProviderEventId NVARCHAR(300) NOT NULL,
    EventType NVARCHAR(200) NOT NULL,
    ExternalMerchantId NVARCHAR(200) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL,
    Status INT NOT NULL CONSTRAINT DF_PosWebhookEvents_Status DEFAULT 0,
    AttemptCount INT NOT NULL CONSTRAINT DF_PosWebhookEvents_AttemptCount DEFAULT 0,
    ReceivedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosWebhookEvents_ReceivedUtc DEFAULT SYSUTCDATETIME(),
    StartedUtc DATETIME2(7) NULL,
    ProcessedUtc DATETIME2(7) NULL,
    NextAttemptUtc DATETIME2(7) NULL,
    FailureReason NVARCHAR(500) NULL,
    CONSTRAINT UQ_PosWebhookEvents_ProviderEvent UNIQUE (Provider, ProviderEventId),
    CONSTRAINT CK_PosWebhookEvents_Provider CHECK (Provider IN (1, 2, 3)),
    CONSTRAINT CK_PosWebhookEvents_Status CHECK (Status IN (0, 1, 2, 3)),
    CONSTRAINT CK_PosWebhookEvents_AttemptCount CHECK (AttemptCount >= 0),
    CONSTRAINT CK_PosWebhookEvents_EventId CHECK (LEN(LTRIM(RTRIM(ProviderEventId))) > 0),
    CONSTRAINT CK_PosWebhookEvents_EventType CHECK (LEN(LTRIM(RTRIM(EventType))) > 0),
    CONSTRAINT CK_PosWebhookEvents_Merchant CHECK (LEN(LTRIM(RTRIM(ExternalMerchantId))) > 0)
);
GO

CREATE INDEX IX_PosWebhookEvents_WorkQueue
    ON dbo.PosWebhookEvents (Status, NextAttemptUtc, ReceivedUtc, Id);
GO
