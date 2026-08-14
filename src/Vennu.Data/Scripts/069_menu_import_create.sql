/*
    Menus 6-A2: create an unpublished menu from a resolved import.

    WHAT THIS DISCARDS: nothing. Destination and completion state extend the
    temporary session; placement price overrides isolate pasted prices to the
    imported menu instead of mutating a shared library item.
*/

ALTER TABLE dbo.MenuImportSessions ADD
    Destination NVARCHAR(16) NULL,
    ProposedMenuName NVARCHAR(200) NULL,
    CompletedMenuId UNIQUEIDENTIFIER NULL,
    CompletedUtc DATETIME2(7) NULL;
GO

ALTER TABLE dbo.MenuImportSessions ADD
    CONSTRAINT FK_MenuImportSessions_CompletedMenu
        FOREIGN KEY (CompletedMenuId, VenueId) REFERENCES dbo.Menus (Id, VenueId),
    CONSTRAINT CK_MenuImportSessions_Destination
        CHECK (Destination IS NULL OR Destination IN (N'create')),
    CONSTRAINT CK_MenuImportSessions_Completion
        CHECK ((CompletedMenuId IS NULL AND CompletedUtc IS NULL) OR
               (CompletedMenuId IS NOT NULL AND CompletedUtc IS NOT NULL AND Destination=N'create'));
GO

ALTER TABLE dbo.Placements ADD ImportedPriceOverride NVARCHAR(12) NULL;
GO

CREATE TABLE dbo.MenuImportCreatedLines
(
    SessionId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    LineNumber INT NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    MenuSectionId UNIQUEIDENTIFIER NOT NULL,
    PlacementId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_MenuImportCreatedLines PRIMARY KEY (SessionId, LineNumber),
    CONSTRAINT FK_MenuImportCreatedLines_Session FOREIGN KEY (SessionId, VenueId)
        REFERENCES dbo.MenuImportSessions (Id, VenueId),
    CONSTRAINT FK_MenuImportCreatedLines_Menu FOREIGN KEY (MenuId, VenueId)
        REFERENCES dbo.Menus (Id, VenueId),
    CONSTRAINT FK_MenuImportCreatedLines_Section FOREIGN KEY (MenuSectionId, VenueId)
        REFERENCES dbo.MenuSections (Id, VenueId),
    CONSTRAINT FK_MenuImportCreatedLines_Placement FOREIGN KEY (PlacementId)
        REFERENCES dbo.Placements (Id),
    CONSTRAINT UQ_MenuImportCreatedLines_Placement UNIQUE (PlacementId)
);
GO
