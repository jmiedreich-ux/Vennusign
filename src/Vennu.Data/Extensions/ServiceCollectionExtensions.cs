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
        services.AddScoped<IScreenRepository, ScreenRepository>();
        services.AddScoped<IScreenPairingCodeRepository, ScreenPairingCodeRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<ISubscriptionTierRepository, SubscriptionTierRepository>();
        services.AddScoped<IVenueSubscriptionRepository, VenueSubscriptionRepository>();
        services.AddScoped<IVenueFeatureOverrideRepository, VenueFeatureOverrideRepository>();
        services.AddScoped<IFeatureUsageRepository, FeatureUsageRepository>();
        services.AddScoped<IFeatureResolutionService, FeatureResolutionService>();
        services.AddScoped<ISubscriptionManagementService, SubscriptionManagementService>();
        services.AddScoped<IUsageMeteringService, UsageMeteringService>();
        return services;
    }
}
