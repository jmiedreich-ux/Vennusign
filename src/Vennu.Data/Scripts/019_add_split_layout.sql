ALTER TABLE dbo.Screens
DROP CONSTRAINT CK_Screens_DisplayLayout;
GO

ALTER TABLE dbo.Screens
ADD SplitRatio NVARCHAR(5) NOT NULL
    CONSTRAINT DF_Screens_SplitRatio DEFAULT '40_60';
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
        CHECK (DisplayLayout IN ('photo_grid', 'classic_diner', 'neon_chalkboard', 'split_layout')),
    CONSTRAINT CK_Screens_SplitRatio
        CHECK (SplitRatio IN ('40_60', '50_50'));
GO
