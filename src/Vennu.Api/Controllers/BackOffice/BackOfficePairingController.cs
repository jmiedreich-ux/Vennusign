using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Controllers;
using Vennu.Api.Contracts.Screens;
using Vennu.Api.BackOffice;
using Vennu.Data.Repositories;
using Vennu.Data.Services;
using Vennu.Api.Contracts.PlatformOperations;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/screens/pairing")]
[Route("api/venue-admin/screens/pairing")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[RequireCapability("screen.device.pair")]
public sealed class BackOfficePairingController(
    IScreenRepository screenRepository,
    IScreenPairingCodeRepository pairingCodeRepository,
    IVenueRepository venueRepository,
    IScreenReplacementService? replacementService = null) : ControllerBase
{
    [HttpPost("{code}/claim")]
    public async Task<ActionResult<ClaimScreenPairingCodeResponse>> Claim(
        string code,
        CancellationToken cancellationToken)
    {
        var venueId = Guid.Parse(
            User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);
        var controller = new ScreensController(
            screenRepository,
            pairingCodeRepository,
            venueRepository);
        return await controller.ClaimPairingCode(
            code,
            new ClaimScreenPairingCodeRequest { VenueId = venueId },
            cancellationToken).ConfigureAwait(false);
    }

    [HttpPost("replacement/preview")]
    public async Task<ActionResult<ScreenReplacementResponse>> PreviewReplacement(
        ScreenReplacementRequest request,
        CancellationToken cancellationToken)
    {
        if (replacementService is null) return Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
        try
        {
            var result = await replacementService.PreviewAsync(VenueId(), request.TargetScreenId, request.PairingCode, cancellationToken).ConfigureAwait(false);
            return MapReplacement(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid replacement request.", Detail = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    [HttpPost("replacement")]
    public async Task<ActionResult<ScreenReplacementResponse>> Replace(
        ScreenReplacementRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.Confirmed) return BadRequest(new ProblemDetails { Title = "Replacement confirmation is required.", Status = StatusCodes.Status400BadRequest });
        if (!request.ExpectedTargetUpdatedUtc.HasValue) return BadRequest(new ProblemDetails { Title = "A current replacement preview is required.", Status = StatusCodes.Status400BadRequest });
        if (replacementService is null) return Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
        try
        {
            var result = await replacementService.ReplaceAsync(VenueId(), request.TargetScreenId, request.PairingCode, request.ExpectedTargetUpdatedUtc.Value, User.Identity?.Name ?? "BackOffice", cancellationToken).ConfigureAwait(false);
            return MapReplacement(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid replacement request.", Detail = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    private Guid VenueId() => Guid.Parse(User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);

    private ActionResult<ScreenReplacementResponse> MapReplacement(ScreenReplacementResult result)
    {
        var response = new ScreenReplacementResponse(result.Status.ToString(), result.TargetScreenId, result.SourceScreenId, result.TargetName, result.ReplacementPlatform, result.ReplacementAppVersion, result.WallGroup, result.WallPosition, result.PreservesConfiguration, result.PreservesHistory, result.PreservesVideoWall, result.TargetUpdatedUtc, result.CompletedUtc);
        return result.Status switch
        {
            ScreenReplacementStatus.Ready or ScreenReplacementStatus.Completed => Ok(response),
            ScreenReplacementStatus.PairingCodeNotFound or ScreenReplacementStatus.TargetNotFound or ScreenReplacementStatus.SourceNotFound => NotFound(response),
            ScreenReplacementStatus.PairingCodeExpired => StatusCode(StatusCodes.Status410Gone, response),
            _ => Conflict(response)
        };
    }
}
