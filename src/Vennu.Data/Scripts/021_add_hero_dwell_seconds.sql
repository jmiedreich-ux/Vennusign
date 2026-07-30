ALTER TABLE dbo.Screens
ADD HeroDwellSeconds INT NOT NULL
    CONSTRAINT DF_Screens_HeroDwellSeconds DEFAULT 8,
    CONSTRAINT CK_Screens_HeroDwellSeconds CHECK (HeroDwellSeconds BETWEEN 4 AND 30);
GO
