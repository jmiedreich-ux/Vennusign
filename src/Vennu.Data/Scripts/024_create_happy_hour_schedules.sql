CREATE TABLE dbo.HappyHourSchedules
(
    VenueId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_HappyHourSchedules PRIMARY KEY,
    StartLocalTime TIME(0) NOT NULL,
    EndLocalTime TIME(0) NOT NULL,
    ActiveDaysMask INT NOT NULL CONSTRAINT DF_HappyHourSchedules_ActiveDaysMask DEFAULT 127,
    IsEnabled BIT NOT NULL CONSTRAINT DF_HappyHourSchedules_IsEnabled DEFAULT 1,
    OverrideMode NVARCHAR(20) NOT NULL CONSTRAINT DF_HappyHourSchedules_OverrideMode DEFAULT 'automatic',
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_HappyHourSchedules_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_HappyHourSchedules_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT CK_HappyHourSchedules_DistinctTimes CHECK (StartLocalTime <> EndLocalTime),
    CONSTRAINT CK_HappyHourSchedules_ActiveDaysMask CHECK (ActiveDaysMask BETWEEN 1 AND 127),
    CONSTRAINT CK_HappyHourSchedules_OverrideMode CHECK (OverrideMode IN ('automatic', 'force_on', 'force_off'))
);
GO
