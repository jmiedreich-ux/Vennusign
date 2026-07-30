ALTER TABLE dbo.Screens
ADD PhotoGridDensity NVARCHAR(3) NOT NULL
    CONSTRAINT DF_Screens_PhotoGridDensity DEFAULT '3x2';
GO

ALTER TABLE dbo.Screens
ADD CONSTRAINT CK_Screens_PhotoGridDensity
    CHECK (PhotoGridDensity IN ('2x2', '3x2', '4x2', '3x3'));
GO
