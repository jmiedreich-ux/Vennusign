CREATE UNIQUE INDEX UX_Screens_VenueId_Id ON dbo.Screens (VenueId, Id);
GO

CREATE TABLE dbo.PlaylistSlides
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PlaylistSlides PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    ScreenId UNIQUEIDENTIFIER NOT NULL,
    SlideType NVARCHAR(20) NOT NULL,
    Title NVARCHAR(200) NULL,
    Body NVARCHAR(1000) NULL,
    MediaUrl NVARCHAR(1000) NULL,
    DwellSeconds INT NOT NULL CONSTRAINT DF_PlaylistSlides_DwellSeconds DEFAULT 10,
    StartLocalTime TIME(0) NULL,
    EndLocalTime TIME(0) NULL,
    ActiveDaysMask INT NULL,
    IsEnabled BIT NOT NULL CONSTRAINT DF_PlaylistSlides_IsEnabled DEFAULT 1,
    SortOrder INT NOT NULL CONSTRAINT DF_PlaylistSlides_SortOrder DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PlaylistSlides_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PlaylistSlides_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PlaylistSlides_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT FK_PlaylistSlides_Screens FOREIGN KEY (VenueId, ScreenId) REFERENCES dbo.Screens (VenueId, Id) ON DELETE CASCADE,
    CONSTRAINT CK_PlaylistSlides_Type CHECK (SlideType IN ('menu', 'image', 'message')),
    CONSTRAINT CK_PlaylistSlides_Dwell CHECK (DwellSeconds BETWEEN 5 AND 120),
    CONSTRAINT CK_PlaylistSlides_Window CHECK
    (
        (StartLocalTime IS NULL AND EndLocalTime IS NULL AND ActiveDaysMask IS NULL)
        OR
        (StartLocalTime IS NOT NULL AND EndLocalTime IS NOT NULL AND StartLocalTime <> EndLocalTime AND ActiveDaysMask BETWEEN 1 AND 127)
    )
);
GO

CREATE INDEX IX_PlaylistSlides_Screen_SortOrder ON dbo.PlaylistSlides (VenueId, ScreenId, SortOrder, Id);
GO
