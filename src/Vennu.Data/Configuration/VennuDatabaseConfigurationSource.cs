using Microsoft.Extensions.Configuration;

namespace Vennu.Data.Configuration;

public sealed class VennuDatabaseConfigurationSource(VennuDatabaseConfigurationOptions options) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder) => new VennuDatabaseConfigurationProvider(options);
}
