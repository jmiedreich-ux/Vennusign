namespace Vennu.Api.Contracts.BackOffice;

public sealed record BackOfficePosConnectResponse(string AuthorizationUrl);
public sealed record BackOfficePosConnectionResponse(string Provider, string Status, string ExternalMerchantId, DateTime? AccessTokenExpiresUtc, DateTime UpdatedUtc);
