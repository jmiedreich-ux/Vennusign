namespace Vennu.Api.BackOffice;

public static class BackOfficeAuthenticationDefaults
{
    public const string AuthenticationScheme = "BackOfficeAccessToken";
    public const string CustomerAuthenticationScheme = "BackOfficeCustomerSession";
    public const string AuthorizationPolicy = "BackOffice";
    public const string HeaderName = "X-Vennusign-Back-Office-Token";
    public const string LegacyHeaderName = "X-Vennu-Venue-Token";
    public const string VenueIdClaim = "vennusign:venue_id";
    public const string OrganizationIdClaim = "vennusign:organization_id";
    public const string SystemRoleClaim = "vennusign:system_role";
    public const string VenueSelectionHeaderName = "X-Vennusign-Venue-Id";
    public const string LegacyVenueSelectionHeaderName = "X-Vennu-Venue-Id";
    public const string AuthenticationSourceClaim = "vennusign:venue_auth_source";
}
