using Microsoft.AspNetCore.Authentication;

namespace Vennu.Api.VenueAdmin;

public sealed class VenueAdminAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "VenueAdmin";

    public List<VenueAdminSessionOptions> Sessions { get; set; } = [];
}

public sealed class VenueAdminSessionOptions
{
    public string AccessToken { get; set; } = string.Empty;

    public Guid VenueId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public List<string> Capabilities { get; set; } = [];
}
