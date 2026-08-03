namespace Vennu.Api.Contracts.PlatformOperations;

public sealed record PlatformOperationsSessionResponse(string DisplayName, IReadOnlyCollection<string> Capabilities);
