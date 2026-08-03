SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.SystemConfigurationDefinitions', N'U') IS NOT NULL
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM dbo.SystemConfigurationDefinitions legacy
        JOIN dbo.SystemConfigurationDefinitions canonical
          ON canonical.ApplicationScope = CASE legacy.ApplicationScope
              WHEN N'Admin' THEN N'PlatformOperations'
              WHEN N'VenueAdmin' THEN N'BackOffice'
          END
         AND canonical.[Key] = legacy.[Key]
        WHERE legacy.ApplicationScope IN (N'Admin', N'VenueAdmin')
    )
        THROW 51020, 'Administrative scope migration found a canonical duplicate.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SystemConfigurationDefinitions legacy
        JOIN dbo.SystemConfigurationDefinitions canonical
          ON canonical.ApplicationScope = legacy.ApplicationScope
         AND canonical.[Key] = CASE legacy.[Key]
              WHEN N'SuperAdmin:ApiKey' THEN N'PlatformOperations:ApiKey'
              WHEN N'Square:OAuth:VenueAdminReturnUrl' THEN N'Square:OAuth:BackOfficeReturnUrl'
              WHEN N'Clover:OAuth:VenueAdminReturnUrl' THEN N'Clover:OAuth:BackOfficeReturnUrl'
          END
        WHERE legacy.[Key] IN
        (
            N'SuperAdmin:ApiKey',
            N'Square:OAuth:VenueAdminReturnUrl',
            N'Clover:OAuth:VenueAdminReturnUrl'
        )
    )
        THROW 51021, 'Administrative key migration found a canonical duplicate.', 1;

    ALTER TABLE dbo.SystemConfigurationDefinitions
        DROP CONSTRAINT CK_SystemConfigurationDefinitions_Scope;

    UPDATE dbo.SystemConfigurationDefinitions
    SET ApplicationScope = CASE ApplicationScope
        WHEN N'Admin' THEN N'PlatformOperations'
        WHEN N'VenueAdmin' THEN N'BackOffice'
        ELSE ApplicationScope
    END,
    UpdatedUtc = SYSUTCDATETIME()
    WHERE ApplicationScope IN (N'Admin', N'VenueAdmin');

    UPDATE dbo.SystemConfigurationDefinitions
    SET [Key] = CASE [Key]
        WHEN N'SuperAdmin:ApiKey' THEN N'PlatformOperations:ApiKey'
        WHEN N'Square:OAuth:VenueAdminReturnUrl' THEN N'Square:OAuth:BackOfficeReturnUrl'
        WHEN N'Clover:OAuth:VenueAdminReturnUrl' THEN N'Clover:OAuth:BackOfficeReturnUrl'
        ELSE [Key]
    END,
    UpdatedUtc = SYSUTCDATETIME()
    WHERE [Key] IN
    (
        N'SuperAdmin:ApiKey',
        N'Square:OAuth:VenueAdminReturnUrl',
        N'Clover:OAuth:VenueAdminReturnUrl'
    );

    ALTER TABLE dbo.SystemConfigurationDefinitions WITH CHECK
        ADD CONSTRAINT CK_SystemConfigurationDefinitions_Scope CHECK
        (
            ApplicationScope IN
            (
                N'Shared', N'API', N'PlatformOperations', N'BackOffice', N'Display', N'Background'
            )
        );

    IF EXISTS
    (
        SELECT 1
        FROM dbo.SystemConfigurationDefinitions
        WHERE ApplicationScope IN (N'Admin', N'VenueAdmin')
           OR [Key] IN
           (
               N'SuperAdmin:ApiKey',
               N'Square:OAuth:VenueAdminReturnUrl',
               N'Clover:OAuth:VenueAdminReturnUrl'
           )
    )
        THROW 51022, 'Administrative identity migration verification failed.', 1;
END;

COMMIT TRANSACTION;
