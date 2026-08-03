using Vennu.Data.Configuration;

namespace Vennu.Api.Contracts.Admin;

public sealed record SystemConfigurationImportPreviewRequest(string TargetEnvironment, SystemConfigurationManifest Manifest);
public sealed record SystemConfigurationImportApplyRequest(Guid OperationId, string TargetEnvironment, IReadOnlyList<SystemConfigurationImportItem> Settings);
