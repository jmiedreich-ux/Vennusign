using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Configuration;

public static class VennuDatabaseConfigurationExtensions
{
    public static IConfigurationBuilder AddVennuDatabaseConfiguration(
        this IConfigurationBuilder builder,
        VennuDatabaseConfigurationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);
        return builder.Add(new VennuDatabaseConfigurationSource(options));
    }
}
