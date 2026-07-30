CREATE TABLE dbo.VenueThemes
(
    VenueId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_VenueThemes PRIMARY KEY,
    BackgroundColor CHAR(7) NOT NULL,
    AccentColor CHAR(7) NOT NULL,
    FontFamily NVARCHAR(50) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_VenueThemes_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_VenueThemes_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT CK_VenueThemes_BackgroundColor CHECK (BackgroundColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_AccentColor CHECK (AccentColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_FontFamily CHECK (FontFamily IN ('Inter', 'Georgia', 'Arial'))
);
GO
