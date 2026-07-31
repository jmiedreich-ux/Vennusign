using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.VenueAdmin;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers.VenueAdmin;

[ApiController]
[Route("api/venue-admin/screens/pairing")]
[Authorize(Policy = VenueAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class VenueAdminPairingController(
    IScreenRepository screenRepository,
    IScreenPairingCodeRepository pairingCodeRepository,
    IVenueRepository venueRepository) : ControllerBase
{
    [HttpPost("{code}/claim")]
    public Task<ActionResult<ClaimScreenPairingCodeResponse>> Claim(
        string code,
        CancellationToken cancellationToken)
    {
        var venueId = Guid.Parse(
            User.FindFirstValue(VenueAdminAuthenticationDefaults.VenueIdClaim)!);
        var controller = new ScreensController(
            screenRepository,
            pairingCodeRepository,
            venueRepository);
        return controller.ClaimPairingCode(
            code,
            new ClaimScreenPairingCodeRequest { VenueId = venueId },
            cancellationToken);
    }
}
