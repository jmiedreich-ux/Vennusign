using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.BackOffice;
using Vennu.Api.BackOffice;
using Vennu.Data.Repositories;
using Vennu.Data.Services;
using Vennu.Core.Models;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/session")]
[Route("api/venue-admin/session")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
public sealed class BackOfficeSessionController(
    IBackOfficeContextRepository contexts,
    IVenueRepository venues,
    ICapabilityDecisionService decisions,
    ICapabilityMessageCatalog messages) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<BackOfficeSessionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BackOfficeSessionResponse>> Get(CancellationToken cancellationToken)
    {
        var venueIdValue = User.FindFirstValue(BackOfficeAuthenticationDefaults.VenueIdClaim);
        if (!Guid.TryParse(venueIdValue, out var venueId))
        {
            return Unauthorized();
        }

        var displayName = User.Identity?.Name ?? "Back Office";
        var source = User.FindFirstValue(BackOfficeAuthenticationDefaults.AuthenticationSourceClaim);
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        Guid? userId = Guid.TryParse(userIdValue, out var parsedUserId) ? parsedUserId : null;

        IReadOnlyCollection<BackOfficeContextResponse> authorized;
        if (string.Equals(source, "customer-session", StringComparison.Ordinal) && userId is Guid customerUserId)
        {
            authorized = (await contexts.GetAuthorizedAsync(customerUserId, cancellationToken).ConfigureAwait(false))
                .Select(context => new BackOfficeContextResponse(
                    context.OrganizationId,
                    context.OrganizationName,
                    context.VenueId,
                    context.VenueName))
                .ToArray();
            if (!authorized.Any(context => context.VenueId == venueId)) return Unauthorized();
        }
        else
        {
            var venue = await venues.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
            var claimOrganizationId = Guid.TryParse(
                User.FindFirstValue(BackOfficeAuthenticationDefaults.OrganizationIdClaim),
                out var parsedOrganizationId) ? parsedOrganizationId : Guid.Empty;
            authorized = [new BackOfficeContextResponse(
                venue?.OrganizationId ?? claimOrganizationId,
                "Configured venue access",
                venueId,
                venue?.Name ?? "Current venue")];
        }

        var active = authorized.Single(context => context.VenueId == venueId);
        var locale = Request.GetTypedHeaders().AcceptLanguage?.FirstOrDefault()?.Value.Value ?? "en-US";
        var correlationId = HttpContext.TraceIdentifier;
        var evaluated = await decisions.EvaluateBatchAsync(
            Version1CapabilityRegistry.Definitions.Select(definition => definition.Id).ToArray(),
            correlationId,
            locale,
            cancellationToken).ConfigureAwait(false);
        var capabilityDecisions = evaluated.Select(decision => new BackOfficeCapabilityDecisionResponse(
            decision.Capability.Value,
            ToApiValue(decision.Decision),
            decision.ReasonCode,
            ToApiValue(decision.Category),
            decision.MessageKey,
            messages.Resolve(decision.Locale, decision.MessageKey, decision.Parameters),
            decision.Parameters,
            decision.CorrelationId,
            decision.Locale,
            decision.Resolution,
            decision.RetryAfter is TimeSpan retry ? (int)Math.Ceiling(retry.TotalSeconds) : null,
            decision.Conditions.Select(condition => new BackOfficeCapabilityDecisionConditionResponse(
                ToApiValue(condition.Category),
                condition.ReasonCode,
                condition.MessageKey,
                messages.Resolve(decision.Locale, condition.MessageKey, condition.Parameters),
                condition.Parameters,
                condition.Resolution)).ToArray())).ToArray();
        return Ok(new BackOfficeSessionResponse(
            venueId,
            displayName,
            capabilityDecisions,
            active.OrganizationId == Guid.Empty ? null : active.OrganizationId,
            active.OrganizationName,
            active.VenueName,
            new BackOfficeAccountResponse(userId, displayName, email),
            authorized));
    }

    private static string ToApiValue<T>(T value) where T : Enum =>
        System.Text.RegularExpressions.Regex.Replace(value.ToString(), "([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
}
