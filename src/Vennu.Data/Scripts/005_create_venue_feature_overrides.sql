CREATE TABLE dbo.VenueFeatureOverrides
(
    VenueId UNIQUEIDENTIFIER NOT NULL,
    FeatureId UNIQUEIDENTIFIER NOT NULL,
    Enabled BIT NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    ExpiresAt DATETIME2(7) NULL,
    CreatedByAdminId UNIQUEIDENTIFIER NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueFeatureOverrides_CreatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_VenueFeatureOverrides PRIMARY KEY (VenueId, FeatureId),
    CONSTRAINT FK_VenueFeatureOverrides_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_VenueFeatureOverrides_Features FOREIGN KEY (FeatureId) REFERENCES dbo.Features (Id),
    CONSTRAINT CK_VenueFeatureOverrides_Reason CHECK (LEN(LTRIM(RTRIM(Reason))) > 0)
);
GO

CREATE INDEX IX_VenueFeatureOverrides_ExpiresAt ON dbo.VenueFeatureOverrides (ExpiresAt) INCLUDE (VenueId, FeatureId, Enabled);
GO
