IF NOT EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope = N'Api' AND [Key] = N'CustomerAuthentication:FrontendOrigin')
BEGIN
    INSERT dbo.SystemConfigurationDefinitions
        (Id, ApplicationScope, [Key], [Description], ValueType, IsRequired, IsSecret, DefaultValue, ValidationPattern, RequiresRestart, ExportPolicy, RotationReminderDays)
    VALUES
        (NEWID(), N'Api', N'CustomerAuthentication:FrontendOrigin', N'Trusted HTTPS origin used after customer authentication callbacks.', N'Uri', 1, 0, NULL, N'^https://[^/]+(?::[0-9]+)?$', 1, N'Include', NULL);
END;
