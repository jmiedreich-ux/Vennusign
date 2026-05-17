using Microsoft.Extensions.DependencyInjection;
using DataManager.DataAccess;

namespace DataManager.DBContext
{ 
    public static class Container
    {
        public static IServiceCollection AddSQLDataAccess(this IServiceCollection services)
        {
            services.AddTransient<ISQLDataAccess, SQLDataAccess>();
            return services;
        }
    }
}

