namespace Vennu.Data.Configuration;

public sealed record SystemConfigurationSetting(
    Guid DefinitionId,
    string Key,
    string ApplicationScope,
    string Description,
    string ValueType,
    bool IsRequired,
    bool IsSecret,
    string? Value,
    bool HasConfiguredValue,
    bool RequiresRestart,
    string ExportPolicy,
    string? Version);

public sealed record SystemConfigurationWrite(
    Guid DefinitionId,
    string EnvironmentName,
    string? Value,
    string? ExpectedVersion,
    string Actor,
    string Source);
