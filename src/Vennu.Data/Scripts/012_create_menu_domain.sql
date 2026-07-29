CREATE TABLE dbo.Menus
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Menus PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Menus_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Menus_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Menus_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Menus_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_Menus_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT CK_Menus_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
);
GO

CREATE INDEX IX_Menus_VenueId_Name ON dbo.Menus (VenueId, Name, Id);
GO

CREATE TABLE dbo.MenuSections
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuSections PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    SortOrder INT NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_MenuSections_IsActive DEFAULT 1,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuSections_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuSections_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MenuSections_Menus FOREIGN KEY (MenuId, VenueId)
        REFERENCES dbo.Menus (Id, VenueId) ON DELETE CASCADE,
    CONSTRAINT UQ_MenuSections_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_MenuSections_MenuId_SortOrder UNIQUE (MenuId, SortOrder),
    CONSTRAINT CK_MenuSections_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_MenuSections_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_MenuSections_VenueId_MenuId_Order
    ON dbo.MenuSections (VenueId, MenuId, SortOrder, Id);
GO

CREATE TABLE dbo.MenuItems
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuItems PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuSectionId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(19, 4) NOT NULL,
    HappyHourPrice DECIMAL(19, 4) NULL,
    IsAvailable BIT NOT NULL CONSTRAINT DF_MenuItems_IsAvailable DEFAULT 1,
    QuantityAvailable INT NULL,
    Tags NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(2048) NULL,
    IsPopular BIT NOT NULL CONSTRAINT DF_MenuItems_IsPopular DEFAULT 0,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuItems_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuItems_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MenuItems_MenuSections FOREIGN KEY (MenuSectionId, VenueId)
        REFERENCES dbo.MenuSections (Id, VenueId) ON DELETE CASCADE,
    CONSTRAINT UQ_MenuItems_Id_VenueId UNIQUE (Id, VenueId),
    CONSTRAINT UQ_MenuItems_SectionId_SortOrder UNIQUE (MenuSectionId, SortOrder),
    CONSTRAINT CK_MenuItems_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_MenuItems_Price_NonNegative CHECK (Price >= 0),
    CONSTRAINT CK_MenuItems_HappyHourPrice_NonNegative CHECK (HappyHourPrice IS NULL OR HappyHourPrice >= 0),
    CONSTRAINT CK_MenuItems_QuantityAvailable_NonNegative CHECK (QuantityAvailable IS NULL OR QuantityAvailable >= 0),
    CONSTRAINT CK_MenuItems_SortOrder_NonNegative CHECK (SortOrder >= 0)
);
GO

CREATE INDEX IX_MenuItems_VenueId_SectionId_Order
    ON dbo.MenuItems (VenueId, MenuSectionId, SortOrder, Id);
GO

CREATE TABLE dbo.MenuItemTranslations
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuItemTranslations PRIMARY KEY DEFAULT NEWID(),
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuItemId UNIQUEIDENTIFIER NOT NULL,
    LanguageCode NVARCHAR(10) NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    IsAutoTranslated BIT NOT NULL CONSTRAINT DF_MenuItemTranslations_IsAutoTranslated DEFAULT 0,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuItemTranslations_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_MenuItemTranslations_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_MenuItemTranslations_MenuItems FOREIGN KEY (MenuItemId, VenueId)
        REFERENCES dbo.MenuItems (Id, VenueId) ON DELETE CASCADE,
    CONSTRAINT UQ_MenuItemTranslations_Item_Language UNIQUE (MenuItemId, LanguageCode),
    CONSTRAINT CK_MenuItemTranslations_LanguageCode_NotBlank CHECK (LEN(LTRIM(RTRIM(LanguageCode))) > 0),
    CONSTRAINT CK_MenuItemTranslations_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
);
GO

CREATE INDEX IX_MenuItemTranslations_VenueId_ItemId_Language
    ON dbo.MenuItemTranslations (VenueId, MenuItemId, LanguageCode, Id);
GO
