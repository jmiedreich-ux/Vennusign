ALTER TABLE dbo.Screens
DROP CONSTRAINT CK_Screens_DisplayLayout;
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_DisplayLayout
    CHECK (DisplayLayout IN (
        'photo_grid',
        'classic_diner',
        'neon_chalkboard',
        'split_layout',
        'daily_special_hero',
        'classic_chalkboard',
        'tap_strips'
    ));
GO
