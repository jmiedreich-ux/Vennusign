CREATE TABLE dbo.FeatureUsages
(
    VenueId UNIQUEIDENTIFIER NOT NULL,
    FeatureId UNIQUEIDENTIFIER NOT NULL,
    PeriodStartUtc DATETIME2(7) NOT NULL,
    UsageCount INT NOT NULL CONSTRAINT DF_FeatureUsages_UsageCount DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_FeatureUsages_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_FeatureUsages_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_FeatureUsages PRIMARY KEY (VenueId, FeatureId, PeriodStartUtc),
    CONSTRAINT FK_FeatureUsages_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_FeatureUsages_Features FOREIGN KEY (FeatureId) REFERENCES dbo.Features (Id),
    CONSTRAINT CK_FeatureUsages_UsageCount CHECK (UsageCount >= 0),
    CONSTRAINT CK_FeatureUsages_PeriodStartUtc CHECK (
        DAY(PeriodStartUtc) = 1
        AND CONVERT(TIME, PeriodStartUtc) = '00:00:00'
    )
);
GO

CREATE INDEX IX_FeatureUsages_FeatureId_PeriodStartUtc
    ON dbo.FeatureUsages (FeatureId, PeriodStartUtc);
GO
