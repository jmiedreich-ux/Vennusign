CREATE TABLE dbo.MealPeriods
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MealPeriods PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    StartLocalTime TIME(0) NOT NULL,
    EndLocalTime TIME(0) NOT NULL,
    ActiveDaysMask INT NOT NULL CONSTRAINT DF_MealPeriods_ActiveDaysMask DEFAULT 127,
    IsEnabled BIT NOT NULL CONSTRAINT DF_MealPeriods_IsEnabled DEFAULT 1,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MealPeriods_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MealPeriods_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MealPeriods_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_MealPeriods_VenueId_Name UNIQUE (VenueId, Name),
    CONSTRAINT UQ_MealPeriods_VenueId_SortOrder UNIQUE (VenueId, SortOrder),
    CONSTRAINT CK_MealPeriods_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_MealPeriods_DistinctTimes CHECK (StartLocalTime <> EndLocalTime),
    CONSTRAINT CK_MealPeriods_ActiveDaysMask CHECK (ActiveDaysMask BETWEEN 1 AND 127),
    CONSTRAINT CK_MealPeriods_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_MealPeriods_VenueId_Enabled_Order
    ON dbo.MealPeriods (VenueId, IsEnabled, SortOrder, Id);
GO
