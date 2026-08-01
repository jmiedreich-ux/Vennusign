namespace Vennu.Api.Pos;

public sealed class CloverOAuthOptions
{
    public const string SectionName = "Clover:OAuth";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string AuthorizationEndpoint { get; set; } = "https://www.clover.com/oauth/v2/authorize";
    public string TokenEndpoint { get; set; } = "https://api.clover.com/oauth/v2/token";
    public string CallbackUrl { get; set; } = string.Empty;
    public string VenueAdminReturnUrl { get; set; } = string.Empty;
}
