ALTER TABLE dbo.Screens
ADD DisplayLayout NVARCHAR(30) NOT NULL
    CONSTRAINT DF_Screens_DisplayLayout DEFAULT 'photo_grid';
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
    CHECK (DisplayLayout IN ('photo_grid', 'classic_diner'));
GO
