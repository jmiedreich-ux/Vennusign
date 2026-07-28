ALTER TABLE dbo.SubscriptionTiers
ADD StripeMonthlyPriceId NVARCHAR(100) NULL,
    StripeAnnualPriceId NVARCHAR(100) NULL;
GO

CREATE UNIQUE INDEX UX_SubscriptionTiers_StripeProductId
    ON dbo.SubscriptionTiers (StripeProductId)
    WHERE StripeProductId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_SubscriptionTiers_StripeMonthlyPriceId
    ON dbo.SubscriptionTiers (StripeMonthlyPriceId)
    WHERE StripeMonthlyPriceId IS NOT NULL;
GO

CREATE UNIQUE INDEX UX_SubscriptionTiers_StripeAnnualPriceId
    ON dbo.SubscriptionTiers (StripeAnnualPriceId)
    WHERE StripeAnnualPriceId IS NOT NULL;
GO
