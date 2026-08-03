IF EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'StripeWebhook:SigningSecret')
AND NOT EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:Webhook:SigningSecret')
    UPDATE dbo.SystemConfigurationDefinitions SET [Key]=N'Stripe:Webhook:SigningSecret' WHERE ApplicationScope=N'API' AND [Key]=N'StripeWebhook:SigningSecret';
IF EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:SecretKey')
AND NOT EXISTS (SELECT 1 FROM dbo.SystemConfigurationDefinitions WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:Revenue:ApiKey')
    UPDATE dbo.SystemConfigurationDefinitions SET [Key]=N'Stripe:Revenue:ApiKey' WHERE ApplicationScope=N'API' AND [Key]=N'Stripe:SecretKey';

DECLARE @Definitions TABLE
(
    [Key] NVARCHAR(300), [Description] NVARCHAR(1000), ValueType NVARCHAR(30), IsSecret BIT,
    DefaultValue NVARCHAR(MAX), ValidationPattern NVARCHAR(1000), RequiresRestart BIT, ExportPolicy NVARCHAR(30)
);
INSERT INTO @Definitions VALUES
(N'SuperAdmin:ApiKey',N'Super Admin API access key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'CustomerAuthentication:Google:Enabled',N'Enables Google customer sign-in.',N'Boolean',0,N'false',NULL,1,N'Include'),
(N'CustomerAuthentication:Google:ClientId',N'Google OAuth client identifier.',N'String',0,NULL,NULL,1,N'Include'),
(N'CustomerAuthentication:Google:ClientSecret',N'Google OAuth client secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'CustomerAuthentication:Apple:Enabled',N'Enables Apple customer sign-in.',N'Boolean',0,N'false',NULL,1,N'Include'),
(N'CustomerAuthentication:Apple:ClientId',N'Apple Services ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'CustomerAuthentication:Apple:ClientSecret',N'Apple client-secret JWT.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'CustomerAuthentication:EmailDelivery:Enabled',N'Enables customer email delivery.',N'Boolean',0,N'false',NULL,1,N'Include'),
(N'CustomerAuthentication:EmailDelivery:Endpoint',N'Customer email delivery HTTPS endpoint.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'CustomerAuthentication:EmailDelivery:ApiKey',N'Customer email delivery API key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Stripe:Revenue:ApiKey',N'Stripe server API key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Stripe:Webhook:SigningSecret',N'Stripe webhook signing secret.',N'String',1,NULL,N'^whsec_',1,N'Exclude'),
(N'Stripe:Webhook:ToleranceSeconds',N'Stripe webhook signature tolerance.',N'Integer',0,N'300',N'^[0-9]+$',0,N'Include'),
(N'Stripe:Checkout:SuccessUrl',N'Stripe checkout success URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:Checkout:CancelUrl',N'Stripe checkout cancellation URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:BillingPortal:ReturnUrl',N'Stripe billing portal return URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:HaasCheckout:SuccessUrl',N'HaaS checkout success URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:HaasCheckout:CancelUrl',N'HaaS checkout cancellation URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Stripe:HaasCheckout:PriceIds:starter_kit',N'Stripe price ID for the Starter Kit HaaS bundle.',N'String',0,NULL,N'^price_',1,N'Include'),
(N'Stripe:HaasCheckout:PriceIds:bar_pack',N'Stripe price ID for the Bar Pack HaaS bundle.',N'String',0,NULL,N'^price_',1,N'Include'),
(N'Stripe:HaasCheckout:PriceIds:full_house',N'Stripe price ID for the Full House HaaS bundle.',N'String',0,NULL,N'^price_',1,N'Include'),
(N'Square:OAuth:ApplicationId',N'Square OAuth application ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'Square:OAuth:ApplicationSecret',N'Square OAuth application secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Square:OAuth:AuthorizationEndpoint',N'Square OAuth authorization endpoint.',N'Uri',0,N'https://connect.squareup.com/oauth2/authorize',N'^https://',1,N'Include'),
(N'Square:OAuth:TokenEndpoint',N'Square OAuth token endpoint.',N'Uri',0,N'https://connect.squareup.com/oauth2/token',N'^https://',1,N'Include'),
(N'Square:OAuth:RevokeEndpoint',N'Square OAuth revoke endpoint.',N'Uri',0,N'https://connect.squareup.com/oauth2/revoke',N'^https://',1,N'Include'),
(N'Square:OAuth:CallbackUrl',N'Square OAuth callback URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Square:OAuth:VenueAdminReturnUrl',N'Square Venue Admin return URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Square:OAuth:ApiVersion',N'Square API version.',N'String',0,N'2026-07-15',NULL,1,N'Include'),
(N'Square:OAuth:Scopes:0',N'First Square OAuth scope.',N'String',0,N'MERCHANT_PROFILE_READ',NULL,1,N'Include'),
(N'Square:OAuth:Scopes:1',N'Second Square OAuth scope.',N'String',0,N'ITEMS_READ',NULL,1,N'Include'),
(N'Square:OAuth:Scopes:2',N'Third Square OAuth scope.',N'String',0,N'INVENTORY_READ',NULL,1,N'Include'),
(N'Square:Catalog:Endpoint',N'Square catalog endpoint.',N'Uri',0,N'https://connect.squareup.com/v2/catalog/list',N'^https://',0,N'Include'),
(N'Square:Catalog:ApiVersion',N'Square catalog API version.',N'String',0,N'2026-07-15',NULL,0,N'Include'),
(N'Square:Webhooks:SignatureKey',N'Square webhook signature key.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Square:Webhooks:NotificationUrl',N'Square webhook notification URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Toast:Catalog:Endpoint',N'Toast menu endpoint.',N'Uri',0,N'https://ws-api.toasttab.com/menus/v2/menus',N'^https://',0,N'Include'),
(N'Toast:Catalog:CurrencyCode',N'Toast catalog currency.',N'String',0,N'USD',N'^[A-Z]{3}$',0,N'Include'),
(N'Toast:Inventory:Endpoint',N'Toast inventory endpoint.',N'Uri',0,N'https://ws-api.toasttab.com/stock/v1/inventory/search',N'^https://',0,N'Include'),
(N'Toast:Inventory:MaximumItemsPerRequest',N'Toast inventory batch size.',N'Integer',0,N'100',N'^[0-9]+$',0,N'Include'),
(N'Toast:Polling:PollInterval',N'Toast polling interval.',N'String',0,N'01:00:00',NULL,0,N'Include'),
(N'Toast:Polling:InterConnectionDelay',N'Delay between Toast connection polls.',N'String',0,N'00:00:00.250',NULL,0,N'Include'),
(N'Toast:Polling:InitialFailureBackoff',N'Toast initial retry backoff.',N'String',0,N'00:05:00',NULL,0,N'Include'),
(N'Toast:Polling:MaximumFailureBackoff',N'Toast maximum retry backoff.',N'String',0,N'01:00:00',NULL,0,N'Include'),
(N'Toast:Webhooks:MenusSecret',N'Toast menu webhook secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Toast:Webhooks:StockSecret',N'Toast stock webhook secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Clover:OAuth:ClientId',N'Clover OAuth client ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'Clover:OAuth:ClientSecret',N'Clover OAuth client secret.',N'String',1,NULL,NULL,1,N'Exclude'),
(N'Clover:OAuth:AuthorizationEndpoint',N'Clover OAuth authorization endpoint.',N'Uri',0,N'https://www.clover.com/oauth/v2/authorize',N'^https://',1,N'Include'),
(N'Clover:OAuth:TokenEndpoint',N'Clover OAuth token endpoint.',N'Uri',0,N'https://api.clover.com/oauth/v2/token',N'^https://',1,N'Include'),
(N'Clover:OAuth:CallbackUrl',N'Clover OAuth callback URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Clover:OAuth:VenueAdminReturnUrl',N'Clover Venue Admin return URL.',N'Uri',0,NULL,N'^https://',1,N'Include'),
(N'Clover:Catalog:BaseUrl',N'Clover API base URL.',N'Uri',0,N'https://api.clover.com',N'^https://',0,N'Include'),
(N'Clover:Catalog:CurrencyCode',N'Clover catalog currency.',N'String',0,N'USD',N'^[A-Z]{3}$',0,N'Include'),
(N'Clover:Catalog:PageSize',N'Clover catalog page size.',N'Integer',0,N'1000',N'^[0-9]+$',0,N'Include'),
(N'Clover:Webhooks:AppId',N'Clover webhook app ID.',N'String',0,NULL,NULL,1,N'Include'),
(N'Clover:Webhooks:AuthCode',N'Clover webhook authorization code.',N'String',1,NULL,NULL,1,N'Exclude');

MERGE dbo.SystemConfigurationDefinitions AS target
USING @Definitions AS source ON target.ApplicationScope=N'API' AND target.[Key]=source.[Key]
WHEN MATCHED THEN UPDATE SET [Description]=source.[Description],ValueType=source.ValueType,IsSecret=source.IsSecret,DefaultValue=source.DefaultValue,ValidationPattern=source.ValidationPattern,RequiresRestart=source.RequiresRestart,ExportPolicy=source.ExportPolicy,UpdatedUtc=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT(Id,[Key],ApplicationScope,[Description],ValueType,IsRequired,IsSecret,DefaultValue,ValidationPattern,RequiresRestart,ExportPolicy)
VALUES(NEWID(),source.[Key],N'API',source.[Description],source.ValueType,0,source.IsSecret,source.DefaultValue,source.ValidationPattern,source.RequiresRestart,source.ExportPolicy);
