CREATE TABLE dbo.DateRangePromotions
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DateRangePromotions PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(160) NOT NULL,
    StartLocalDate DATE NOT NULL,
    EndLocalDate DATE NOT NULL,
    TargetLayout NVARCHAR(80) NULL,
    Title NVARCHAR(200) NULL,
    Body NVARCHAR(1000) NULL,
    Priority INT NOT NULL CONSTRAINT DF_DateRangePromotions_Priority DEFAULT 0,
    IsEnabled BIT NOT NULL CONSTRAINT DF_DateRangePromotions_IsEnabled DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_DateRangePromotions_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_DateRangePromotions_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_DateRangePromotions_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT CK_DateRangePromotions_DateRange CHECK (EndLocalDate >= StartLocalDate),
    CONSTRAINT CK_DateRangePromotions_Priority CHECK (Priority BETWEEN -1000 AND 1000)
);
GO

CREATE INDEX IX_DateRangePromotions_Resolution
    ON dbo.DateRangePromotions (VenueId, IsEnabled, StartLocalDate, EndLocalDate, Priority DESC);
GO
