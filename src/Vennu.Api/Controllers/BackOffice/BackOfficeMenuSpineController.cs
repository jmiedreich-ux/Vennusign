using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.Services;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers.BackOffice;

/// <summary>
/// The Menus save model over HTTP: the draft queue, publish, history,
/// "go back to", availability and screen assignment.
/// </summary>
[ApiController]
[Route("api/back-office/menu-spine")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
public sealed class BackOfficeMenuSpineController(
    MenuSpineService spine,
    IMenuLibraryRepository library) : ControllerBase
{
    private Guid VenueId => Guid.Parse(
        User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);

    private string? Author =>
        User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email);

    /// <summary>
    /// The venue's timezone and its configured ceilings. Every Menus surface
    /// renders times in the venue's local time, so the client is told which.
    /// </summary>
    [HttpGet("context")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<MenuContextResponse>> GetContext(CancellationToken cancellationToken)
    {
        var context = await spine.GetContextAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(new MenuContextResponse(context.Timezone, context.Ceilings, context.MenuCount));
    }

    // ----- Availability -------------------------------------------------------------

    /// <summary>
    /// Turning availability off hides the item on every screen immediately.
    /// It is not part of any draft and it survives a publish.
    /// </summary>
    [HttpPut("items/{itemId:guid}/availability")]
    [RequireCapability("content.item.availability_update")]
    public async Task<ActionResult<AvailabilityResponse>> SetAvailability(
        Guid itemId,
        AvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var result = await spine
                .SetAvailabilityAsync(VenueId, itemId, request.IsAvailable, Author, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new AvailabilityResponse(
                result.Item.Id,
                result.Item.Name,
                result.Availability.IsAvailable,
                result.Availability.ChangedUtc,
                result.Availability.ChangedBy,
                result.ScreenIds));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("availability")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<IReadOnlyCollection<AvailabilityStateResponse>>> GetAvailability(
        CancellationToken cancellationToken)
    {
        var states = await library.GetAvailabilityAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(states
            .Select(state => new AvailabilityStateResponse(state.ItemId, state.IsAvailable, state.ChangedUtc, state.ChangedBy))
            .ToArray());
    }

    // ----- Draft queue --------------------------------------------------------------

    [HttpGet("menus/{menuId:guid}/draft")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<DraftResponse>> GetDraft(Guid menuId, CancellationToken cancellationToken)
    {
        var changes = await spine.GetDraftAsync(VenueId, menuId, cancellationToken).ConfigureAwait(false);
        return Ok(ToDraftResponse(changes));
    }

    [HttpPost("menus/{menuId:guid}/draft")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<DraftResponse>> QueueChange(
        Guid menuId,
        DraftChangeRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            await spine.QueueChangeAsync(
                VenueId,
                menuId,
                request.TargetKind,
                request.TargetId,
                request.Field,
                request.BeforeValue,
                request.AfterValue,
                Author,
                cancellationToken).ConfigureAwait(false);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }

        var changes = await spine.GetDraftAsync(VenueId, menuId, cancellationToken).ConfigureAwait(false);
        return Ok(ToDraftResponse(changes));
    }

    /// <summary>Throws the draft away. Irreversible, so it is recorded in history.</summary>
    [HttpDelete("menus/{menuId:guid}/draft")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<DiscardResponse>> DiscardDraft(Guid menuId, CancellationToken cancellationToken)
    {
        var discarded = await spine.DiscardDraftAsync(VenueId, menuId, Author, cancellationToken).ConfigureAwait(false);
        return Ok(new DiscardResponse(discarded));
    }

    // ----- Publish ------------------------------------------------------------------

    /// <summary>
    /// Ships every queued change for this menu and nothing belonging to another.
    /// All or nothing: a failure leaves the screens and the draft untouched.
    /// </summary>
    [HttpPost("menus/{menuId:guid}/publish")]
    [RequireCapability("publishing.release.publish")]
    public async Task<ActionResult<PublishResponse>> Publish(Guid menuId, CancellationToken cancellationToken)
    {
        var result = await spine.PublishAsync(VenueId, menuId, Author, cancellationToken).ConfigureAwait(false);
        return Ok(new PublishResponse(
            result.Event.Version,
            result.ChangeCount,
            result.Event.PublishedUtc,
            result.Event.Author,
            result.Targets
                .Select(target => new PublishTargetResponse(target.ScreenId, target.State))
                .ToArray()));
    }

    [HttpGet("menus/{menuId:guid}/history")]
    [RequireCapability("publishing.history.view")]
    public async Task<ActionResult<IReadOnlyCollection<HistoryEntryResponse>>> GetHistory(
        Guid menuId,
        CancellationToken cancellationToken)
    {
        var ceilings = await library.GetCeilingsAsync(VenueId, cancellationToken).ConfigureAwait(false);
        var retention = ceilings.TryGetValue(MenuCeilings.HistoryRetention, out var configured) ? configured : 50;

        var entries = await library.GetHistoryAsync(VenueId, menuId, retention, cancellationToken).ConfigureAwait(false);
        return Ok(entries
            .Select(entry => new HistoryEntryResponse(
                entry.Kind,
                entry.OccurredUtc,
                entry.Author,
                entry.Detail,
                entry.ReplacedByVersion))
            .ToArray());
    }

    /// <summary>
    /// "Go back to" a moment in time. It produces a draft you then publish —
    /// never a second silent path to the screens.
    /// </summary>
    [HttpPost("menus/{menuId:guid}/go-back-to/{version:long}")]
    [RequireCapability("publishing.release.replace")]
    public async Task<ActionResult<DraftResponse>> GoBackTo(
        Guid menuId,
        long version,
        CancellationToken cancellationToken)
    {
        try
        {
            var changes = await spine
                .GoBackToAsync(VenueId, menuId, version, Author, cancellationToken)
                .ConfigureAwait(false);
            return Ok(ToDraftResponse(changes));
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    // ----- Assignment ----------------------------------------------------------------

    [HttpGet("assignments")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<IReadOnlyCollection<AssignmentResponse>>> GetAssignments(CancellationToken cancellationToken)
    {
        var assignments = await library.GetAssignmentsAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(assignments
            .Select(assignment => new AssignmentResponse(assignment.ScreenId, assignment.MenuId, assignment.AssignedUtc, assignment.AssignedBy))
            .ToArray());
    }

    /// <summary>Puts a menu on a screen. A screen shows exactly one menu.</summary>
    [HttpPut("screens/{screenId:guid}/menu")]
    [RequireCapability("screen.content.target")]
    public async Task<ActionResult<AssignmentResponse>> Assign(
        Guid screenId,
        AssignmentRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var assignment = await spine
                .AssignAsync(VenueId, screenId, request.MenuId, Author, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new AssignmentResponse(
                assignment.ScreenId,
                assignment.MenuId,
                assignment.AssignedUtc,
                assignment.AssignedBy));
        }
        catch (InvalidOperationException)
        {
            // The screen or the menu belongs to another venue. Say nothing about
            // which, or whether it exists at all.
            return NotFound(new { message = "That screen is not one of this venue's screens." });
        }
    }

    /// <summary>
    /// Takes a menu off its screens. It keeps its place and its history; the
    /// venue fallback shows instead.
    /// </summary>
    [HttpDelete("menus/{menuId:guid}/screens")]
    [RequireCapability("screen.content.target")]
    public async Task<ActionResult<TakeOffResponse>> TakeOffScreens(Guid menuId, CancellationToken cancellationToken)
    {
        var released = await spine.TakeOffScreensAsync(VenueId, menuId, Author, cancellationToken).ConfigureAwait(false);
        return Ok(new TakeOffResponse(released));
    }

    private static DraftResponse ToDraftResponse(IReadOnlyCollection<Core.Models.MenuDraftChange> changes) =>
        new(
            changes.Count,
            changes
                .Select(change => new DraftChangeResponse(
                    change.TargetKind,
                    change.TargetId,
                    change.Field,
                    change.BeforeValue,
                    change.AfterValue,
                    change.Author,
                    change.UpdatedUtc))
                .ToArray());
}
