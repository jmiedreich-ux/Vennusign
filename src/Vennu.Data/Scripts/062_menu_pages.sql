/*
    Menus M3-A slice 1: pages are first-class children of a menu and screen
    assignment targets a page. Every existing menu receives one "Page 1" and all
    existing sections/assignments move to it.

    WHAT THIS DISCARDS: nothing. No menu, section, placement, assignment or publish
    history row is removed. Existing menu-level assignments become assignments to
    that menu's carried-forward first page.
*/

CREATE TABLE dbo.MenuPages
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuPages PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    MenuId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    SortOrder INT NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_MenuPages_Menus FOREIGN KEY (MenuId, VenueId) REFERENCES dbo.Menus (Id, VenueId) ON DELETE CASCADE,
    CONSTRAINT UQ_MenuPages_Id_Venue UNIQUE (Id, VenueId),
    CONSTRAINT UQ_MenuPages_Id_Menu_Venue UNIQUE (Id, MenuId, VenueId),
    CONSTRAINT UQ_MenuPages_Menu_Order UNIQUE (MenuId, SortOrder),
    CONSTRAINT CK_MenuPages_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
    CONSTRAINT CK_MenuPages_Order_NonNegative CHECK (SortOrder >= 0)
);
CREATE INDEX IX_MenuPages_Venue_Menu_Order ON dbo.MenuPages (VenueId, MenuId, SortOrder, Id);
GO

INSERT dbo.MenuPages (Id, VenueId, MenuId, Name, SortOrder, CreatedUtc, UpdatedUtc)
SELECT NEWID(), VenueId, Id, N'Page 1', 0, CreatedUtc, UpdatedUtc
FROM dbo.Menus;
GO

ALTER TABLE dbo.MenuSections ADD PageId UNIQUEIDENTIFIER NULL;
GO
UPDATE s SET PageId = p.Id
FROM dbo.MenuSections s
INNER JOIN dbo.MenuPages p ON p.MenuId = s.MenuId AND p.VenueId = s.VenueId AND p.SortOrder = 0;
ALTER TABLE dbo.MenuSections ALTER COLUMN PageId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE dbo.MenuSections DROP CONSTRAINT UQ_MenuSections_MenuId_SortOrder;
ALTER TABLE dbo.MenuSections ADD
    CONSTRAINT FK_MenuSections_Pages FOREIGN KEY (PageId, VenueId) REFERENCES dbo.MenuPages (Id, VenueId),
    CONSTRAINT FK_MenuSections_PageOnMenu FOREIGN KEY (PageId, MenuId, VenueId) REFERENCES dbo.MenuPages (Id, MenuId, VenueId),
    CONSTRAINT UQ_MenuSections_Id_Page_Menu_Venue UNIQUE (Id, PageId, MenuId, VenueId),
    CONSTRAINT UQ_MenuSections_Page_Order UNIQUE (PageId, SortOrder);
DROP INDEX IX_MenuSections_VenueId_MenuId_Order ON dbo.MenuSections;
CREATE INDEX IX_MenuSections_Venue_Page_Order ON dbo.MenuSections (VenueId, PageId, SortOrder, Id);
GO

ALTER TABLE dbo.MenuScreenAssignments ADD PageId UNIQUEIDENTIFIER NULL;
GO
UPDATE a SET PageId = p.Id
FROM dbo.MenuScreenAssignments a
INNER JOIN dbo.MenuPages p ON p.MenuId = a.MenuId AND p.VenueId = a.VenueId AND p.SortOrder = 0;
ALTER TABLE dbo.MenuScreenAssignments ALTER COLUMN PageId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE dbo.MenuScreenAssignments DROP CONSTRAINT UQ_MenuScreenAssignments_Screen;
ALTER TABLE dbo.MenuScreenAssignments ADD
    CONSTRAINT FK_MenuScreenAssignments_Pages FOREIGN KEY (PageId, VenueId) REFERENCES dbo.MenuPages (Id, VenueId),
    CONSTRAINT FK_MenuScreenAssignments_PageOnMenu FOREIGN KEY (PageId, MenuId, VenueId) REFERENCES dbo.MenuPages (Id, MenuId, VenueId),
    CONSTRAINT UQ_MenuScreenAssignments_Screen_Page UNIQUE (ScreenId, PageId);
CREATE INDEX IX_MenuScreenAssignments_Page ON dbo.MenuScreenAssignments (VenueId, PageId);
GO

-- Carry the owning page onto each placement so "once per page" is enforceable by
-- a unique key under concurrent writes, rather than by an application pre-check.
ALTER TABLE dbo.Placements ADD PageId UNIQUEIDENTIFIER NULL;
GO
UPDATE p SET PageId = s.PageId
FROM dbo.Placements p INNER JOIN dbo.MenuSections s ON s.Id = p.MenuSectionId AND s.VenueId = p.VenueId;
ALTER TABLE dbo.Placements ALTER COLUMN PageId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE dbo.Placements DROP CONSTRAINT UQ_Placements_MenuItem;
ALTER TABLE dbo.Placements ADD
    CONSTRAINT FK_Placements_PageOnMenu FOREIGN KEY (PageId, MenuId, VenueId) REFERENCES dbo.MenuPages (Id, MenuId, VenueId),
    CONSTRAINT FK_Placements_SectionOnPage FOREIGN KEY (MenuSectionId, PageId, MenuId, VenueId) REFERENCES dbo.MenuSections (Id, PageId, MenuId, VenueId) ON UPDATE CASCADE,
    CONSTRAINT UQ_Placements_PageItem UNIQUE (PageId, ItemId);
GO

ALTER TABLE dbo.Screens ADD
    WidthPixels INT NOT NULL CONSTRAINT DF_Screens_WidthPixels DEFAULT (1920),
    HeightPixels INT NOT NULL CONSTRAINT DF_Screens_HeightPixels DEFAULT (1080),
    CONSTRAINT CK_Screens_Geometry_Positive CHECK (WidthPixels > 0 AND HeightPixels > 0);
GO

