IF COL_LENGTH('dbo.PosConnections', 'RefreshTokenExpiresUtc') IS NULL
BEGIN
    ALTER TABLE dbo.PosConnections
        ADD RefreshTokenExpiresUtc DATETIME2(7) NULL;
END;
