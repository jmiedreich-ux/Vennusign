ALTER TABLE dbo.VenueSubscriptions
ADD CancelAtPeriodEnd BIT NOT NULL
    CONSTRAINT DF_VenueSubscriptions_CancelAtPeriodEnd DEFAULT 0;
GO
