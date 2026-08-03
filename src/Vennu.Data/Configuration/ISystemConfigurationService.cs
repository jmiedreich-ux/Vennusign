namespace Vennu.Data.Configuration;

public interface ISystemConfigurationService
{
    Task<IReadOnlyList<SystemConfigurationSetting>> GetAsync(string environmentName, string? applicationScope, CancellationToken cancellationToken = default);
    Task<SystemConfigurationSetting> SetAsync(SystemConfigurationWrite write, CancellationToken cancellationToken = default);
    Task<SystemConfigurationSetting> ClearAsync(SystemConfigurationWrite write, CancellationToken cancellationToken = default);
}
