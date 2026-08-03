namespace Vennu.Api.Contracts.BackOffice;

public sealed record BackOfficeSessionResponse(
    Guid VenueId,
    string DisplayName,
    IReadOnlyCollection<string> Capabilities);
