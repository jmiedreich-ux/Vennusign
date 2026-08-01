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
        services.AddScoped<IMealPeriodRepository, MealPeriodRepository>();
        services.AddSingleton<IMealPeriodScheduleResolver, MealPeriodScheduleResolver>();
        services.AddScoped<IMealPeriodAdministrationService, MealPeriodAdministrationService>();
        services.AddScoped<IHappyHourScheduleRepository, HappyHourScheduleRepository>();
        services.AddSingleton<IHappyHourScheduleResolver, HappyHourScheduleResolver>();
        services.AddScoped<IHappyHourService, HappyHourService>();
        services.AddScoped<IPlaylistSlideRepository, PlaylistSlideRepository>();
        services.AddScoped<IPlaylistAdministrationService, PlaylistAdministrationService>();
        services.AddScoped<IEmergencyBroadcastRepository, EmergencyBroadcastRepository>();
        services.AddScoped<IEmergencyBroadcastService, EmergencyBroadcastService>();
        services.AddScoped<IDateRangePromotionRepository, DateRangePromotionRepository>();
        services.AddSingleton<IDateRangePromotionResolver, DateRangePromotionResolver>();
        services.AddScoped<IDateRangePromotionService, DateRangePromotionService>();
        services.AddScoped<ITapListRepository, TapListRepository>();
        services.AddScoped<ITapListAdministrationService, TapListAdministrationService>();
        services.AddScoped<IMenuSectionManagementService, MenuSectionManagementService>();
        services.AddScoped<IScreenRepository, ScreenRepository>();
        services.AddScoped<IScreenPairingCodeRepository, ScreenPairingCodeRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<SubscriptionTierRepository>();
        services.AddScoped<ISubscriptionTierRepository>(provider => provider.GetRequiredService<SubscriptionTierRepository>());
        services.AddScoped<IBillingCatalogRepository>(provider => provider.GetRequiredService<SubscriptionTierRepository>());
        services.AddScoped<IVenueSubscriptionRepository, VenueSubscriptionRepository>();
        services.AddScoped<IHaasContractRepository, HaasContractRepository>();
        services.AddScoped<IPosConnectionRepository, PosConnectionRepository>();
        services.AddScoped<IPosCatalogMappingRepository, PosCatalogMappingRepository>();
        services.AddScoped<IPosCatalogImportService, PosCatalogImportService>();
        services.AddScoped<IPosWebhookEventRepository, PosWebhookEventRepository>();
        services.AddScoped<IPosWebhookEventDispatcher, PosWebhookEventDispatcher>();
        services.AddScoped<IVenueFeatureOverrideRepository, VenueFeatureOverrideRepository>();
        services.AddScoped<IFeatureUsageRepository, FeatureUsageRepository>();
        services.AddScoped<IFeatureResolutionService, FeatureResolutionService>();
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();
        services.AddScoped<IVenueProvisioningService, VenueProvisioningService>();
        services.AddScoped<IUsageMeteringService, UsageMeteringService>();
        services.AddScoped<IBillingCatalogService, BillingCatalogService>();
        services.AddScoped<ICheckoutSessionService, CheckoutSessionService>();
        services.AddScoped<IBillingPortalSessionService, BillingPortalSessionService>();
        services.AddScoped<IHaasBillingService, HaasBillingService>();
        services.AddScoped<IPosConnectionService, PosConnectionService>();
        services.AddScoped<IProcessedStripeEventRepository, ProcessedStripeEventRepository>();
        services.AddScoped<IOperationalEventRepository, OperationalEventRepository>();
        services.AddScoped<IRevenueDailySnapshotRepository, RevenueDailySnapshotRepository>();
        services.AddScoped<IStripeEventIdempotencyService, StripeEventIdempotencyService>();
        services.AddScoped<IStripeSubscriptionEventHandler, StripeSubscriptionEventHandler>();
        services.AddScoped<IHaasContractSubscriptionEventHandler, HaasContractSubscriptionEventHandler>();
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
        services.AddScoped<ICustomerIdentityRepository, CustomerIdentityRepository>();
        services.AddScoped<IOrganizationMembershipRepository, OrganizationMembershipRepository>();
        services.AddSingleton<IMembershipCapabilityResolver, MembershipCapabilityResolver>();
        services.AddScoped<IIdentityMembershipService, IdentityMembershipService>();
        return services;
    }
}
