using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.VenueAdmin;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.VenueAdmin;

[ApiController]
[Route("api/venue-admin/screens/pairing")]
[Authorize(Policy = VenueAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class VenueAdminPairingController(
    IScreenRepository screenRepository,
    IScreenPairingCodeRepository pairingCodeRepository,
    IVenueRepository venueRepository,
    IVenueEntitlementService entitlementService) : ControllerBase
{
    [HttpPost("{code}/claim")]
    public async Task<ActionResult<ClaimScreenPairingCodeResponse>> Claim(
        string code,
        CancellationToken cancellationToken)
    {
        var venueId = Guid.Parse(
            User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
        try
        {
            await entitlementService.EnsureCanAddScreenAsync(venueId, cancellationToken).ConfigureAwait(false);
        }
        catch (TierScreenLimitReachedException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Screen limit reached.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        var controller = new ScreensController(
            screenRepository,
            pairingCodeRepository,
            venueRepository);
        return await controller.ClaimPairingCode(
            code,
            new ClaimScreenPairingCodeRequest { VenueId = venueId },
            cancellationToken).ConfigureAwait(false);
    }
}
