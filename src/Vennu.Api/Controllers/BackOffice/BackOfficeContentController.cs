using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.Services;
using Vennu.Api.Menus;
using Vennu.Core.Models;
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
    IContentRepository library,
    MenuBuilderConfigurationResolver configurationResolver) : ControllerBase
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

    [HttpGet("configuration")]
    public async Task<ActionResult<MenuBuilderConfigurationResponse>> GetConfiguration(CancellationToken cancellationToken)
    {
        var resolved = await configurationResolver.ResolveAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(new MenuBuilderConfigurationResponse(
            resolved.ImportFileSizeLimitBytes,
            resolved.PublishRetrySilenceThresholdSeconds,
            resolved.HistoryRetentionDepth));
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
                [.. (snapshot.Pages ?? []).Select(page => new PageResponse(page.PageId, page.Name ?? string.Empty, page.SortOrder))],
                [.. (snapshot.Sections ?? []).Select(section => new BoardSectionResponse(
                    section.SectionId,
                    section.PageId,
                    section.Name,
                    section.SortOrder,
                    [.. (section.Items ?? []).Select(item => new BoardItemResponse(
                        item.ItemId,
                        item.Name,
                        item.Description,
                        item.Price,
                        item.SortOrder))]))]);

    // ----- Pages --------------------------------------------------------------------

    [HttpGet("menus/{menuId:guid}/pages")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<IReadOnlyCollection<PageResponse>>> GetPages(Guid menuId, CancellationToken cancellationToken)
    {
        var pages = await library.GetPagesAsync(VenueId, menuId, cancellationToken).ConfigureAwait(false);
        return Ok(pages.Select(page => new PageResponse(page.Id, page.Name, page.SortOrder)).ToArray());
    }

    [HttpPost("menus/{menuId:guid}/pages")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<PageResponse>> AddPage(Guid menuId, PageNameRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Give this page a name." });
        var page = await library.CreatePageAsync(VenueId, menuId, Guid.NewGuid(), name, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        return page is null ? NotFound(new { message = "That menu is not one of this venue's menus." }) : Ok(new PageResponse(page.Id, page.Name, page.SortOrder));
    }

    [HttpPut("menus/{menuId:guid}/pages/{pageId:guid}")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> RenamePage(Guid menuId, Guid pageId, PageNameRequest request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(new { message = "Give this page a name." });
        return await library.RenamePageAsync(VenueId, menuId, pageId, name, DateTime.UtcNow, cancellationToken).ConfigureAwait(false)
            ? NoContent() : NotFound(new { message = "That page is not on this menu." });
    }

    [HttpPut("menus/{menuId:guid}/pages/order")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> ReorderPages(Guid menuId, PageOrderRequest request, CancellationToken cancellationToken)
    {
        var outcome = await library.ReorderPagesGuardedAsync(VenueId, menuId, request.PageIds, DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        return outcome.Outcome == ReorderOutcomes.Reordered ? NoContent() : Conflict(new { reason = ReorderOutcomes.OrderStale, message = "The pages changed while you were dragging. Nothing moved — reload and try again." });
    }

    [HttpPost("menus/{menuId:guid}/pages/{pageId:guid}/duplicate")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<PageResponse>> DuplicatePage(Guid menuId, Guid pageId, CancellationToken cancellationToken)
    {
        var page = await library.DuplicatePageAsync(VenueId, menuId, pageId, Guid.NewGuid(), DateTime.UtcNow, cancellationToken).ConfigureAwait(false);
        return page is null ? NotFound(new { message = "That page is not on this menu." }) : Ok(new PageResponse(page.Id, page.Name, page.SortOrder));
    }

    [HttpDelete("menus/{menuId:guid}/pages/{pageId:guid}")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<PageDeleteResponse>> DeletePage(Guid menuId, Guid pageId, [FromBody] PageDeleteRequest? request, CancellationToken cancellationToken)
    {
        var outcome = await library.DeletePageAsync(VenueId, menuId, pageId, request?.MoveSectionsToPageId, request?.DeleteSections ?? false, cancellationToken).ConfigureAwait(false);
        return outcome.Outcome switch
        {
            "deleted" => Ok(new PageDeleteResponse(
                request?.DeleteSections == true ? 0 : outcome.AffectedSectionCount,
                request?.DeleteSections == true ? outcome.AffectedSectionCount : 0,
                outcome.RemovedAssignmentCount)),
            "last_page" => Conflict(new { reason = "last_page", message = "A menu always keeps one page." }),
            "move_required" => Conflict(new { reason = "move_required", message = "Choose another page for these sections before deleting this page." }),
            "item_conflict" => Conflict(new { reason = "item_conflict", message = "Those pages share an item, so their sections cannot be combined yet. Choose another page." }),
            _ => NotFound(new { message = "That page is not on this menu." })
        };
    }

    // ----- Availability -------------------------------------------------------------

    [HttpGet("quick-update")]
    [RequireCapability("content.item.availability_update")]
    public async Task<ActionResult<QuickUpdateBoardResponse>> GetQuickUpdateBoard(CancellationToken cancellationToken)
    {
        var shelf = (await content.GetShelfAsync(VenueId, cancellationToken).ConfigureAwait(false))
            .Where(menu => menu.ScreenIds.Count > 0 && menu.PublishedBoard is not null)
            .ToArray();
        var itemIds = shelf.SelectMany(menu => menu.PublishedBoard!.Sections ?? [])
            .SelectMany(section => section.Items ?? []).Select(item => item.ItemId).ToHashSet();
        var availability = (await library.GetAvailabilityAsync(VenueId, cancellationToken).ConfigureAwait(false))
            .Where(state => itemIds.Contains(state.ItemId))
            .Select(state => new AvailabilityStateResponse(state.ItemId, state.IsAvailable, state.ChangedUtc, state.ChangedBy))
            .ToArray();
        var screenIds = shelf.SelectMany(menu => menu.ScreenIds).ToHashSet();
        var screens = (await library.GetScreensShowingAsync(VenueId, cancellationToken).ConfigureAwait(false))
            .Where(screen => screenIds.Contains(screen.ScreenId))
            .Select(screen => new ScreenShowingResponse(screen.ScreenId, screen.ScreenName, screen.Location, screen.Status,
                screen.LastSeenUtc, screen.WidthPixels, screen.HeightPixels, screen.MenuId, screen.MenuName,
                screen.Version, screen.PublishedUtc, screen.Author)).ToArray();
        var context = await content.GetContextAsync(VenueId, cancellationToken).ConfigureAwait(false);
        return Ok(new QuickUpdateBoardResponse(
            context.Timezone,
            shelf.Select(menu => new QuickUpdateMenuResponse(menu.MenuId, menu.Name, menu.ScreenIds, ToBoardResponse(menu.PublishedBoard)!)).ToArray(),
            availability,
            screens));
    }

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

    [HttpPost("availability/restore-all")]
    [RequireCapability("content.item.availability_update")]
    public async Task<ActionResult<RestoreAllAvailabilityResponse>> RestoreAllAvailability(CancellationToken cancellationToken)
    {
        var result = await content.RestoreAllAvailabilityAsync(VenueId, Author, cancellationToken).ConfigureAwait(false);
        return Ok(new RestoreAllAvailabilityResponse(result.Count, result.ScreenIds));
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
        var retention = (await configurationResolver.ResolveAsync(VenueId, cancellationToken).ConfigureAwait(false))
            .HistoryRetentionDepth;

        var entries = await library.GetHistoryAsync(VenueId, menuId, retention, cancellationToken).ConfigureAwait(false);
        return Ok(entries
            .Select(entry => new HistoryEntryResponse(
                entry.Kind,
                entry.OccurredUtc,
                entry.Author,
                entry.Detail,
                entry.ReplacedByVersion,
                entry.Version,
                entry.PageId,
                entry.PageName))
            .ToArray());
    }

    [HttpGet("menus/{menuId:guid}/pages/{pageId:guid}/history")]
    [RequireCapability("publishing.history.view")]
    public async Task<ActionResult<IReadOnlyCollection<HistoryEntryResponse>>> GetPageHistory(
        Guid menuId,
        Guid pageId,
        CancellationToken cancellationToken)
    {
        var retention = (await configurationResolver.ResolveAsync(VenueId, cancellationToken).ConfigureAwait(false))
            .HistoryRetentionDepth;
        var entries = await library
            .GetPageHistoryAsync(VenueId, menuId, pageId, retention, cancellationToken)
            .ConfigureAwait(false);
        return Ok(entries.Select(entry => new HistoryEntryResponse(
            entry.Kind,
            entry.OccurredUtc,
            entry.Author,
            entry.Detail,
            entry.ReplacedByVersion,
            entry.Version,
            entry.PageId,
            entry.PageName)).ToArray());
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
            .Select(assignment => new AssignmentResponse(assignment.ScreenId, assignment.MenuId, assignment.PageId, assignment.MenuName, assignment.PageName, assignment.AssignedUtc, assignment.AssignedBy))
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
                screen.Location,
                screen.Status,
                screen.LastSeenUtc,
                screen.WidthPixels,
                screen.HeightPixels,
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
        if (request.PageId == Guid.Empty) return BadRequest(new { message = "Choose a page for this screen." });
        if (request.Mode is not ("replace" or "rotate")) return BadRequest(new { message = "Choose replace or rotate." });
        try
        {
            var assignment = await content
                .AssignAsync(VenueId, screenId, request.MenuId, request.PageId, request.Mode == "rotate", Author, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new AssignmentResponse(
                assignment.ScreenId,
                assignment.MenuId,
                assignment.PageId,
                assignment.MenuName,
                assignment.PageName,
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

    [HttpDelete("screens/{screenId:guid}/menus/{menuId:guid}/pages/{pageId:guid}")]
    [RequireCapability("screen.content.target")]
    public async Task<IActionResult> RemovePageAssignment(Guid screenId, Guid menuId, Guid pageId, CancellationToken cancellationToken)
    {
        await library.ClearPageScreenAssignmentAsync(VenueId, screenId, menuId, pageId, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPut("menus/{menuId:guid}/screens")]
    [RequireCapability("screen.content.target")]
    public async Task<IActionResult> SavePageAssignments(Guid menuId, PageAssignmentsRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Changes is null
            || request.Changes.Any(change => change.ScreenId == Guid.Empty || change.PageId == Guid.Empty || change.Mode is not ("remove" or "replace" or "rotate"))
            || request.Changes.Select(change => (change.ScreenId, change.PageId)).Distinct().Count() != request.Changes.Count
            || request.Changes.GroupBy(change => change.ScreenId).Any(group => group.Count(change => change.Mode == "replace") > 1))
            return BadRequest(new { message = "Choose a valid change for every screen." });
        try
        {
            await library.ApplyPageScreenAssignmentsAsync(
                VenueId,
                menuId,
                request.Changes.Select(change => new PageScreenAssignmentChange(change.ScreenId, change.PageId, change.Mode)).ToArray(),
                Author,
                DateTime.UtcNow,
                cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (MenuPutAwayException exception)
        {
            return Conflict(new { reason = "menu_put_away", message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { reason = "assignments_changed", message = exception.Message });
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

    // ----- The builder ---------------------------------------------------------
    //
    // Every write here changes the working state, which is what makes the draft
    // count follow on its own: MenuSnapshot.Diff compares the menu as it stands
    // against the board its screens are showing, so nothing below needs to report
    // a change, and nothing below can report one that a publish would not ship.

    /// <summary>
    /// The menu as the builder opens it: the working board the canvas draws, the
    /// draft it differs by, and the publish that put the current board on screen.
    /// </summary>
    [HttpGet("menus/{menuId:guid}/board")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<BuilderBoardResponse>> GetBuilderBoard(
        Guid menuId,
        CancellationToken cancellationToken)
    {
        var result = await content.GetBuilderBoardAsync(VenueId, menuId, cancellationToken).ConfigureAwait(false);
        if (result is null)
        {
            return NotFound(new { message = "That menu is not one of this venue's menus." });
        }

        var draft = ToDraftResponse(result.Draft);
        return Ok(new BuilderBoardResponse(
            ToBoardResponse(result.Board)!,
            draft.Count,
            draft.Changes,
            result.PublishedVersion,
            result.LastPublishedUtc,
            result.LastPublishedBy,
            [.. (result.Board.Screens ?? []).Select(screen => screen.ScreenId)]));
    }

    /// <summary>
    /// Adds a section at the end of the board (Q95). It lands as a draft change
    /// like everything else the builder does.
    /// </summary>
    [HttpPost("menus/{menuId:guid}/sections")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<SectionResponse>> AddSection(
        Guid menuId,
        SectionNameRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
        {
            return Problem("A section needs a name.", statusCode: 400);
        }

        var sectionId = Guid.NewGuid();
        var outcome = await content
            .AddSectionAsync(VenueId, menuId, sectionId, request.Name, request.PageId, Author, cancellationToken)
            .ConfigureAwait(false);

        return outcome.Outcome == SectionOutcomes.Created
            ? Ok(new SectionResponse(sectionId, request.Name.Trim(), outcome.SortOrder))
            : NotFound(new { message = "That menu is not one of this venue's menus." });
    }

    /// <summary>Renaming happens by typing over the heading on the canvas (Q96).</summary>
    [HttpPut("menus/{menuId:guid}/sections/{sectionId:guid}")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> RenameSection(
        Guid menuId,
        Guid sectionId,
        SectionNameRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
        {
            return Problem("A section needs a name.", statusCode: 400);
        }

        return await content.RenameSectionAsync(VenueId, menuId, sectionId, request.Name, Author, cancellationToken).ConfigureAwait(false)
            ? NoContent()
            : NotFound(new { message = "That section is not on this menu." });
    }

    /// <summary>
    /// Deletes a section after the caller explicitly chooses whether its placements
    /// move to a sibling section or return to the library.
    /// </summary>
    [HttpDelete("menus/{menuId:guid}/sections/{sectionId:guid}")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<SectionDeleteResponse>> DeleteSection(
        Guid menuId,
        Guid sectionId,
        [FromBody] SectionDeleteRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null || (!request.DeletePlacements && request.MoveItemsToSectionId is null))
        {
            return BadRequest(new { message = "Choose where the section's items should go." });
        }

        var outcome = await content
            .DeleteSectionAsync(VenueId, menuId, sectionId, request.MoveItemsToSectionId, request.DeletePlacements, Author, cancellationToken)
            .ConfigureAwait(false);

        return outcome.Outcome switch
        {
            SectionOutcomes.Deleted => Ok(new SectionDeleteResponse(outcome.MovedItemCount, outcome.ReleasedItemCount)),
            SectionOutcomes.SectionMissing => NotFound(new { message = "That section is not on this menu." }),
            SectionOutcomes.DestinationMissing => Conflict(new { message = "That destination section is no longer available. Nothing was changed." }),
            SectionOutcomes.DestinationConflict => Conflict(new { message = "One or more items are already in that destination section. Nothing was changed." }),
            _ => Conflict(new { message = "The section could not be deleted. Nothing was changed." })
        };
    }

    /// <summary>
    /// Reordering refuses rather than half-applies when the list no longer matches
    /// the menu — someone else added or removed something in between, and applying
    /// it to the part that still matches would leave the rest at stale orders.
    /// </summary>
    [HttpPut("menus/{menuId:guid}/sections/order")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> ReorderSections(
        Guid menuId,
        SectionOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.SectionIds is null)
        {
            return Problem("Section identifiers are required.", statusCode: 400);
        }

        var outcome = await content
            .ReorderSectionsAsync(VenueId, menuId, request.SectionIds, Author, cancellationToken)
            .ConfigureAwait(false);

        return outcome.Outcome == ReorderOutcomes.Reordered
            ? NoContent()
            : Conflict(new
            {
                reason = ReorderOutcomes.OrderStale,
                message = "The sections changed while you were dragging. Nothing moved — reload and try again."
            });
    }

    /// <inheritdoc cref="ReorderSections"/>
    [HttpPut("menus/{menuId:guid}/sections/{sectionId:guid}/items/order")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> ReorderItems(
        Guid menuId,
        Guid sectionId,
        ItemOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.ItemIds is null)
        {
            return Problem("Item identifiers are required.", statusCode: 400);
        }

        var outcome = await content
            .ReorderItemsAsync(VenueId, menuId, sectionId, request.ItemIds, Author, cancellationToken)
            .ConfigureAwait(false);

        return outcome.Outcome == ReorderOutcomes.Reordered
            ? NoContent()
            : Conflict(new
            {
                reason = ReorderOutcomes.OrderStale,
                message = "The items changed while you were dragging. Nothing moved — reload and try again."
            });
    }

    [HttpPut("menus/{menuId:guid}/items/{itemId:guid}/placement")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> MoveItem(
        Guid menuId,
        Guid itemId,
        ItemMoveRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.SourceItemIds is null || request.DestinationItemIds is null)
        {
            return Problem("Both section orders are required.", statusCode: 400);
        }
        var outcome = await content.MoveItemAsync(
            VenueId, menuId, itemId, request.SourceSectionId, request.DestinationSectionId,
            request.SourceItemIds, request.DestinationItemIds, Author, cancellationToken).ConfigureAwait(false);
        return outcome.Outcome == ReorderOutcomes.Reordered
            ? NoContent()
            : Conflict(new { reason = ReorderOutcomes.OrderStale, message = "The page changed while you were dragging. Nothing moved — reload and try again." });
    }

    /// <summary>
    /// Puts something in a section: an item the library already holds, or a new one
    /// born with the typed name (Q112/Q113). An item already on this board is
    /// answered with where it sits, so the caller jumps rather than duplicating.
    /// </summary>
    [HttpPost("menus/{menuId:guid}/sections/{sectionId:guid}/items")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<PlaceResponse>> PlaceItem(
        Guid menuId,
        Guid sectionId,
        PlaceRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.ItemId is null && string.IsNullOrWhiteSpace(request?.Name))
        {
            return Problem("Name an item to create, or an item to place.", statusCode: 400);
        }

        if (request.Price?.Trim().Length > Item.PriceMaxLength)
        {
            return Problem($"Price must be {Item.PriceMaxLength} characters or fewer.", statusCode: 400);
        }

        if (request.ItemId is { } existingId)
        {
            var placed = await content
                .PlaceExistingItemAsync(VenueId, menuId, sectionId, existingId, Author, cancellationToken)
                .ConfigureAwait(false);

            return placed.Outcome switch
            {
                PlaceExistingOutcomes.Placed => Ok(new PlaceResponse(
                    placed.Outcome, existingId, sectionId, placed.SortOrder, placed.ItemCountOnMenu)),

                // Not an error: this is Q112's jump, and the caller is told where to.
                PlaceExistingOutcomes.AlreadyOnBoard => Ok(new PlaceResponse(
                    placed.Outcome, existingId, placed.ExistingSectionId, 0, placed.ItemCountOnMenu)),

                PlaceExistingOutcomes.CeilingReached => Problem(
                    await content.DescribeCeilingRefusalAsync(
                        VenueId, MenuCeilings.ItemsPerMenu, placed.ItemCountOnMenu + 1, cancellationToken)
                        .ConfigureAwait(false)
                    ?? "This menu is full.",
                    statusCode: 409),

                _ => NotFound(new { message = "That section or item is not on this venue's menu." })
            };
        }

        var itemId = Guid.NewGuid();
        var created = await content
            .AddNewItemAsync(VenueId, menuId, sectionId, itemId, request.Name!, request.Price, Author, cancellationToken)
            .ConfigureAwait(false);

        return created.Outcome switch
        {
            ItemPlacementOutcomes.Created => Ok(new PlaceResponse(
                PlaceExistingOutcomes.Placed, itemId, sectionId, created.SortOrder, created.ItemCountOnMenu)),

            ItemPlacementOutcomes.OverCeiling => Problem(
                await content.DescribeCeilingRefusalAsync(
                    VenueId, MenuCeilings.ItemsPerMenu, created.ItemCountOnMenu + 1, cancellationToken)
                    .ConfigureAwait(false)
                ?? "This menu is full.",
                statusCode: 409),

            _ => NotFound(new { message = "That section is not on this venue's menu." })
        };
    }

    /// <summary>
    /// Takes an item off this board. It stays in the library, so it can be placed
    /// again here or anywhere else (Q97).
    /// </summary>
    [HttpDelete("menus/{menuId:guid}/pages/{pageId:guid}/items/{itemId:guid}")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> RemoveItem(
        Guid menuId,
        Guid pageId,
        Guid itemId,
        CancellationToken cancellationToken) =>
        await content.RemoveItemFromPageAsync(VenueId, menuId, pageId, itemId, Author, cancellationToken).ConfigureAwait(false)
            ? NoContent()
            : NotFound(new { message = "That item is not on this board." });

    [HttpPut("menus/{menuId:guid}/pages/{pageId:guid}/items/{itemId:guid}/transition")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult> TransitionItemPlacement(
        Guid menuId, Guid pageId, Guid itemId, ItemPlacementTransitionRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.ExpectedItemIds is null || request.DesiredItemIds is null)
            return Problem("Both expected and desired section orders are required.", statusCode: 400);
        var outcome = await content.TransitionPlacementAsync(
            VenueId, menuId, pageId, request.SectionId, itemId,
            request.ExpectedItemIds, request.DesiredItemIds, Author, cancellationToken).ConfigureAwait(false);
        return outcome.Outcome switch
        {
            ReorderOutcomes.Reordered => NoContent(),
            PlaceExistingOutcomes.CeilingReached => Conflict(new
            {
                reason = PlaceExistingOutcomes.CeilingReached,
                message = await content.DescribeCeilingRefusalAsync(
                    VenueId, MenuCeilings.ItemsPerMenu, request.DesiredItemIds.Count, cancellationToken).ConfigureAwait(false)
                    ?? "This menu is full. Nothing changed."
            }),
            _ => Conflict(new { reason = ReorderOutcomes.OrderStale, message = "The page changed after this action. Nothing changed — reload and try again." })
        };
    }

    /// <summary>
    /// Edits an item. One item is one shared price across every board it sits on
    /// (Q5) — each board's screens still change only when that board publishes.
    /// </summary>
    [HttpPut("items/{itemId:guid}")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<BoardItemResponse>> UpdateItem(
        Guid itemId,
        ItemValuesRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Problem("Item values are required.", statusCode: 400);
        }
        if (request.Price?.Trim().Length > Item.PriceMaxLength
            || request.ExpectedPrice?.Trim().Length > Item.PriceMaxLength)
        {
            return Problem($"Price must be {Item.PriceMaxLength} characters or fewer.", statusCode: 400);
        }

        /*
         * The expectation is optional and only Undo sends it. A plain edit is a
         * person deciding now, with the board in front of them; an inverse is a
         * decision made earlier, and it has to prove the ground has not moved
         * under it before it puts anything back.
         */
        var expected = request.ExpectedName is null
            ? null
            : new ItemValueExpectation(request.ExpectedName, request.ExpectedDescription, request.ExpectedPrice);

        var result = await content
            .UpdateItemValuesAsync(
                VenueId,
                itemId,
                request.Name,
                request.Description,
                request.Price,
                expected,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.Outcome == "item_changed")
        {
            return Conflict(new
            {
                reason = "item_changed",
                message = $"“{result.Item?.Name}” changed since you did that, so nothing was moved. "
                    + "Have another look before undoing again."
            });
        }

        return result.Item is null
            ? NotFound(new { message = "That item is not one of this venue's items." })
            : Ok(new BoardItemResponse(
                result.Item.Id,
                result.Item.Name,
                result.Item.Description,
                result.Item.Price,
                0));
    }

    /// <summary>
    /// The add row's search across the whole venue library, 86'd items included,
    /// each result naming the boards it already sits on (Q112/Q123).
    /// </summary>
    [HttpGet("items")]
    [RequireCapability("content.item.update")]
    public async Task<ActionResult<IReadOnlyCollection<LibraryItemResponse>>> SearchItems(
        [FromQuery] string? query,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var bounded = take <= 0 ? 20 : Math.Min(take, 100);
        var results = await content
            .SearchLibraryAsync(VenueId, query, bounded, cancellationToken)
            .ConfigureAwait(false);

        return Ok(results
            .Select(result => new LibraryItemResponse(
                result.Item.Id,
                result.Item.Name,
                result.Item.Description,
                result.Item.Price,
                result.IsAvailable,
                [.. result.Boards.Select(board => new LibraryItemBoardResponse(board.MenuId, board.MenuName))]))
            .ToArray());
    }

    /// <summary>
    /// The menu themes this venue could attach. Always empty: menu themes are built
    /// in the theme editor, which does not exist yet, and no named looks ship (Q86).
    /// The picker reads this rather than a hard-coded list, so it needs no change
    /// when the first theme is built.
    /// </summary>
    [HttpGet("menu-themes")]
    [RequireCapability("content.item.update")]
    public ActionResult<IReadOnlyCollection<MenuThemeResponse>> GetMenuThemes() =>
        Ok(ContentService.GetMenuThemes()
            .Select(theme => new MenuThemeResponse(theme.Key, theme.Name))
            .ToArray());
}
