using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Services;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/venues/{venueId:guid}/screens")]
[Route("api/venue-admin/venues/{venueId:guid}/screens")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[BackOfficeVenueScope]
public sealed class BackOfficeScreensController(
    IScreenManagementService screenService,
    IHaasPreRegistrationService preRegistrationService,
    IScreenTargetingService targetingService,
    IVideoWallService videoWallService) : ControllerBase
{
    [HttpPost("pre-registrations")]
    public async Task<ActionResult<HaasPreRegistrationResponse>> PreRegister(
        Guid venueId,
        HaasPreRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return CreatedAtAction(
                nameof(Get),
                new { venueId },
                await preRegistrationService.CreateAsync(venueId, request, cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ScreenManagementItem>>> Get(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await screenService.GetAsync(venueId, cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<ScreenManagementItem>> Create(
        Guid venueId,
        ScreenCreateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var screen = await screenService
                .CreateAsync(venueId, request.Name, request.Location, cancellationToken)
                .ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId }, screen);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
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
    }

    [HttpPut("{screenId:guid}")]
    public async Task<ActionResult<ScreenManagementItem>> Update(
        Guid venueId,
        Guid screenId,
        ScreenUpdateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var screen = await screenService
                .UpdateAsync(
                    venueId,
                    screenId,
                    request.Name,
                    request.Location,
                    request.PhotoGridDensity,
                    request.DisplayLayout,
                    request.SplitRatio,
                    request.HeroDwellSeconds,
                    cancellationToken)
                .ConfigureAwait(false);
            return screen is null ? NotFound() : Ok(screen);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPost("{screenId:guid}/push")]
    public async Task<IActionResult> Push(
        Guid venueId,
        Guid screenId,
        CancellationToken cancellationToken) =>
        await screenService.PushAsync(venueId, screenId, cancellationToken).ConfigureAwait(false)
            ? NoContent()
            : NotFound();

    [HttpPost("push-all")]
    public async Task<ActionResult<ScreenPushAllResult>> PushAll(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        try
        {
            var count = await targetingService.PushAllAsync(venueId, cancellationToken).ConfigureAwait(false);
            return Ok(new ScreenPushAllResult(count));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("overflow")]
    public async Task<ActionResult<ScreenOverflowPreview>> GetOverflow(
        Guid venueId,
        [FromQuery] int capacity,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await targetingService.GetOverflowAsync(venueId, capacity, cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpGet("video-walls")]
    public async Task<ActionResult<VideoWallSnapshot>> GetVideoWalls(
        Guid venueId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await videoWallService.GetAsync(venueId, cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("video-walls")]
    public async Task<ActionResult<VideoWallGroup>> SaveVideoWall(
        Guid venueId,
        VideoWallSaveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await videoWallService.SaveAsync(
                venueId,
                request.Name,
                request.Layout,
                request.ScreenIds,
                cancellationToken).ConfigureAwait(false));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpDelete("video-walls/{name}")]
    public async Task<IActionResult> RemoveVideoWall(
        Guid venueId,
        string name,
        CancellationToken cancellationToken)
    {
        try
        {
            return await videoWallService.RemoveAsync(venueId, name, cancellationToken).ConfigureAwait(false)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }
}
