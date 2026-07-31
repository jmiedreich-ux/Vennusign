namespace Vennu.Api.VenueAdmin;

public static class VenueAdminAuthenticationDefaults
{
    public const string AuthenticationScheme = "VenueAdminAccessToken";
    public const string AuthorizationPolicy = "VenueAdmin";
    public const string HeaderName = "X-Vennu-Venue-Token";
    public const string VenueIdClaim = "vennu:venue_id";
    public const string CapabilitiesClaim = "vennu:capability";
}
