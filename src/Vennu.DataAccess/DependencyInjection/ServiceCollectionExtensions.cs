using Microsoft.Extensions.DependencyInjection;

namespace Vennu.DataAccess.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlDataAccess(this IServiceCollection services)
    {
        services.AddTransient<ISqlDataAccess, SqlDataAccess>();
        services.AddTransient<SqlDataAccess>();
        return services;
    }
}
