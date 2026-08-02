DECLARE @Definitions TABLE
(
    [Key] NVARCHAR(300), ApplicationScope NVARCHAR(50), [Description] NVARCHAR(1000), ValueType NVARCHAR(30),
    IsRequired BIT, IsSecret BIT, DefaultValue NVARCHAR(MAX), ValidationPattern NVARCHAR(1000), RequiresRestart BIT, ExportPolicy NVARCHAR(30)
);

INSERT INTO @Definitions VALUES
(N'CustomerAuthentication:Google:Enabled', N'API', N'Enables Google customer sign-in.', N'Boolean', 0, 0, N'false', NULL, 1, N'Include'),
(N'CustomerAuthentication:Google:ClientId', N'API', N'Google OAuth client identifier.', N'String', 0, 0, NULL, NULL, 1, N'Include'),
(N'CustomerAuthentication:Google:ClientSecret', N'API', N'Google OAuth client secret.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'CustomerAuthentication:Apple:Enabled', N'API', N'Enables Apple customer sign-in.', N'Boolean', 0, 0, N'false', NULL, 1, N'Include'),
(N'CustomerAuthentication:Apple:ClientId', N'API', N'Apple Services ID.', N'String', 0, 0, NULL, NULL, 1, N'Include'),
(N'CustomerAuthentication:Apple:ClientSecret', N'API', N'Apple client-secret JWT.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'CustomerAuthentication:EmailDelivery:Enabled', N'API', N'Enables customer email-link delivery.', N'Boolean', 0, 0, N'false', NULL, 1, N'Include'),
(N'CustomerAuthentication:EmailDelivery:Endpoint', N'API', N'HTTPS customer email delivery endpoint.', N'Uri', 0, 0, NULL, N'^https://', 1, N'Include'),
(N'CustomerAuthentication:EmailDelivery:ApiKey', N'API', N'Customer email delivery API key.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'Stripe:SecretKey', N'API', N'Stripe server API secret.', N'String', 0, 1, NULL, NULL, 1, N'Exclude'),
(N'StripeWebhook:SigningSecret', N'API', N'Stripe webhook signing secret.', N'String', 0, 1, NULL, N'^whsec_', 1, N'Exclude');

MERGE dbo.SystemConfigurationDefinitions AS target
USING @Definitions AS source
ON target.ApplicationScope = source.ApplicationScope AND target.[Key] = source.[Key]
WHEN MATCHED THEN UPDATE SET
    [Description] = source.[Description], ValueType = source.ValueType, IsRequired = source.IsRequired,
    IsSecret = source.IsSecret, DefaultValue = source.DefaultValue, ValidationPattern = source.ValidationPattern,
    RequiresRestart = source.RequiresRestart, ExportPolicy = source.ExportPolicy, UpdatedUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT
    (Id, [Key], ApplicationScope, [Description], ValueType, IsRequired, IsSecret, DefaultValue, ValidationPattern, RequiresRestart, ExportPolicy)
VALUES
    (NEWID(), source.[Key], source.ApplicationScope, source.[Description], source.ValueType, source.IsRequired,
     source.IsSecret, source.DefaultValue, source.ValidationPattern, source.RequiresRestart, source.ExportPolicy);
