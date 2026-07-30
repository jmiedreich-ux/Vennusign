using Microsoft.Extensions.DependencyInjection;
using Vennu.DataAccess.DependencyInjection;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVennuData(this IServiceCollection services)
    {
        services.AddSqlDataAccess();
        services.AddMemoryCache();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IVenueThemeRepository, VenueThemeRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IMenuSectionManagementService, MenuSectionManagementService>();
        services.AddScoped<IScreenRepository, ScreenRepository>();
        services.AddScoped<IScreenPairingCodeRepository, ScreenPairingCodeRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<SubscriptionTierRepository>();
        services.AddScoped<ISubscriptionTierRepository>(provider => provider.GetRequiredService<SubscriptionTierRepository>());
        services.AddScoped<IBillingCatalogRepository>(provider => provider.GetRequiredService<SubscriptionTierRepository>());
        services.AddScoped<IVenueSubscriptionRepository, VenueSubscriptionRepository>();
        services.AddScoped<IVenueFeatureOverrideRepository, VenueFeatureOverrideRepository>();
        services.AddScoped<IFeatureUsageRepository, FeatureUsageRepository>();
        services.AddScoped<IFeatureResolutionService, FeatureResolutionService>();
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();
        services.AddScoped<IUsageMeteringService, UsageMeteringService>();
        services.AddScoped<IBillingCatalogService, BillingCatalogService>();
        services.AddScoped<IProcessedStripeEventRepository, ProcessedStripeEventRepository>();
        services.AddScoped<IOperationalEventRepository, OperationalEventRepository>();
        services.AddScoped<IRevenueDailySnapshotRepository, RevenueDailySnapshotRepository>();
        services.AddScoped<IStripeEventIdempotencyService, StripeEventIdempotencyService>();
        services.AddScoped<IStripeSubscriptionEventHandler, StripeSubscriptionEventHandler>();
        services.AddScoped<IVenueDirectoryService, VenueDirectoryService>();
        services.AddScoped<IVenueSupportDetailService, VenueSupportDetailService>();
        services.AddScoped<ITierManagementService, TierManagementService>();
        services.AddScoped<IFeatureMatrixRepository, FeatureMatrixRepository>();
        services.AddScoped<IFeatureMatrixService, FeatureMatrixService>();
        services.AddScoped<IVenueFeatureOverrideManagementService, VenueFeatureOverrideManagementService>();
        services.AddScoped<IVenueTierSwitchService, VenueTierSwitchService>();
        services.AddScoped<IOperationalDashboardService, OperationalDashboardService>();
        services.AddScoped<IOperationalEventFeedService, OperationalEventFeedService>();
        services.AddScoped<IRevenueSnapshotService, RevenueSnapshotService>();
        services.AddScoped<IRevenueTrendService, RevenueTrendService>();
        return services;
    }
}
