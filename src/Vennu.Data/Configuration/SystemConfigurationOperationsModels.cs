namespace Vennu.Data.Configuration;

public sealed record SystemConfigurationRevision(
    int RevisionNumber,
    string ValueFingerprint,
    bool IsSecret,
    bool IsClear,
    string ChangedBy,
    string ChangeSource,
    DateTime CreatedUtc);

public sealed record SystemConfigurationRollback(
    Guid DefinitionId,
    string EnvironmentName,
    int RevisionNumber,
    string ExpectedVersion,
    string Actor);
