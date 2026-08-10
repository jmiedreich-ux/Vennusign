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
[Route("api/back-office/content")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
public sealed class BackOfficeContentController(
    ContentService content,
    IContentRepository library) : ControllerBase
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
        var context = await content.GetContextAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(new MenuContextResponse(context.Timezone, context.Ceilings, context.MenuCount));
    }

    // ----- The shelf -----------------------------------------------------------------

    /// <summary>
    /// Every menu the venue has, as the Menus home shelf draws it: the board its
    /// screens are showing, how many changes are waiting, and which screens it is on.
    ///
    /// One call, whatever the menu count — the shelf ships its scale behaviour this
    /// milestone (Q176), and a request per card would be thirteen diffs on load.
    /// </summary>
    [HttpGet("menus")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<IReadOnlyCollection<ShelfMenuResponse>>> GetShelf(CancellationToken cancellationToken)
    {
        var menus = await content.GetShelfAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(menus
            .Select(menu => new ShelfMenuResponse(
                menu.MenuId,
                menu.Name,
                menu.Theme,
                menu.IsPutAway,
                menu.PublishedVersion,
                menu.LastPublishedUtc,
                menu.LastPublishedBy,
                menu.Draft.Count,
                menu.ScreenIds,
                ToBoardResponse(menu.PublishedBoard)))
            .ToArray());
    }

    /// <summary>
    /// The board one menu's screens are showing, for a single card refreshed after an
    /// act. 404 when the menu has never been published: the shelf already knows which
    /// cards those are and does not ask.
    /// </summary>
    [HttpGet("menus/{menuId:guid}/published-board")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<PublishedBoardResponse>> GetPublishedBoard(
        Guid menuId,
        CancellationToken cancellationToken)
    {
        var board = await content.GetPublishedBoardAsync(VenueId, menuId, cancellationToken).ConfigureAwait(false);
        if (board is null)
        {
            return NotFound(new { message = "This menu has never been published, so no screen is showing it." });
        }

        return Ok(new PublishedBoardResponse(
            board.MenuId,
            board.Version,
            board.PublishedUtc,
            board.Author,
            ToBoardResponse(board.Board)));
    }

    /// <summary>
    /// Duplicates a menu. The copy places the same library items, so a later price
    /// edit reaches both boards (Q20); it has never been published and is on no
    /// screen, because delivery is always a deliberate act.
    /// </summary>
    [HttpPost("menus/{menuId:guid}/duplicate")]
    [RequireCapability("content.menu.manage")]
    public async Task<ActionResult<DuplicateResponse>> Duplicate(Guid menuId, CancellationToken cancellationToken)
    {
        try
        {
            var copy = await content.DuplicateAsync(VenueId, menuId, Author, cancellationToken).ConfigureAwait(false);
            return Ok(new DuplicateResponse(copy.MenuId, copy.Name, copy.ActiveMenuCount));
        }
        catch (MenuCeilingReachedException exception)
        {
            return Conflict(new { reason = "ceiling_reached", message = exception.Message });
        }
        catch (TooManyMenuCopiesException exception)
        {
            return Conflict(new { reason = "too_many_copies", message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    private static BoardResponse? ToBoardResponse(MenuSnapshot? snapshot) =>
        snapshot is null
            ? null
            : new BoardResponse(
                snapshot.MenuId,
                snapshot.Name,
                snapshot.Theme,
                snapshot.DwellSeconds,
                snapshot.LoopWarningSeconds,
                [.. (snapshot.Sections ?? []).Select(section => new BoardSectionResponse(
                    section.SectionId,
                    section.Name,
                    section.SortOrder,
                    [.. (section.Items ?? []).Select(item => new BoardItemResponse(
                        item.ItemId,
                        item.Name,
                        item.Description,
                        item.Price,
                        item.SortOrder))]))]);

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
            var result = await content
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

    /// <summary>
    /// What is different between this menu and the one its screens are showing.
    /// There is deliberately no endpoint to author a change here: edits go through
    /// the menu itself, and this reports what that made different (Q182).
    /// </summary>
    [HttpGet("menus/{menuId:guid}/draft")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<DraftResponse>> GetDraft(Guid menuId, CancellationToken cancellationToken)
    {
        try
        {
            var changes = await content.GetDraftAsync(VenueId, menuId, cancellationToken).ConfigureAwait(false);
            return Ok(ToDraftResponse(changes));
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "That menu is not one of this venue's menus." });
        }
    }

    /// <summary>
    /// Puts the menu back to what its screens are showing, throwing away every
    /// unpublished edit.
    /// </summary>
    [HttpDelete("menus/{menuId:guid}/draft")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<DiscardResponse>> DiscardDraft(Guid menuId, CancellationToken cancellationToken)
    {
        try
        {
            var discarded = await content.DiscardDraftAsync(VenueId, menuId, Author, cancellationToken).ConfigureAwait(false);
            return Ok(new DiscardResponse(discarded));
        }
        catch (MenuPutAwayException exception)
        {
            return Conflict(new { reason = "menu_put_away", message = exception.Message });
        }
        catch (ScreensTakenByAnotherMenuException exception)
        {
            return Conflict(new { reason = "screens_taken", message = exception.Message });
        }
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
        PublishResult result;
        try
        {
            result = await content.PublishAsync(VenueId, menuId, Author, cancellationToken).ConfigureAwait(false);
        }
        catch (MenuNotOnAnyScreenException exception)
        {
            // A real named state, not a silent absence (decision 5, Q80).
            return Conflict(new { reason = "no_screen_paired", message = exception.Message });
        }
        catch (ScreensTakenByAnotherMenuException exception)
        {
            return Conflict(new { reason = "screens_taken", message = exception.Message });
        }
        catch (MenuMovedWhilePublishingException exception)
        {
            return Conflict(new { reason = "menu_kept_changing", message = exception.Message });
        }
        catch (MenuPutAwayException exception)
        {
            return Conflict(new { reason = "menu_put_away", message = exception.Message });
        }

        return Ok(new PublishResponse(
            result.Event.Version,
            result.ChangeCount,
            result.Event.PublishedUtc,
            result.Event.Author,
            result.Targets
                .Select(target => new PublishTargetResponse(target.ScreenId, target.State))
                .ToArray(),
            result.ConflictedScreenIds));
    }

    /// <summary>
    /// Puts a menu away, or back on the shelf. Put away is the terminal state for a
    /// menu this build — nothing deletes a menu — so it is attributable, and a menu
    /// still on a screen is taken off deliberately first.
    /// </summary>
    [HttpPut("menus/{menuId:guid}/put-away")]
    [RequireCapability("content.menu.manage")]
    public async Task<ActionResult<PutAwayResponse>> SetPutAway(
        Guid menuId,
        PutAwayRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            var result = await content
                .SetPutAwayAsync(VenueId, menuId, request.IsPutAway, Author, cancellationToken)
                .ConfigureAwait(false);
            return Ok(new PutAwayResponse(result.Changed, request.IsPutAway, result.ActiveMenuCount));
        }
        catch (MenuStillOnScreensException exception)
        {
            return Conflict(new { reason = "still_on_screens", message = exception.Message });
        }
        catch (MenuCeilingReachedException exception)
        {
            return Conflict(new { reason = "ceiling_reached", message = exception.Message });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = "That menu is not one of this venue's menus." });
        }
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
                entry.ReplacedByVersion,
                entry.Version))
            .ToArray());
    }

    /// <summary>
    /// "Go back to" a moment in time. It produces a draft you then publish —
    /// never a second silent path to the screens.
    /// </summary>
    [HttpPost("menus/{menuId:guid}/go-back-to/{version:long}")]
    [RequireCapability("publishing.release.replace")]
    public async Task<ActionResult<RestoreResponse>> GoBackTo(
        Guid menuId,
        long version,
        CancellationToken cancellationToken)
    {
        try
        {
            var restore = await content
                .GoBackToAsync(VenueId, menuId, version, Author, cancellationToken)
                .ConfigureAwait(false);
            var draft = ToDraftResponse(restore.Draft);
            return Ok(new RestoreResponse(draft.Count, draft.Changes, restore.ReplacedChangeCount));
        }
        catch (ScreensTakenByAnotherMenuException exception)
        {
            return Conflict(new { reason = "screens_taken", message = exception.Message });
        }
        catch (MenuPutAwayException exception)
        {
            return Conflict(new { reason = "menu_put_away", message = exception.Message });
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

    /// <summary>
    /// What every screen in this venue is showing. Answers the question the model is
    /// built around - the screens show the last published version - which until now
    /// nothing could be asked.
    /// </summary>
    [HttpGet("screens/showing")]
    [RequireCapability("publishing.history.view")]
    public async Task<ActionResult<IReadOnlyCollection<ScreenShowingResponse>>> GetScreensShowing(CancellationToken cancellationToken)
    {
        var showing = await library.GetScreensShowingAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(showing
            .Select(screen => new ScreenShowingResponse(
                screen.ScreenId,
                screen.ScreenName,
                screen.MenuId,
                screen.MenuName,
                screen.Version,
                screen.PublishedUtc,
                screen.Author))
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
            var assignment = await content
                .AssignAsync(VenueId, screenId, request.MenuId, Author, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new AssignmentResponse(
                assignment.ScreenId,
                assignment.MenuId,
                assignment.AssignedUtc,
                assignment.AssignedBy));
        }
        catch (MenuPutAwayException exception)
        {
            return Conflict(new { reason = "menu_put_away", message = exception.Message });
        }
        catch (InvalidOperationException)
        {
            // The screen or the menu belongs to another venue. Say nothing about
            // which, or whether it exists at all.
            return NotFound(new { message = "That screen is not one of this venue's screens." });
        }
    }

    /// <summary>
    /// Queues taking a menu off its screens. Unlike an 86 this is permanent, so it
    /// waits in the draft and reaches the screens on the next Publish (Q68). The
    /// menu keeps its place and its history either way.
    /// </summary>
    [HttpDelete("menus/{menuId:guid}/screens")]
    [RequireCapability("screen.content.target")]
    public async Task<ActionResult<DraftResponse>> TakeOffScreens(Guid menuId, CancellationToken cancellationToken)
    {
        await content.QueueTakeOffScreensAsync(VenueId, menuId, Author, cancellationToken).ConfigureAwait(false);
        var changes = await content.GetDraftAsync(VenueId, menuId, cancellationToken).ConfigureAwait(false);
        return Ok(ToDraftResponse(changes));
    }

    private static DraftResponse ToDraftResponse(IReadOnlyList<SnapshotChange> changes) =>
        new(
            changes.Count,
            changes
                .Select(change => new DraftChangeResponse(
                    change.TargetKind,
                    change.TargetId,
                    change.Field,
                    change.BeforeValue,
                    change.AfterValue))
                .ToArray());
}
