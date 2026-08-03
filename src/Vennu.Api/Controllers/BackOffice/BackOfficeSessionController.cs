using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.BackOffice;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/session")]
[Route("api/venue-admin/session")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
public sealed class BackOfficeSessionController(
    IBackOfficeContextRepository contexts,
    IVenueRepository venues) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<BackOfficeSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BackOfficeSessionResponse>> Get(CancellationToken cancellationToken)
    {
        var venueIdValue = User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim);
        if (!Guid.TryParse(venueIdValue, out var venueId))
        {
            return Unauthorized();
        }

        var capabilities = User.FindAll(BackOfficeAuthenticationDefaults.CapabilitiesClaim)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(capability => capability, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var displayName = User.Identity?.Name ?? "Back Office";
        var source = User.FindFirstValue(BackOfficeAuthenticationDefaults.AuthenticationSourceClaim);
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        Guid? userId = Guid.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : null;

        IReadOnlyCollection<BackOfficeContextResponse> authorized;
        if (string.Equals(source, "customer-session", StringComparison.Ordinal) && userId is Guid customerUserId)
        {
            authorized = (await contexts.GetAuthorizedAsync(customerUserId, cancellationToken).ConfigureAwait(false))
                .Select(context => new BackOfficeContextResponse(
                    context.OrganizationId,
                    context.OrganizationName,
                    context.VenueId,
                    context.VenueName))
                .ToArray();
            if (!authorized.Any(context => context.VenueId == venueId)) return Unauthorized();
        }
        else
        {
            var venue = await venues.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
            authorized = [new BackOfficeContextResponse(
                venue?.OrganizationId ?? Guid.Empty,
                "Legacy venue access",
                venueId,
                venue?.Name ?? "Current venue")];
        }

        var active = authorized.Single(context => context.VenueId == venueId);
        return Ok(new BackOfficeSessionResponse(
            venueId,
            displayName,
            capabilities,
            active.OrganizationId == Guid.Empty ? null : active.OrganizationId,
            active.OrganizationName,
            active.VenueName,
            new BackOfficeAccountResponse(userId, displayName, email),
            authorized));
    }
}
