namespace Vennu.Api.Contracts.VenueAdmin;

public sealed record VenueAdminPosConnectResponse(string AuthorizationUrl);
public sealed record VenueAdminPosConnectionResponse(string Provider, string Status, string ExternalMerchantId, DateTime? AccessTokenExpiresUtc, DateTime UpdatedUtc);
