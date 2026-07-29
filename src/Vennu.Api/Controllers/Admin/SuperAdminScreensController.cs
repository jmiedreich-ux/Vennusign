using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Api.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues/{venueId:guid}/screens")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminScreensController(IScreenManagementService screenService) : ControllerBase
{
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
                .UpdateAsync(venueId, screenId, request.Name, request.Location, cancellationToken)
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
}
