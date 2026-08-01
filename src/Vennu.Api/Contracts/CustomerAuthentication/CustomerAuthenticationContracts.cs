namespace Vennu.Api.Contracts.CustomerAuthentication;

public sealed record RequestEmailLoginRequest(string Email, string ReturnPath);
public sealed record RedeemEmailLoginRequest(string Token);
public sealed record CustomerSessionResponse(Guid UserId, string Email, string DisplayName, string AuthenticationMethod);
