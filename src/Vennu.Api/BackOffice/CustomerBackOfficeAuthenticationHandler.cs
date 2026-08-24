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
    private readonly ICustomerSessionService sessions;
    private readonly ICustomerOnboardingRepository onboarding;
    private readonly IVenueRepository venues;
    private readonly IOrganizationMembershipRepository memberships;
    private readonly IBackOfficeContextRepository contexts;
    private readonly IMembershipCapabilityResolver membershipCapabilities;

    public CustomerBackOfficeAuthenticationHandler(
        IOptionsMonitor<CustomerBackOfficeAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        ICustomerSessionService sessions,
        ICustomerOnboardingRepository onboarding,
        IVenueRepository venues,
        IOrganizationMembershipRepository memberships,
        IBackOfficeContextRepository contexts,
        IMembershipCapabilityResolver membershipCapabilities) : base(options, logger, encoder)
    {
        this.sessions = sessions;
        this.onboarding = onboarding;
        this.venues = venues;
        this.memberships = memberships;
        this.contexts = contexts;
        this.membershipCapabilities = membershipCapabilities;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!CustomerSessionCookie.TryRead(Request, out var token))
            return AuthenticateResult.NoResult();
        var customer = await sessions.AuthenticateAsync(token, Context.RequestAborted).ConfigureAwait(false);
        if (customer is null) return AuthenticateResult.Fail("The customer session is invalid or expired.");

        // Every authenticated request pays this chain, and the builder client sends
        // an explicit venue header on essentially every call once a venue is
        // selected (api.ts's venueFetch). ResolveVenueIdFromHeaders needs no DB
        // access at all, so skip onboarding.GetByUserIdAsync's round trip entirely
        // whenever it can already answer - it only falls back to the account's
        // onboarded venue when there is genuinely no (or a conflicting) header to
        // read, exactly like ResolveVenueId used to before headers and state were
        // split apart here.
        var (headerVenueId, headerConflict) = ResolveVenueIdFromHeaders();
        Guid? venueId = headerConflict
            ? null
            : headerVenueId ??
              (await onboarding.GetByUserIdAsync(customer.User.Id, Context.RequestAborted).ConfigureAwait(false))?.VenueId;
        var authorized = venueId is Guid requestedVenueId
            ? await ResolveAuthorizedVenueAsync(requestedVenueId, customer.User.Id).ConfigureAwait(false)
            : null;
        if (authorized is null && !HasExplicitVenueSelection())
        {
            var fallback = (await contexts.GetAuthorizedAsync(customer.User.Id, Context.RequestAborted).ConfigureAwait(false))
                .FirstOrDefault();
            if (fallback is not null)
                authorized = await ResolveAuthorizedVenueAsync(fallback.VenueId, customer.User.Id).ConfigureAwait(false);
        }
        if (authorized is null) return AuthenticateResult.Fail("The customer is not authorized to manage the selected venue.");
        var venue = authorized.Value.Venue;

        var systemRole = ResolveSystemRole(authorized.Value.OrganizationRole, authorized.Value.VenueRole);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customer.User.Id.ToString()),
            new(ClaimTypes.Name, customer.User.DisplayName),
            new(ClaimTypes.Email, customer.User.Email),
            new(ClaimTypes.Role, "BackOffice"),
            new(BackOfficeAuthenticationDefaults.VenueIdClaim, venue.Id.ToString()),
            new(BackOfficeAuthenticationDefaults.OrganizationIdClaim, venue.OrganizationId!.Value.ToString()),
            new(BackOfficeAuthenticationDefaults.SystemRoleClaim, systemRole),
            new(BackOfficeAuthenticationDefaults.AuthenticationSourceClaim, "customer-session")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    private static string ResolveSystemRole(
        OrganizationMembershipRole? organizationRole,
        VenueMembershipRole? venueRole) => organizationRole switch
    {
        OrganizationMembershipRole.Owner => "organization_owner",
        OrganizationMembershipRole.Admin => "organization_administrator",
        _ => venueRole switch
        {
            VenueMembershipRole.Manager => "venue_administrator",
            VenueMembershipRole.Editor => "content_editor",
            VenueMembershipRole.Viewer => "viewer",
            _ => throw new InvalidOperationException("An authorized venue context requires a protected system role.")
        }
    };

    /// <summary>
    /// The header-only half of what used to be ResolveVenueId(state) - split out so
    /// the caller can tell "no usable header" (needs the onboarding fallback) apart
    /// from "conflicting headers" (never falls back, same as before) without
    /// awaiting onboarding.GetByUserIdAsync just to find out which one applies.
    /// </summary>
    private (Guid? VenueId, bool Conflict) ResolveVenueIdFromHeaders()
    {
        var canonicalPresent = Request.Headers.TryGetValue(BackOfficeAuthenticationDefaults.VenueSelectionHeaderName, out var canonicalValues);
        var legacyPresent = Request.Headers.TryGetValue(BackOfficeAuthenticationDefaults.LegacyVenueSelectionHeaderName, out var legacyValues);
        if (canonicalPresent && legacyPresent && !string.Equals(canonicalValues.ToString(), legacyValues.ToString(), StringComparison.OrdinalIgnoreCase))
            return (null, true);
        var values = canonicalPresent ? canonicalValues : legacyValues;
        if ((canonicalPresent || legacyPresent) && Guid.TryParse(values.ToString(), out var selected) && selected != Guid.Empty)
            return (selected, false);
        return (null, false);
    }

    private bool HasExplicitVenueSelection() =>
        Request.Headers.ContainsKey(BackOfficeAuthenticationDefaults.VenueSelectionHeaderName) ||
        Request.Headers.ContainsKey(BackOfficeAuthenticationDefaults.LegacyVenueSelectionHeaderName);

    private async Task<(Venue Venue, OrganizationMembershipRole? OrganizationRole, VenueMembershipRole? VenueRole)?> ResolveAuthorizedVenueAsync(
        Guid venueId,
        Guid userId)
    {
        var venue = await venues.GetByIdAsync(venueId, Context.RequestAborted).ConfigureAwait(false);
        if (venue?.OrganizationId is not Guid organizationId) return null;
        // Independent of each other - both only need organizationId/venue.Id/userId,
        // already in hand - so there is no reason to pay two round trips in series.
        var organizationMembershipTask = memberships.GetOrganizationMembershipAsync(
            organizationId, userId, Context.RequestAborted);
        var venueMembershipTask = memberships.GetVenueMembershipAsync(
            organizationId, venue.Id, userId, Context.RequestAborted);
        await Task.WhenAll(organizationMembershipTask, venueMembershipTask).ConfigureAwait(false);
        var organizationMembership = organizationMembershipTask.Result;
        var venueMembership = venueMembershipTask.Result;
        var organizationRole = organizationMembership?.RevokedUtc is null ? organizationMembership?.Role : null;
        var venueRole = venueMembership?.RevokedUtc is null ? venueMembership?.Role : null;
        return membershipCapabilities.HasCapability(organizationRole, venueRole, MembershipCapability.ManageVenueContent)
            ? (venue, organizationRole, venueRole)
            : null;
    }
}
