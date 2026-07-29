ALTER TABLE dbo.Menus
ADD DailySpecial NVARCHAR(240) NULL;
GO

ALTER TABLE dbo.MenuItems
ADD AvailabilityResetUtc DATETIME2(7) NULL;
GO

CREATE INDEX IX_MenuItems_AvailabilityResetUtc
    ON dbo.MenuItems (AvailabilityResetUtc)
    INCLUDE (VenueId, IsAvailable)
    WHERE AvailabilityResetUtc IS NOT NULL;
GO
