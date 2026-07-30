CREATE TABLE dbo.TapCategories
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TapCategories PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    CategoryPrice DECIMAL(19, 4) NULL,
    SortOrder INT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_TapCategories_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapCategories_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapCategories_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TapCategories_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TapCategories_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_TapCategories_VenueId_SortOrder UNIQUE (VenueId, SortOrder),
    CONSTRAINT CK_TapCategories_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_TapCategories_Price_NonNegative CHECK (CategoryPrice IS NULL OR CategoryPrice >= 0),
    CONSTRAINT CK_TapCategories_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_TapCategories_VenueId_Order ON dbo.TapCategories (VenueId, SortOrder, Id);
GO

CREATE TABLE dbo.TapItems
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TapItems PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    TapCategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Style NVARCHAR(160) NULL,
    Abv DECIMAL(5, 2) NULL,
    Ibu INT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(19, 4) NOT NULL,
    GlassColor CHAR(7) NULL,
    NameColor CHAR(7) NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_TapItems_IsAvailable DEFAULT 1,
    IsComingSoon BIT NOT NULL CONSTRAINT DF_TapItems_IsComingSoon DEFAULT 0,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapItems_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_TapItems_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_TapItems_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_TapItems_Categories FOREIGN KEY (TapCategoryId, VenueId)
        REFERENCES dbo.TapCategories (Id, VenueId),
    CONSTRAINT UQ_TapItems_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_TapItems_VenueId_SortOrder UNIQUE (VenueId, SortOrder),
    CONSTRAINT CK_TapItems_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_TapItems_Abv_Range CHECK (Abv IS NULL OR Abv BETWEEN 0 AND 100),
    CONSTRAINT CK_TapItems_Ibu_Range CHECK (Ibu IS NULL OR Ibu BETWEEN 0 AND 1000),
    CONSTRAINT CK_TapItems_Price_NonNegative CHECK (Price >= 0),
    CONSTRAINT CK_TapItems_GlassColor CHECK (GlassColor IS NULL OR GlassColor LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'),
    CONSTRAINT CK_TapItems_NameColor CHECK (NameColor IS NULL OR NameColor LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'),
    CONSTRAINT CK_TapItems_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_TapItems_VenueId_Order ON dbo.TapItems (VenueId, SortOrder, Id);
GO
