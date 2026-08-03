namespace Vennu.Api.Pos;

public sealed class SquareOAuthOptions
{
    public const string SectionName = "Square:OAuth";

    public string ApplicationId { get; set; } = string.Empty;
    public string ApplicationSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = "https://connect.squareup.com/oauth2/authorize";
    public string TokenEndpoint { get; set; } = "https://connect.squareup.com/oauth2/token";
    public string RevokeEndpoint { get; set; } = "https://connect.squareup.com/oauth2/revoke";
    public string CallbackUrl { get; set; } = string.Empty;
    public string VenueAdminReturnUrl { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2026-07-15";
    public string[] Scopes { get; set; } = [];
}
