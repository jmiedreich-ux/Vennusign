using Microsoft.Extensions.DependencyInjection;
using Vennu.DataAccess.DependencyInjection;
using Vennu.Data.Repositories;

namespace Vennu.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVennuData(this IServiceCollection services)
    {
        services.AddSqlDataAccess();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IScreenRepository, ScreenRepository>();
        services.AddScoped<IScreenPairingCodeRepository, ScreenPairingCodeRepository>();
        return services;
    }
}
