namespace Vennu.Api.Contracts.Admin;

public sealed record SystemConfigurationSettingResponse(
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

public sealed record SystemConfigurationWriteRequest(string EnvironmentName, string? Value, string? ExpectedVersion);
