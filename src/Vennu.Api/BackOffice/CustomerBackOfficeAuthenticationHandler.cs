using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Vennu.Api.CustomerAuthentication;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.BackOffice;

public sealed class CustomerBackOfficeAuthenticationOptions : AuthenticationSchemeOptions;

public sealed class CustomerBackOfficeAuthenticationHandler : AuthenticationHandler<CustomerBackOfficeAuthenticationOptions>
{
    private static readonly string[] CoreContentCapabilities = ["menus", "screens", "themes", "tap_list"];
    private readonly ICustomerSessionService sessions;
    private readonly ICustomerOnboardingRepository onboarding;
    private readonly IVenueRepository venues;
    private readonly IOrganizationMembershipRepository memberships;
    private readonly IMembershipCapabilityResolver membershipCapabilities;
    private readonly IFeatureResolutionService features;

    public CustomerBackOfficeAuthenticationHandler(
        IOptionsMonitor<CustomerBackOfficeAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        ICustomerSessionService sessions,
        ICustomerOnboardingRepository onboarding,
        IVenueRepository venues,
        IOrganizationMembershipRepository memberships,
        IMembershipCapabilityResolver membershipCapabilities,
        IFeatureResolutionService features) : base(options, logger, encoder)
    {
        this.sessions = sessions;
        this.onboarding = onboarding;
        this.venues = venues;
        this.memberships = memberships;
        this.membershipCapabilities = membershipCapabilities;
        this.features = features;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!CustomerSessionCookie.TryRead(Request, out var token))
            return AuthenticateResult.NoResult();
        var customer = await sessions.AuthenticateAsync(token, Context.RequestAborted).ConfigureAwait(false);
        if (customer is null) return AuthenticateResult.Fail("The customer session is invalid or expired.");

        var state = await onboarding.GetByUserIdAsync(customer.User.Id, Context.RequestAborted).ConfigureAwait(false);
        var venueId = ResolveVenueId(state);
        if (venueId is null) return AuthenticateResult.Fail("No authorized venue was selected.");
        var venue = await venues.GetByIdAsync(venueId.Value, Context.RequestAborted).ConfigureAwait(false);
        if (venue?.OrganizationId is not Guid organizationId) return AuthenticateResult.Fail("The selected venue has no customer organization.");

        var organizationMembership = await memberships.GetOrganizationMembershipAsync(organizationId, customer.User.Id, Context.RequestAborted).ConfigureAwait(false);
        var venueMembership = await memberships.GetVenueMembershipAsync(organizationId, venue.Id, customer.User.Id, Context.RequestAborted).ConfigureAwait(false);
        var organizationRole = organizationMembership?.RevokedUtc is null ? organizationMembership?.Role : null;
        var venueRole = venueMembership?.RevokedUtc is null ? venueMembership?.Role : null;
        if (!membershipCapabilities.HasCapability(organizationRole, venueRole, MembershipCapability.ManageVenueContent))
            return AuthenticateResult.Fail("The customer is not authorized to manage this venue.");

        var effectiveFeatures = await features.GetFeatureSetAsync(venue.Id, Context.RequestAborted).ConfigureAwait(false);
        var capabilityValues = CoreContentCapabilities
            .Concat(effectiveFeatures.Values.Where(feature => feature.Enabled).Select(feature => feature.Key))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.User.Id.ToString()),
            new(ClaimTypes.Name, customer.User.DisplayName),
            new(ClaimTypes.Email, customer.User.Email),
            new(ClaimTypes.Role, "BackOffice"),
            new(BackOfficeAuthenticationDefaults.VenueIdClaim, venue.Id.ToString()),
            new(BackOfficeAuthenticationDefaults.AuthenticationSourceClaim, "customer-session")
        };
        claims.AddRange(capabilityValues.Select(value => new Claim(BackOfficeAuthenticationDefaults.CapabilitiesClaim, value)));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    private Guid? ResolveVenueId(Vennu.Core.Models.CustomerOnboardingState? state)
    {
        var canonicalPresent = Request.Headers.TryGetValue(BackOfficeAuthenticationDefaults.VenueSelectionHeaderName, out var canonicalValues);
        var legacyPresent = Request.Headers.TryGetValue(BackOfficeAuthenticationDefaults.LegacyVenueSelectionHeaderName, out var legacyValues);
        if (canonicalPresent && legacyPresent && !string.Equals(canonicalValues.ToString(), legacyValues.ToString(), StringComparison.OrdinalIgnoreCase))
            return null;
        var values = canonicalPresent ? canonicalValues : legacyValues;
        if ((canonicalPresent || legacyPresent) && Guid.TryParse(values.ToString(), out var selected) && selected != Guid.Empty)
            return selected;
        return state?.VenueId;
    }
}
