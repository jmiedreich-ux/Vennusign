using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vennu.Data.Repositories;
using Vennu.DataAccess.Configuration;
using Vennu.DataAccess.Infrastructure;
using Vennu.DataAccess.Repositories;

namespace Vennu.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVennuDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("VennuDatabase")
            ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");

        RepoDbBootstrapper.Initialize();
        services.AddSingleton(new VennuDataAccessOptions(connectionString));
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IScreenRepository, ScreenRepository>();
        services.AddScoped<IScreenPairingCodeRepository, ScreenPairingCodeRepository>();

        return services;
    }
}
