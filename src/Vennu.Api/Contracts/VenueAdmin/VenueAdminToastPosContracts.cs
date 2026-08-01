namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record ConfigureToastConnectionRequest(string RestaurantGuid, string AccessToken);

public sealed record VenueAdminToastStatusResponse(
    VenueAdminPosConnectionResponse? Connection,
    string WebhookRegistrationStatus,
    bool RequiresToastApproval,
    string Guidance);
