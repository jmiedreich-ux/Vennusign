using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Menus;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/menu-imports")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[RequireCapability("content.menu.import")]
public sealed class BackOfficeMenuImportsController(MenuImportService imports) : ControllerBase
{
    private Guid VenueId => Guid.Parse(User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim)!);
    private string? Actor => User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email);

    [HttpPost]
    public async Task<ActionResult<MenuImportAggregate>> Start(StartMenuImportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await imports.StartAsync(VenueId, request.RawPaste, Actor, cancellationToken).ConfigureAwait(false);
            SetEtag(aggregate);
            return CreatedAtAction(nameof(Get), new { sessionId = aggregate.Session.Id }, aggregate);
        }
        catch (MenuImportValidationException exception)
        {
            return BadRequest(new { reason = "invalid_paste", message = exception.Message });
        }
    }

    [HttpGet("{sessionId:guid}")]
    public async Task<ActionResult<MenuImportAggregate>> Get(Guid sessionId, CancellationToken cancellationToken)
    {
        MenuImportAggregate? aggregate;
        try { aggregate = await imports.GetAsync(VenueId, sessionId, cancellationToken).ConfigureAwait(false); }
        catch (MenuImportValidationException exception) { return Conflict(new { reason = "allowance_changed", message = exception.Message }); }
        if (aggregate is null) return NotFound(new { reason = "missing_or_expired", message = "This import is no longer available." });
        SetEtag(aggregate);
        return Ok(aggregate);
    }

    [HttpPut("{sessionId:guid}/answers/{questionKey}")]
    public async Task<ActionResult<MenuImportAggregate>> PutAnswer(Guid sessionId, string questionKey, PutMenuImportAnswerRequest request,
        CancellationToken cancellationToken) => await Mutate(() => imports.PutAnswerAsync(VenueId, sessionId, RequiredRevision(), questionKey,
            request.Fingerprint, request.Choice, request.SelectedItemId, Actor, cancellationToken));

    [HttpPost("{sessionId:guid}/accept-safe-matches")]
    public async Task<ActionResult<MenuImportAggregate>> AcceptSafeMatches(Guid sessionId, CancellationToken cancellationToken) =>
        await Mutate(() => imports.AcceptSafeMatchesAsync(VenueId, sessionId, RequiredRevision(), Actor, cancellationToken));

    [HttpPost("{sessionId:guid}/lines/{lineNumber:int}/promote-to-section")]
    public async Task<ActionResult<MenuImportAggregate>> Promote(Guid sessionId, int lineNumber, CancellationToken cancellationToken) =>
        await Mutate(() => imports.SetSectionOverrideAsync(VenueId, sessionId, RequiredRevision(), lineNumber, true, Actor, cancellationToken));

    [HttpDelete("{sessionId:guid}/lines/{lineNumber:int}/section-promotion")]
    public async Task<ActionResult<MenuImportAggregate>> UndoPromotion(Guid sessionId, int lineNumber, CancellationToken cancellationToken) =>
        await Mutate(() => imports.SetSectionOverrideAsync(VenueId, sessionId, RequiredRevision(), lineNumber, false, Actor, cancellationToken));

    [HttpPut("{sessionId:guid}/destination/create")]
    public async Task<ActionResult<MenuImportAggregate>> SetCreateDestination(Guid sessionId, SetCreateDestinationRequest request,
        CancellationToken cancellationToken)
    {
        try { return await Mutate(() => imports.SetCreateDestinationAsync(VenueId, sessionId, RequiredRevision(), request.MenuName, Actor, cancellationToken)); }
        catch (ArgumentException exception) { return BadRequest(new { reason = "invalid_name", message = exception.Message }); }
    }

    [HttpPost("{sessionId:guid}/destination/create/confirm")]
    public async Task<ActionResult<MenuImportCreateResponse>> ConfirmCreate(Guid sessionId, CancellationToken cancellationToken)
    {
        try
        {
            var outcome = await imports.ConfirmCreateAsync(VenueId, sessionId, RequiredRevision(), Actor, cancellationToken).ConfigureAwait(false);
            if (outcome.Result is MenuImportCreateOutcome.Created or MenuImportCreateOutcome.AlreadyCompleted)
            {
                if (outcome.Aggregate is not null) SetEtag(outcome.Aggregate);
                return Ok(new MenuImportCreateResponse(outcome.Result, outcome.MenuId!.Value, outcome.Aggregate!));
            }
            if (outcome.Result == MenuImportMutationOutcome.Conflict && outcome.Aggregate is not null)
            {
                SetEtag(outcome.Aggregate);
                return Conflict(new { reason = "stale_revision", message = "This import changed in another window. Review the latest state and try again.", current = outcome.Aggregate });
            }
            if (outcome.Result == MenuImportCreateOutcome.NameConflict) return Conflict(new { reason = "name_conflict", message = "A menu with that name already exists. Choose another name." });
            if (outcome.Result == MenuImportCreateOutcome.MenuLimit) return Conflict(new { reason = "menu_limit", message = "This venue has reached its menu limit. Put a menu away, then try again." });
            if (outcome.Result == MenuImportCreateOutcome.ItemLimit) return Conflict(new { reason = "item_limit", message = "This import no longer fits the venue's item limit. Nothing was created." });
            if (outcome.Result == MenuImportCreateOutcome.InvalidContent) return Conflict(new { reason = "invalid_content", message = "One or more pasted lines cannot become menu items yet. Return to review and correct them." });
            if (outcome.Result == MenuImportMutationOutcome.Expired) return StatusCode(StatusCodes.Status410Gone, new { reason = "expired", message = "This import has expired. Paste the menu again to restart." });
            if (outcome.Result == MenuImportMutationOutcome.Invalid) return Conflict(new { reason = "not_ready", message = "Finish every required review answer before creating the menu." });
            return NotFound(new { reason = "not_found", message = "This import could not be found." });
        }
        catch (MenuImportValidationException exception) { return Conflict(new { reason = "allowance_changed", message = exception.Message }); }
    }

    private async Task<ActionResult<MenuImportAggregate>> Mutate(Func<Task<MenuImportMutationOutcome>> action)
    {
        try { return await Respond(await action().ConfigureAwait(false)); }
        catch (MenuImportValidationException exception) { return Conflict(new { reason = "allowance_changed", message = exception.Message }); }
    }

    private async Task<ActionResult<MenuImportAggregate>> Respond(MenuImportMutationOutcome outcome)
    {
        await Task.CompletedTask;
        if (outcome.Result == MenuImportMutationOutcome.Updated && outcome.Aggregate is not null) { SetEtag(outcome.Aggregate); return Ok(outcome.Aggregate); }
        if (outcome.Result == MenuImportMutationOutcome.Conflict && outcome.Aggregate is not null) { SetEtag(outcome.Aggregate); return Conflict(new { reason = "stale_revision", message = "This import changed in another window. Review the latest answers and try again.", current = outcome.Aggregate }); }
        if (outcome.Result == MenuImportMutationOutcome.Expired) return StatusCode(StatusCodes.Status410Gone, new { reason = "expired", message = "This import has expired. Paste the menu again to restart." });
        if (outcome.Result == MenuImportMutationOutcome.Invalid) return BadRequest(new { reason = "invalid_answer", message = "That answer does not apply to the current review question." });
        return NotFound(new { reason = "not_found", message = "This import could not be found." });
    }

    private byte[] RequiredRevision()
    {
        var value = Request.GetTypedHeaders().IfMatch.FirstOrDefault()?.Tag.Value?.Trim('"');
        if (string.IsNullOrWhiteSpace(value)) throw new BadHttpRequestException("An If-Match revision is required.", StatusCodes.Status428PreconditionRequired);
        try { var bytes = Convert.FromBase64String(value); return bytes.Length == 8 ? bytes : throw new FormatException(); }
        catch (FormatException) { throw new BadHttpRequestException("If-Match must contain the session's base64 revision.", StatusCodes.Status400BadRequest); }
    }

    private void SetEtag(MenuImportAggregate aggregate) => Response.Headers.ETag = $"\"{Convert.ToBase64String(aggregate.Session.Revision)}\"";
}

public sealed record StartMenuImportRequest(string RawPaste);
public sealed record PutMenuImportAnswerRequest(string Fingerprint, string Choice, Guid? SelectedItemId);
public sealed record SetCreateDestinationRequest(string MenuName);
public sealed record MenuImportCreateResponse(string Result, Guid MenuId, MenuImportAggregate Import);
