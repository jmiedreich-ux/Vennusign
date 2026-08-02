ALTER TABLE dbo.SubscriptionTiers
ADD MaxVenues INT NOT NULL CONSTRAINT DF_SubscriptionTiers_MaxVenues DEFAULT (1),
    TrialDays INT NOT NULL CONSTRAINT DF_SubscriptionTiers_TrialDays DEFAULT (14),
    TrialExpiryBehavior NVARCHAR(20) NOT NULL CONSTRAINT DF_SubscriptionTiers_TrialExpiryBehavior DEFAULT ('disable'),
    CONSTRAINT CK_SubscriptionTiers_MaxVenues CHECK (MaxVenues = -1 OR MaxVenues > 0),
    CONSTRAINT CK_SubscriptionTiers_TrialDays CHECK (TrialDays BETWEEN 0 AND 90),
    CONSTRAINT CK_SubscriptionTiers_TrialExpiryBehavior CHECK (TrialExpiryBehavior IN ('disable', 'read_only'));
GO
