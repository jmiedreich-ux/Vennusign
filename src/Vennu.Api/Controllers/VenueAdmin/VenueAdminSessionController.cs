using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.VenueAdmin;
using Vennu.Api.VenueAdmin;

namespace Vennu.Api.Controllers.VenueAdmin;

[ApiController]
[Route("api/venue-admin/session")]
[Authorize(Policy = VenueAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class VenueAdminSessionController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<VenueAdminSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<VenueAdminSessionResponse> Get()
    {
        var venueIdValue = User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim);
        if (!Guid.TryParse(venueIdValue, out var venueId))
        {
            return Unauthorized();
        }

        var capabilities = User.FindAll(VenueAdminAuthenticationDefaults.CapabilitiesClaim)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(capability => capability, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return Ok(new VenueAdminSessionResponse(
            venueId,
            User.Identity?.Name ?? "Venue Admin",
            capabilities));
    }
}
