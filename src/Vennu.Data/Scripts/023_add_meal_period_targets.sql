ALTER TABLE dbo.MealPeriods
ADD TargetLayout NVARCHAR(50) NULL,
    MenuFilter NVARCHAR(100) NULL,
    ThemePresetKey NVARCHAR(50) NULL;
GO

ALTER TABLE dbo.MealPeriods
ADD CONSTRAINT CK_MealPeriods_TargetLayout CHECK
(
    TargetLayout IS NULL OR TargetLayout IN
    ('photo_grid', 'classic_diner', 'neon_chalkboard', 'split_layout', 'daily_special_hero')
);
GO
