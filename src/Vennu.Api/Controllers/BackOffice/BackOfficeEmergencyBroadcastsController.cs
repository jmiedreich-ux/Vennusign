using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Contracts.Display;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/venues/{venueId:guid}/emergency-broadcasts")]
[Route("api/venue-admin/venues/{venueId:guid}/emergency-broadcasts")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[BackOfficeVenueScope]
[RequireCapability("publishing.release.publish")]
public sealed class BackOfficeEmergencyBroadcastsController(
    IEmergencyBroadcastService service,
    IScreenUpdateNotifier notifier,
    IScreenRepository screenRepository,
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

    private async Task NotifyAsync(
        EmergencyBroadcast broadcast, DisplayEmergencyBroadcastResponse? active, CancellationToken cancellationToken)
    {
        var payload = new { change = "emergency-broadcast", emergencyBroadcast = active };
        if (broadcast.ScreenId.HasValue)
        {
            await notifier.NotifyScreenContentUpdatedAsync(broadcast.ScreenId.Value, payload, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        // #811: "All venue screens" (ScreenId null) is the pre-selected default in
        // EmergencyBroadcastAdministration.tsx, not an edge case - so this is the
        // common path for an emergency broadcast, and it carries an immediate fact
        // a display should act on right away. NotifyVenueContentUpdatedAsync alone
        // reaches no display: displayConnection.mjs joins only `screen:{id}`, and
        // nothing in the codebase ever calls the hub's JoinVenue (see #769's audit
        // in ContentService.cs). Without this loop the broadcast only reached a
        // screen via the 60s content-poll recovery - the same masking mechanism
        // that hid #769 until #763 measured it. Same fix shape as #763's publish
        // fan-out: notify every one of the venue's screens directly.
        //
        // The venue-wide call is kept alongside the per-screen loop, not replaced
        // by it - same reasoning as PublishAsync's in ContentService.cs.
        await notifier.NotifyVenueContentUpdatedAsync(broadcast.VenueId, payload, cancellationToken)
            .ConfigureAwait(false);
        var screens = await screenRepository.GetByVenueIdAsync(broadcast.VenueId, cancellationToken)
            .ConfigureAwait(false);
        foreach (var screen in screens)
        {
            await notifier.NotifyScreenContentUpdatedAsync(screen.Id, payload, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
