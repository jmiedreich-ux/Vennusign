namespace Vennu.Data.Configuration;

public sealed class VennuDatabaseConfigurationOptions
{
    public required string ConnectionString { get; init; }
    public required string EnvironmentName { get; init; }
    public required string ApplicationScope { get; init; }
    public IConfigurationSecretProtector? SecretProtector { get; init; }
    public SystemConfigurationProviderHealth? Health { get; init; }
    public TimeSpan ReloadInterval { get; init; } = TimeSpan.FromMinutes(1);
}
