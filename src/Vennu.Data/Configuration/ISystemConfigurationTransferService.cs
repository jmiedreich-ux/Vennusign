namespace Vennu.Data.Configuration;

public interface ISystemConfigurationTransferService
{
    Task<SystemConfigurationManifest> ExportAsync(string environmentName, CancellationToken cancellationToken = default);
    Task<SystemConfigurationImportPreview> PreviewAsync(string targetEnvironment, SystemConfigurationManifest manifest, CancellationToken cancellationToken = default);
    Task ApplyAsync(SystemConfigurationImportApply import, CancellationToken cancellationToken = default);
}
