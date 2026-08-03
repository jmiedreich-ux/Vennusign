using Microsoft.AspNetCore.Authentication;

namespace Vennu.Api.BackOffice;

public sealed class BackOfficeAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SectionName = "BackOffice";
    public const string LegacySectionName = "VenueAdmin";

    public List<BackOfficeSessionOptions> Sessions { get; set; } = [];
    public bool LegacySessionsEnabled { get; set; } = true;
    public DateTime? LegacySessionsRetireAfterUtc { get; set; }
}

public sealed class BackOfficeSessionOptions
{
    public string AccessToken { get; set; } = string.Empty;

    public Guid VenueId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public List<string> Capabilities { get; set; } = [];
    public bool Enabled { get; set; } = true;
    public DateTime? ExpiresUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
}
