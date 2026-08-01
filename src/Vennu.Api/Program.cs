using Vennu.Data;
using Vennu.Data.Extensions;
using Vennu.Api.Hubs;
using Vennu.Api.Notifications;
using Vennu.Api.BackgroundServices;
using Vennu.Api.Webhooks;
using Vennu.Api.Admin;
using Vennu.Api.Billing;
using Vennu.Data.Services;
using Vennu.Api.Services;
using Vennu.Api.VenueAdmin;
using Vennu.Api.Infrastructure;
using Vennu.Api.Pos;

var builder = WebApplication.CreateBuilder(args);

var adminCorsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (builder.Environment.IsDevelopment() && (adminCorsOrigins is null || adminCorsOrigins.Length == 0))
{
    adminCorsOrigins =
    [
        "http://localhost:5173",
        "https://localhost:5173",
        "http://localhost:5174",
        "https://localhost:5174"
    ];
}

var adminCorsEnabled = adminCorsOrigins is { Length: > 0 };

builder.Services.AddControllers();
if (adminCorsEnabled)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AdminPortal", policy => policy
            .WithOrigins(adminCorsOrigins!)
            .AllowAnyHeader()
            .AllowAnyMethod());
    });
}
builder.Services
    .AddAuthentication(SuperAdminAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<SuperAdminAuthenticationOptions, SuperAdminAuthenticationHandler>(
        SuperAdminAuthenticationDefaults.AuthenticationScheme,
        options => builder.Configuration.GetSection(SuperAdminAuthenticationOptions.SectionName).Bind(options))
    .AddScheme<VenueAdminAuthenticationOptions, VenueAdminAuthenticationHandler>(
        VenueAdminAuthenticationDefaults.AuthenticationScheme,
        options => builder.Configuration.GetSection(VenueAdminAuthenticationOptions.SectionName).Bind(options));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        SuperAdminAuthenticationDefaults.AuthorizationPolicy,
        policy => policy
            .AddAuthenticationSchemes(SuperAdminAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole("SuperAdmin"));
    options.AddPolicy(
        VenueAdminAuthenticationDefaults.AuthorizationPolicy,
        policy => policy
            .AddAuthenticationSchemes(VenueAdminAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole("VenueAdmin")
            .RequireClaim(VenueAdminAuthenticationDefaults.VenueIdClaim));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddDataProtection();
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
builder.Services.AddScoped<IQuickUpdateService, QuickUpdateService>();
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
builder.Services.AddScoped<IVenueThemeService, VenueThemeService>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<QuickAvailabilityResetService>();
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

if (adminCorsEnabled)
{
    app.UseCors("AdminPortal");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<VennuHub>("/hubs/vennu");
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "Vennu.Api" }));

app.Run();

public partial class Program;
