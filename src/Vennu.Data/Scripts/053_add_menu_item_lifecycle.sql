IF COL_LENGTH('dbo.MenuItems', 'IsActive') IS NULL
BEGIN
    ALTER TABLE dbo.MenuItems
        ADD IsActive BIT NOT NULL
            CONSTRAINT DF_MenuItems_IsActive DEFAULT 1 WITH VALUES;
END;
