using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.BackOffice;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/session")]
[Route("api/venue-admin/session")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
public sealed class BackOfficeSessionController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<BackOfficeSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<BackOfficeSessionResponse> Get()
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
        return Ok(new BackOfficeSessionResponse(
            venueId,
            User.Identity?.Name ?? "Back Office",
            capabilities));
    }
}
