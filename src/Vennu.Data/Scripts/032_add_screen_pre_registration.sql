ALTER TABLE dbo.Screens
ADD DesiredAppVersion NVARCHAR(50) NULL,
    DeliveryReference NVARCHAR(100) NULL,
    PreRegistrationTokenHash CHAR(64) NULL,
    PreRegistrationExpiresUtc DATETIME2(7) NULL,
    PreRegisteredUtc DATETIME2(7) NULL;
GO

CREATE UNIQUE INDEX UX_Screens_PreRegistrationTokenHash
    ON dbo.Screens (PreRegistrationTokenHash)
    WHERE PreRegistrationTokenHash IS NOT NULL;
GO
