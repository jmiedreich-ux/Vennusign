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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddAuthentication(SuperAdminAuthenticationDefaults.AuthenticationScheme)
    .AddScheme<SuperAdminAuthenticationOptions, SuperAdminAuthenticationHandler>(
        SuperAdminAuthenticationDefaults.AuthenticationScheme,
        options => builder.Configuration.GetSection(SuperAdminAuthenticationOptions.SectionName).Bind(options));
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        SuperAdminAuthenticationDefaults.AuthorizationPolicy,
        policy => policy
            .AddAuthenticationSchemes(SuperAdminAuthenticationDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .RequireRole("SuperAdmin"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IScreenUpdateNotifier, SignalRScreenUpdateNotifier>();
builder.Services.AddScoped<IMenuItemManagementService, MenuItemManagementService>();
builder.Services.AddScoped<IQuickUpdateService, QuickUpdateService>();
builder.Services.AddScoped<IScreenManagementService, ScreenManagementService>();
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
builder.Services.AddScoped<IStripeRevenueSource, StripeRevenueSource>();
builder.Services.AddScoped<IStripeSubscriptionTierUpdater, StripeSubscriptionTierUpdater>();
builder.Services.AddHostedService<HeartbeatMonitor>();
builder.Services.AddVennuData();
builder.Services.AddScoped<IVenueThemeService, VenueThemeService>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<QuickAvailabilityResetService>();
    builder.Services.AddHostedService<ScheduledContentActivationService>();
    builder.Services.AddHostedService<HappyHourEvaluatorService>();
    builder.Services.AddHostedService<PromotionActivationService>();
    var connectionString = builder.Configuration.GetConnectionString("VennuDatabase")
        ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");

    Vennu.Data.DatabaseMigrator.Run(connectionString);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<VennuHub>("/hubs/vennu");
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "Vennu.Api" }));

app.Run();

public partial class Program;
