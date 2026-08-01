CREATE TABLE dbo.PosCatalogMappings
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PosCatalogMappings PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    Provider INT NOT NULL,
    EntityType INT NOT NULL,
    ExternalId NVARCHAR(300) NOT NULL,
    LocalEntityId UNIQUEIDENTIFIER NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosCatalogMappings_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_PosCatalogMappings_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PosCatalogMappings_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_PosCatalogMappings_Source UNIQUE (VenueId, Provider, EntityType, ExternalId),
    CONSTRAINT CK_PosCatalogMappings_Provider CHECK (Provider IN (1, 2, 3)),
    CONSTRAINT CK_PosCatalogMappings_EntityType CHECK (EntityType IN (1, 2, 3, 4)),
    CONSTRAINT CK_PosCatalogMappings_ExternalId CHECK (LEN(LTRIM(RTRIM(ExternalId))) > 0)
);
GO

CREATE INDEX IX_PosCatalogMappings_LocalEntity
    ON dbo.PosCatalogMappings (VenueId, Provider, EntityType, LocalEntityId);
GO
