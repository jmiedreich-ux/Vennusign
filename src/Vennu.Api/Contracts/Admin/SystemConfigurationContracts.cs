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
    string? Version,
    DateTime? LastUpdatedUtc,
    int? RotationReminderDays);

public sealed record SystemConfigurationWriteRequest(string EnvironmentName, string? Value, string? ExpectedVersion);
public sealed record SystemConfigurationRollbackRequest(string EnvironmentName, int RevisionNumber, string ExpectedVersion);
