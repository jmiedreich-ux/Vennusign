using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Api.Contracts.Display;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues/{venueId:guid}/emergency-broadcasts")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminEmergencyBroadcastsController(
    IEmergencyBroadcastService service,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EmergencyBroadcast>>> Get(
        Guid venueId, CancellationToken cancellationToken)
    {
        try { return Ok(await service.GetAsync(venueId, cancellationToken).ConfigureAwait(false)); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPost]
    public async Task<ActionResult<EmergencyBroadcast>> Create(
        Guid venueId, EmergencyBroadcastWriteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(
                venueId, request.ScreenId, request.Title, request.Message, request.MediaUrl,
                request.DurationMinutes, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
            await NotifyAsync(created, DisplayEmergencyBroadcastResponse.From(created), cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId }, created);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpDelete("{broadcastId:guid}")]
    public async Task<IActionResult> Cancel(
        Guid venueId, Guid broadcastId, CancellationToken cancellationToken)
    {
        try
        {
            var broadcast = await service.CancelAsync(
                venueId, broadcastId, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
            if (broadcast is null) return NotFound();
            await NotifyAsync(broadcast, null, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    private Task NotifyAsync(
        EmergencyBroadcast broadcast, DisplayEmergencyBroadcastResponse? active, CancellationToken cancellationToken)
    {
        var payload = new { change = "emergency-broadcast", emergencyBroadcast = active };
        return broadcast.ScreenId.HasValue
            ? notifier.NotifyScreenContentUpdatedAsync(broadcast.ScreenId.Value, payload, cancellationToken)
            : notifier.NotifyVenueContentUpdatedAsync(broadcast.VenueId, payload, cancellationToken);
    }
}
