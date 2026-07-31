namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record VenueAdminSessionResponse(
    Guid VenueId,
    string DisplayName,
    IReadOnlyCollection<string> Capabilities);
