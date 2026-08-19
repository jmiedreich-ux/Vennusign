using Vennu.Data;
using Vennu.Data.Extensions;
using Vennu.Api.Hubs;
using Vennu.Api.Notifications;
using Vennu.Api.BackgroundServices;
using Vennu.Api.Webhooks;
using Vennu.Api.PlatformOperations;
using Vennu.Api.Billing;
using Vennu.Data.Services;
using Vennu.Api.Services;
using Vennu.Api.BackOffice;
using Vennu.Api.Infrastructure;
using Vennu.Api.Pos;
using Vennu.Api.CustomerAuthentication;
using Vennu.Api.Configuration;
using Vennu.Api.Release;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Fido2NetLib;
using Azure.Identity;
using Vennu.Data.Configuration;

var builder = WebApplication.CreateBuilder(args);
IConfigurationSecretProtector? databaseSecretProtector = null;
var configurationProviderHealth = new SystemConfigurationProviderHealth();

var configurationEnvironment = Environment.GetEnvironmentVariable("VENU_CONFIGURATION_ENVIRONMENT");
if (!string.IsNullOrWhiteSpace(configurationEnvironment))
{
    configurationProviderHealth.Enable();
    var configurationConnectionString = Environment.GetEnvironmentVariable("VENU_CONFIGURATION_CONNECTION_STRING")
        ?? builder.Configuration.GetConnectionString("VennuDatabase")
        ?? throw new InvalidOperationException("VENU_CONFIGURATION_CONNECTION_STRING or ConnectionStrings:VennuDatabase is required when database configuration is enabled.");
    DatabaseMigrator.Run(configurationConnectionString);
    databaseSecretProtector = Environment.GetEnvironmentVariable("VENU_CONFIGURATION_KEY_PROVIDER") switch
    {
        "AzureKeyVault" => new AzureKeyVaultConfigurationSecretProtector(
            new Uri(Environment.GetEnvironmentVariable("VENU_CONFIGURATION_KEY_ID")
                ?? throw new InvalidOperationException("VENU_CONFIGURATION_KEY_ID is required for Azure Key Vault secret protection.")),
            new DefaultAzureCredential()),
        "Environment" => new EnvironmentKeyConfigurationSecretProtector(
            Environment.GetEnvironmentVariable("VENU_CONFIGURATION_LOCAL_KEY")
                ?? throw new InvalidOperationException("VENU_CONFIGURATION_LOCAL_KEY is required for environment-key secret protection.")),
        null or "" => null,
        var provider => throw new InvalidOperationException($"Unsupported VENU_CONFIGURATION_KEY_PROVIDER '{provider}'.")
    };
    builder.Configuration.AddVennuDatabaseConfiguration(new VennuDatabaseConfigurationOptions
    {
        ConnectionString = configurationConnectionString,
        EnvironmentName = configurationEnvironment,
        ApplicationScope = "API",
        SecretProtector = databaseSecretProtector,
        Health = configurationProviderHealth
    });
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddCommandLine(args);
}

var administrativeCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (builder.Environment.IsDevelopment() && (administrativeCorsOrigins is null || administrativeCorsOrigins.Length == 0))
{
    administrativeCorsOrigins = DevelopmentCorsOrigins.Values;
}

var administrativeCorsEnabled = administrativeCorsOrigins is { Length: > 0 };

builder.Services.AddControllers();
builder.Services.AddSingleton(configurationProviderHealth);
if (databaseSecretProtector is not null) builder.Services.AddSingleton<IConfigurationSecretProtector>(databaseSecretProtector);
builder.Services
    .AddOptions<CustomerAuthenticationOptions>()
    .Bind(builder.Configuration.GetSection(CustomerAuthenticationOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<CustomerAuthenticationOptions>, CustomerAuthenticationOptionsValidator>();
builder.Services.AddSingleton<IValidateOptions<BackOfficeAuthenticationOptions>, BackOfficeAuthenticationOptionsValidator>();
var customerAuthentication = builder.Configuration
    .GetSection(CustomerAuthenticationOptions.SectionName)
    .Get<CustomerAuthenticationOptions>() ?? new CustomerAuthenticationOptions();
builder.Services.AddSingleton(new CustomerSessionPolicy
{
    AbsoluteLifetime = customerAuthentication.AbsoluteSessionLifetime,
    IdleLifetime = customerAuthentication.IdleSessionLifetime,
    TouchInterval = customerAuthentication.SessionTouchInterval,
    EmailLinkLifetime = customerAuthentication.EmailLinkLifetime,
    RecentAuthenticationWindow = customerAuthentication.RecentAuthenticationWindow
});
if (administrativeCorsEnabled)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AdministrativePortals", policy => policy
            .WithOrigins(administrativeCorsOrigins!)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });
}
builder.Services
    .AddOptions<BackOfficeAuthenticationOptions>(BackOfficeAuthenticationDefaults.AuthenticationScheme)
    .Configure(options => builder.Configuration.GetSection(BackOfficeAuthenticationOptions.LegacySectionName).Bind(options))
    .Bind(builder.Configuration.GetSection(BackOfficeAuthenticationOptions.SectionName))
    .ValidateOnStart();
builder.Services
    .AddAuthentication(PlatformOperationsAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<PlatformOperationsAuthenticationOptions, PlatformOperationsAuthenticationHandler>(
        PlatformOperationsAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            builder.Configuration.GetSection(PlatformOperationsAuthenticationOptions.LegacySectionName).Bind(options);
            builder.Configuration.GetSection(PlatformOperationsAuthenticationOptions.SectionName).Bind(options);
        })
    .AddScheme<BackOfficeAuthenticationOptions, BackOfficeAuthenticationHandler>(
        BackOfficeAuthenticationDefaults.AuthenticationScheme,
        _ => { })
    .AddScheme<CustomerBackOfficeAuthenticationOptions, CustomerBackOfficeAuthenticationHandler>(
        BackOfficeAuthenticationDefaults.CustomerAuthenticationScheme,
        _ => { })
    .AddScheme<CustomerSessionAuthenticationOptions, CustomerSessionAuthenticationHandler>(
        CustomerAuthenticationDefaults.AuthenticationScheme,
        _ => { })
    .AddCookie(CustomerAuthenticationDefaults.ExternalCookieScheme, options =>
    {
        options.Cookie.Name = "__Host-Vennusign.CustomerExternal";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddOpenIdConnect(CustomerAuthenticationDefaults.GoogleScheme, options =>
        ConfigureCustomerOidc(options, "https://accounts.google.com", "/signin-customer-google", customerAuthentication.Google, useFormPost: false, getClaimsFromUserInfoEndpoint: true))
    .AddOpenIdConnect(CustomerAuthenticationDefaults.AppleScheme, options =>
        ConfigureCustomerOidc(options, "https://appleid.apple.com", "/signin-customer-apple", customerAuthentication.Apple, useFormPost: true, getClaimsFromUserInfoEndpoint: false))
    .AddOpenIdConnect(CustomerAuthenticationDefaults.EntraScheme, options =>
        ConfigureCustomerOidc(options, customerAuthentication.Entra.Authority, "/signin-customer-entra", new CustomerOidcProviderOptions
        {
            Enabled = customerAuthentication.Entra.Enabled,
            ClientId = customerAuthentication.Entra.ClientId,
            ClientSecret = customerAuthentication.Entra.ClientSecret
            // Entra External ID's UserInfo endpoint returns an incomplete claim set for local
            // accounts - it drops email_verified even though the ID token itself carries it
            // correctly, which made every local-account sign-in fail CustomerOidcEvents'
            // verified-identity check. The ID token alone already has everything needed, so
            // skip the UserInfo round-trip for this provider.
        }, useFormPost: false, getClaimsFromUserInfoEndpoint: false));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        PlatformOperationsAuthenticationDefaults.AuthorizationPolicy,
        policy => policy
            .AddAuthenticationSchemes(PlatformOperationsAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole("PlatformOperations"));
    foreach (var permission in new[] { "read", "edit", "secrets", "import", "admin" })
    {
        options.AddPolicy($"Configuration:{permission}", policy => policy
            .AddAuthenticationSchemes(PlatformOperationsAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireClaim("vennusign:configuration_permission", permission));
    }
    options.AddPolicy(
        BackOfficeAuthenticationDefaults.AuthorizationPolicy,
        policy => policy
            .AddAuthenticationSchemes(
                BackOfficeAuthenticationDefaults.CustomerAuthenticationScheme,
                BackOfficeAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole("BackOffice")
            .RequireClaim(BackOfficeAuthenticationDefaults.VenueIdClaim));
    options.AddPolicy(
        CustomerAuthenticationDefaults.AuthorizationPolicy,
        policy => policy
            .AddAuthenticationSchemes(CustomerAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser());
    options.AddPolicy(
        CustomerAuthenticationDefaults.MfaSatisfiedAuthorizationPolicy,
        policy => policy
            .AddAuthenticationSchemes(CustomerAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .AddRequirements(new MfaSatisfiedRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, MfaSatisfiedAuthorizationHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CustomerOidcEvents>();
builder.Services.AddSingleton<ICustomerSecretProtector, DataProtectionCustomerSecretProtector>();
builder.Services.AddScoped<ICustomerPasskeyService, CustomerPasskeyService>();
builder.Services.AddFido2(options =>
{
    options.ServerDomain = customerAuthentication.Passkeys.ServerDomain;
    options.ServerName = "Vennusign";
    options.Origins = customerAuthentication.Passkeys.Origins;
});
builder.Services.AddHttpClient<IEmailLoginDelivery, ConfiguredEmailLoginDelivery>();
builder.Services.AddSingleton<IPosCredentialProtector, DataProtectionPosCredentialProtector>();
builder.Services.AddSingleton<IPosOAuthStateService, ProtectedPosOAuthStateService>();
builder.Services.Configure<SquareOAuthOptions>(builder.Configuration.GetSection(SquareOAuthOptions.SectionName));
builder.Services.AddHttpClient<ISquareOAuthGateway, SquareOAuthGateway>();
builder.Services.AddScoped<ISquareOAuthConnectionService, SquareOAuthConnectionService>();
builder.Services.Configure<SquareCatalogOptions>(builder.Configuration.GetSection(SquareCatalogOptions.SectionName));
builder.Services.AddHttpClient<ISquareCatalogGateway, SquareCatalogGateway>();
builder.Services.AddScoped<IPosProvider, SquarePosProvider>();
builder.Services.Configure<SquareWebhookOptions>(builder.Configuration.GetSection(SquareWebhookOptions.SectionName));
builder.Services.AddSingleton<IPosWebhookVerifier, SquarePosWebhookVerifier>();
builder.Services.Configure<ToastCatalogOptions>(builder.Configuration.GetSection(ToastCatalogOptions.SectionName));
builder.Services.AddHttpClient<IToastCatalogGateway, ToastCatalogGateway>();
builder.Services.Configure<ToastInventoryOptions>(builder.Configuration.GetSection(ToastInventoryOptions.SectionName));
builder.Services.AddHttpClient<IToastInventoryGateway, ToastInventoryGateway>();
builder.Services.AddScoped<IPosProvider, ToastPosProvider>();
builder.Services.Configure<ToastWebhookOptions>(builder.Configuration.GetSection(ToastWebhookOptions.SectionName));
builder.Services.AddSingleton<IPosWebhookVerifier, ToastPosWebhookVerifier>();
builder.Services.AddScoped<IToastInventorySyncService, ToastInventorySyncService>();
builder.Services.Configure<ToastPollingOptions>(builder.Configuration.GetSection(ToastPollingOptions.SectionName));
builder.Services.AddSingleton<IToastPollingCoordinator, ToastPollingCoordinator>();
builder.Services.AddHostedService<ToastPollingService>();
builder.Services.Configure<CloverOAuthOptions>(builder.Configuration.GetSection(CloverOAuthOptions.SectionName));
builder.Services.AddHttpClient<ICloverOAuthGateway, CloverOAuthGateway>();
builder.Services.AddScoped<ICloverOAuthConnectionService, CloverOAuthConnectionService>();
builder.Services.Configure<CloverCatalogOptions>(builder.Configuration.GetSection(CloverCatalogOptions.SectionName));
builder.Services.AddHttpClient<ICloverCatalogGateway, CloverCatalogGateway>();
builder.Services.AddHttpClient<ICloverInventoryGateway, CloverInventoryGateway>();
builder.Services.AddScoped<IPosProvider, CloverPosProvider>();
builder.Services.Configure<CloverWebhookOptions>(builder.Configuration.GetSection(CloverWebhookOptions.SectionName));
builder.Services.AddSingleton<IPosWebhookVerifier, CloverPosWebhookVerifier>();
builder.Services.AddSingleton<IPosWebhookWorkSignal, PosWebhookWorkSignal>();
builder.Services.AddScoped<IPosWebhookEventHandler, SquareRealtimeSyncHandler>();
builder.Services.AddScoped<IPosWebhookEventHandler, ToastRealtimeSyncHandler>();
builder.Services.AddScoped<IPosWebhookEventHandler, CloverRealtimeSyncHandler>();
builder.Services.AddSingleton<IScreenUpdateNotifier, SignalRScreenUpdateNotifier>();
builder.Services.AddScoped<IMenuItemManagementService, MenuItemManagementService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddOptions<Vennu.Api.Menus.MenuBuilderOptions>()
    .Bind(builder.Configuration.GetSection(Vennu.Api.Menus.MenuBuilderOptions.SectionName))
    .Validate(options => options.ImportFileSizeLimitBytes > 0, "Menus import file-size limit must be positive.")
    .Validate(options => options.PublishRetrySilenceThreshold > TimeSpan.Zero, "Menus publish retry threshold must be positive.")
    .Validate(options => options.HistoryRetentionDepth > 0, "Menus history retention depth must be positive.")
    .ValidateOnStart();
builder.Services.AddScoped<Vennu.Api.Menus.MenuBuilderConfigurationResolver>();
builder.Services.AddScoped<Vennu.Api.Menus.MenuImportService>();
builder.Services.AddSingleton<Vennu.Api.Menus.MenuPasteParser>();
builder.Services.AddOptions<Vennu.Api.TestAutomation.TestAutomationOptions>()
    .Bind(builder.Configuration.GetSection(Vennu.Api.TestAutomation.TestAutomationOptions.SectionName));
builder.Services.AddSingleton<Vennu.Api.TestAutomation.TestAutomationAuthorization>();
builder.Services.AddScoped<IScreenManagementService, ScreenManagementService>();
builder.Services.AddScoped<IHaasPreRegistrationService, HaasPreRegistrationService>();
builder.Services.AddScoped<IScreenTargetingService, ScreenTargetingService>();
builder.Services.AddScoped<IVideoWallService, VideoWallService>();
builder.Services.Configure<HeartbeatMonitorOptions>(builder.Configuration.GetSection(HeartbeatMonitorOptions.SectionName));
builder.Services
    .AddOptions<StripeWebhookOptions>()
    .Bind(builder.Configuration.GetSection(StripeWebhookOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.SigningSecret) &&
            options.SigningSecret.StartsWith("whsec_", StringComparison.Ordinal),
        "Stripe webhook signing secret must be configured.")
    .Validate(
        options => options.ToleranceSeconds is > 0 and <= 3600,
        "Stripe webhook signature tolerance must be between 1 and 3600 seconds.")
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IStripeWebhookEventVerifier, StripeWebhookEventVerifier>();
builder.Services.Configure<StripeRevenueOptions>(builder.Configuration.GetSection(StripeRevenueOptions.SectionName));
builder.Services.Configure<StripeCheckoutOptions>(builder.Configuration.GetSection(StripeCheckoutOptions.SectionName));
builder.Services.Configure<StripeBillingPortalOptions>(builder.Configuration.GetSection(StripeBillingPortalOptions.SectionName));
builder.Services.Configure<StripeHaasCheckoutOptions>(builder.Configuration.GetSection(StripeHaasCheckoutOptions.SectionName));
builder.Services.AddScoped<IStripeRevenueSource, StripeRevenueSource>();
builder.Services.AddScoped<IStripeSubscriptionTierUpdater, StripeSubscriptionTierUpdater>();
builder.Services.AddScoped<IStripeCheckoutSessionGateway, StripeCheckoutSessionGateway>();
builder.Services.AddScoped<IStripeBillingPortalSessionGateway, StripeBillingPortalSessionGateway>();
builder.Services.AddScoped<IStripeHaasCheckoutSessionGateway, StripeHaasCheckoutSessionGateway>();
builder.Services.AddHostedService<HeartbeatMonitor>();
builder.Services.AddVennuData();
builder.Services.AddScoped<ICapabilityDecisionInputProvider, BackOfficeCapabilityDecisionInputProvider>();
builder.Services.AddScoped<ICapabilityDecisionService, CapabilityDecisionService>();
builder.Services.AddScoped<ICapabilityActionAuthorizer, CapabilityActionAuthorizer>();
builder.Services.AddScoped<IVenueThemeService, VenueThemeService>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<ScheduledContentActivationService>();
    builder.Services.AddHostedService<HappyHourEvaluatorService>();
    builder.Services.AddHostedService<PromotionActivationService>();
    builder.Services.AddHostedService<PosWebhookWorker>();
    var connectionString = builder.Configuration.GetConnectionString("VennuDatabase")
        ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");

    Vennu.Data.DatabaseMigrator.Run(connectionString);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (administrativeCorsEnabled)
{
    app.UseCors("AdministrativePortals");
}

app.UseHttpsRedirection();

app.UseMiddleware<AdministrativeCompatibilityMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<VennuHub>("/hubs/vennusign");
app.MapHub<VennuHub>("/hubs/vennu");
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "Vennusign.Api" }));
app.MapGet("/health/version", () => Results.Ok(ReleaseVersionMetadata.FromEnvironment()));

app.Run();

static void ConfigureCustomerOidc(
    OpenIdConnectOptions options,
    string authority,
    string callbackPath,
    CustomerOidcProviderOptions provider,
    bool useFormPost,
    bool getClaimsFromUserInfoEndpoint)
{
    options.Authority = authority;
    options.ClientId = string.IsNullOrWhiteSpace(provider.ClientId) ? "not-configured" : provider.ClientId;
    options.ClientSecret = string.IsNullOrWhiteSpace(provider.ClientSecret) ? "not-configured" : provider.ClientSecret;
    options.CallbackPath = callbackPath;
    options.SignInScheme = CustomerAuthenticationDefaults.ExternalCookieScheme;
    options.ResponseType = "code";
    options.ResponseMode = useFormPost ? "form_post" : "query";
    options.UsePkce = true;
    options.RequireHttpsMetadata = true;
    options.MapInboundClaims = false;
    options.SaveTokens = false;
    options.GetClaimsFromUserInfoEndpoint = getClaimsFromUserInfoEndpoint;
    options.RemoteAuthenticationTimeout = TimeSpan.FromMinutes(10);
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.ValidateIssuer = true;
    options.TokenValidationParameters.ValidateAudience = true;
    options.TokenValidationParameters.ValidateLifetime = true;
    options.EventsType = typeof(CustomerOidcEvents);
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
    options.NonceCookie.SameSite = SameSiteMode.None;
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
}

public partial class Program;
