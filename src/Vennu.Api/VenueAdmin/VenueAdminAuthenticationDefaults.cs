namespace Vennu.Api.VenueAdmin;

public static class VenueAdminAuthenticationDefaults
{
    public const string AuthenticationScheme = "VenueAdminAccessToken";
    public const string CustomerAuthenticationScheme = "VenueAdminCustomerSession";
    public const string AuthorizationPolicy = "VenueAdmin";
    public const string HeaderName = "X-Vennu-Venue-Token";
    public const string VenueIdClaim = "vennu:venue_id";
    public const string CapabilitiesClaim = "vennu:capability";
    public const string VenueSelectionHeaderName = "X-Vennu-Venue-Id";
    public const string AuthenticationSourceClaim = "vennu:venue_auth_source";
}
