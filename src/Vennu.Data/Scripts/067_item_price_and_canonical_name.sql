/*
    Menus M3-A slice 3: enforce the owner-approved price width and centralise
    library-name canonicalisation.

    WHAT THIS DISCARDS: nothing. The migration refuses before narrowing when an
    existing live item price exceeds 12 characters, naming those rows for repair.
    Published snapshot JSON remains unchanged and is still read at NVARCHAR(40),
    preserving historical content exactly as published.
*/

IF EXISTS (SELECT 1 FROM dbo.Items WHERE LEN(Price) > 12)
BEGIN
    SELECT Id, VenueId, Name, Price FROM dbo.Items WHERE LEN(Price) > 12;
    THROW 51067, 'Cannot narrow Items.Price to 12 characters: existing longer prices must be corrected first.', 1;
END;
GO

ALTER TABLE dbo.Items ALTER COLUMN Price NVARCHAR(12) NULL;
GO

CREATE OR ALTER FUNCTION dbo.CanonicalItemName(@Value NVARCHAR(200))
RETURNS NVARCHAR(600)
WITH SCHEMABINDING
AS
BEGIN
    DECLARE @Result NVARCHAR(600)=N'', @Index INT=1, @Character NCHAR(1);
    WHILE @Index <= LEN(ISNULL(@Value,N''))
    BEGIN
        SET @Character=SUBSTRING(LOWER(@Value),@Index,1);
        IF @Character=N'&' SET @Result+=N'and';
        ELSE IF @Character LIKE N'[0-9a-z]' COLLATE Latin1_General_100_BIN2 SET @Result+=@Character;
        SET @Index+=1;
    END;
    RETURN @Result;
END;
GO
