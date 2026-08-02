CREATE TABLE dbo.OrganizationSubscriptions
(
    OrganizationId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_OrganizationSubscriptions PRIMARY KEY,
    TierId UNIQUEIDENTIFIER NOT NULL,
    StripeCustomerId NVARCHAR(100) NULL,
    StripeSubscriptionId NVARCHAR(100) NULL,
    Status NVARCHAR(30) NOT NULL,
    TrialEndsAt DATETIME2(7) NULL,
    CurrentPeriodEnd DATETIME2(7) NULL,
    CancelAtPeriodEnd BIT NOT NULL CONSTRAINT DF_OrganizationSubscriptions_CancelAtPeriodEnd DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_OrganizationSubscriptions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_OrganizationSubscriptions_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_OrganizationSubscriptions_Organizations FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations (Id),
    CONSTRAINT FK_OrganizationSubscriptions_SubscriptionTiers FOREIGN KEY (TierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT CK_OrganizationSubscriptions_Status CHECK (Status IN ('trialing', 'active', 'past_due', 'canceled'))
);
GO

CREATE UNIQUE INDEX UX_OrganizationSubscriptions_StripeCustomerId
    ON dbo.OrganizationSubscriptions (StripeCustomerId)
    WHERE StripeCustomerId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_OrganizationSubscriptions_StripeSubscriptionId
    ON dbo.OrganizationSubscriptions (StripeSubscriptionId)
    WHERE StripeSubscriptionId IS NOT NULL;
GO

CREATE INDEX IX_OrganizationSubscriptions_TierId_Status
    ON dbo.OrganizationSubscriptions (TierId, Status);
GO

;WITH UnambiguousLegacySubscription AS
(
    SELECT V.OrganizationId
    FROM dbo.Venues V
    INNER JOIN dbo.VenueSubscriptions VS ON VS.VenueId = V.Id
    WHERE V.OrganizationId IS NOT NULL
    GROUP BY V.OrganizationId
    HAVING COUNT_BIG(*) = 1
)
INSERT dbo.OrganizationSubscriptions
(
    OrganizationId,
    TierId,
    StripeSubscriptionId,
    Status,
    TrialEndsAt,
    CurrentPeriodEnd,
    CancelAtPeriodEnd,
    CreatedUtc,
    UpdatedUtc
)
SELECT
    Legacy.OrganizationId,
    VS.TierId,
    VS.StripeSubscriptionId,
    VS.Status,
    VS.TrialEndsAt,
    VS.CurrentPeriodEnd,
    VS.CancelAtPeriodEnd,
    VS.CreatedUtc,
    VS.UpdatedUtc
FROM UnambiguousLegacySubscription Legacy
INNER JOIN dbo.Venues V ON V.OrganizationId = Legacy.OrganizationId
INNER JOIN dbo.VenueSubscriptions VS ON VS.VenueId = V.Id;
GO
