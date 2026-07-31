CREATE TABLE dbo.HaasContracts
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HaasContracts PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    BundleKey NVARCHAR(50) NOT NULL,
    TermMonths INT NOT NULL,
    MonthlyAmount DECIMAL(10,2) NOT NULL,
    StripeSubscriptionId NVARCHAR(100) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    StartedUtc DATETIME2(7) NOT NULL,
    ContractEndsUtc DATETIME2(7) NOT NULL,
    EndedUtc DATETIME2(7) NULL,
    CancelAtPeriodEnd BIT NOT NULL CONSTRAINT DF_HaasContracts_CancelAtPeriodEnd DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_HaasContracts_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_HaasContracts_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_HaasContracts_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_HaasContracts_StripeSubscriptionId UNIQUE (StripeSubscriptionId),
    CONSTRAINT CK_HaasContracts_BundleTerm CHECK
    (
        (BundleKey = 'starter_kit' AND TermMonths = 18) OR
        (BundleKey = 'bar_pack' AND TermMonths = 24) OR
        (BundleKey = 'full_house' AND TermMonths = 36)
    ),
    CONSTRAINT CK_HaasContracts_MonthlyAmount CHECK (MonthlyAmount > 0),
    CONSTRAINT CK_HaasContracts_Status CHECK (Status IN ('active', 'past_due', 'completed', 'canceled')),
    CONSTRAINT CK_HaasContracts_Dates CHECK (ContractEndsUtc > StartedUtc)
);
GO

CREATE INDEX IX_HaasContracts_VenueId_Status
    ON dbo.HaasContracts (VenueId, Status, ContractEndsUtc DESC);
GO
