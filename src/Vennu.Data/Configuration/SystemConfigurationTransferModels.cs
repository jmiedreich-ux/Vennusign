namespace Vennu.Data.Configuration;

public sealed record SystemConfigurationManifest(
    int SchemaVersion,
    string SourceEnvironment,
    DateTime ExportedUtc,
    IReadOnlyList<SystemConfigurationManifestItem> Settings);

public sealed record SystemConfigurationManifestItem(
    string Key,
    string ApplicationScope,
    string ValueType,
    bool RequiresRestart,
    string? Value);

public sealed record SystemConfigurationImportPreview(
    Guid OperationId,
    string TargetEnvironment,
    IReadOnlyList<SystemConfigurationImportItem> Settings);

public sealed record SystemConfigurationImportItem(
    string Key,
    string ApplicationScope,
    string Status,
    string? Value,
    string? ExpectedVersion,
    string? Message);

public sealed record SystemConfigurationImportApply(
    Guid OperationId,
    string TargetEnvironment,
    string Actor,
    IReadOnlyList<SystemConfigurationImportItem> Settings);
