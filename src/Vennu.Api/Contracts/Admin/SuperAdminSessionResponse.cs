namespace Vennu.Api.Contracts.Admin;

public sealed record SuperAdminSessionResponse(string DisplayName, IReadOnlyCollection<string> Capabilities);

