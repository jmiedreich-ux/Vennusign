ALTER TABLE dbo.VenueThemes
ADD PresetKey NVARCHAR(30) NOT NULL
        CONSTRAINT DF_VenueThemes_PresetKey DEFAULT 'bar_classic',
    TitleColor CHAR(7) NOT NULL
        CONSTRAINT DF_VenueThemes_TitleColor DEFAULT '#F8F5E9',
    GlowColor CHAR(7) NOT NULL
        CONSTRAINT DF_VenueThemes_GlowColor DEFAULT '#00E5FF',
    BoardBackgroundColor CHAR(7) NOT NULL
        CONSTRAINT DF_VenueThemes_BoardBackgroundColor DEFAULT '#071013',
    SectionColors NVARCHAR(64) NOT NULL
        CONSTRAINT DF_VenueThemes_SectionColors DEFAULT '#00E5FF,#FF2BD6,#FFE66D,#7CFF6B',
    GlowIntensity DECIMAL(3, 2) NOT NULL
        CONSTRAINT DF_VenueThemes_GlowIntensity DEFAULT 1.00,
    TitleFont NVARCHAR(40) NOT NULL
        CONSTRAINT DF_VenueThemes_TitleFont DEFAULT 'Righteous',
    ItemFont NVARCHAR(40) NOT NULL
        CONSTRAINT DF_VenueThemes_ItemFont DEFAULT 'Caveat';
GO

ALTER TABLE dbo.VenueThemes
ADD CONSTRAINT CK_VenueThemes_PresetKey
        CHECK (PresetKey IN ('custom', 'bar_classic', 'violet_lounge', 'hot_summer', 'ocean_dive', 'rose_gold')),
    CONSTRAINT CK_VenueThemes_TitleColor
        CHECK (TitleColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_GlowColor
        CHECK (GlowColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_BoardBackgroundColor
        CHECK (BoardBackgroundColor LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'),
    CONSTRAINT CK_VenueThemes_SectionColors
        CHECK (LEN(SectionColors) BETWEEN 7 AND 64),
    CONSTRAINT CK_VenueThemes_GlowIntensity
        CHECK (GlowIntensity BETWEEN 0.20 AND 2.00),
    CONSTRAINT CK_VenueThemes_TitleFont
        CHECK (TitleFont IN ('Pacifico', 'Lobster', 'Righteous', 'Fredoka One', 'Bungee', 'Permanent Marker')),
    CONSTRAINT CK_VenueThemes_ItemFont
        CHECK (ItemFont IN ('Caveat', 'Kalam', 'Patrick Hand', 'Permanent Marker'));
GO
