using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vennu.Api.BackOffice;
using Vennu.Api.TestAutomation;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/test-automation")]
public sealed class TestAutomationController(
    TestAutomationAuthorization authorization,
    IOptionsMonitor<BackOfficeAuthenticationOptions> backOfficeOptions,
    IContentRepository content) : ControllerBase
{
    public sealed record BackdateAvailabilityRequest(string? AccessToken, Guid ItemId, int MinutesAgo);
    public sealed record ResetVenueRequest(string? AccessToken);
    public sealed record WriteHistoryAtRequest(string? AccessToken, Guid MenuId, string Kind, string? Detail, DateTime OccurredUtc);

    [HttpPost("availability/backdate")]
    public async Task<IActionResult> BackdateAvailability(BackdateAvailabilityRequest request, CancellationToken cancellationToken)
    {
        if (request.MinutesAgo is < 1 or > 525_600)
            return ValidationProblem("MinutesAgo must be between 1 and 525600.");

        var session = ResolveSession(request.AccessToken);
        if (session is null || !authorization.Allows(Request, "availability.backdate", session.VenueId)) return NotFound();

        var existing = (await content.GetAvailabilityAsync(session.VenueId, cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(state => state.ItemId == request.ItemId);
        if (existing is null) return NotFound();
        await content.SetAvailabilityAsync(new ItemAvailability
        {
            VenueId = session.VenueId,
            ItemId = request.ItemId,
            IsAvailable = existing.IsAvailable,
            ChangedUtc = existing.ChangedUtc.AddMinutes(-request.MinutesAgo),
            ChangedBy = existing.ChangedBy
        }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("venues/reset")]
    public async Task<IActionResult> ResetVenue(ResetVenueRequest request, CancellationToken cancellationToken)
    {
        var session = ResolveSession(request.AccessToken);
        if (session is null || !authorization.Allows(Request, "venue.reset", session.VenueId)) return NotFound();
        await content.ResetAutomationVenueAsync(session.VenueId, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Gives this venue room for a whole UI run.
    ///
    /// Separate from the reset because the ordinary seed cannot reset - it runs 98 times against a
    /// venue other tests are using in parallel, and wiping it would be the opposite of what those
    /// tests need. Only the scale seed resets, and it does so on a different venue, which is why
    /// raising the ceiling inside reset alone left the owner venue exactly as full as before.
    /// </summary>
    [HttpPost("venues/headroom")]
    public async Task<IActionResult> GiveHeadroom(ResetVenueRequest request, CancellationToken cancellationToken)
    {
        var session = ResolveSession(request.AccessToken);
        if (session is null || !authorization.Allows(Request, "venue.headroom", session.VenueId)) return NotFound();
        await content.GiveAutomationVenueHeadroomAsync(session.VenueId, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("history/write-at")]
    public async Task<IActionResult> WriteHistoryAt(WriteHistoryAtRequest request, CancellationToken cancellationToken)
    {
        var session = ResolveSession(request.AccessToken);
        if (session is null || !authorization.Allows(Request, "history.write_at", session.VenueId)) return NotFound();
        if (request.MenuId == Guid.Empty || !MenuHistoryKinds.IsSupported(request.Kind) || request.OccurredUtc.Kind != DateTimeKind.Utc)
            return ValidationProblem("MenuId, a supported history kind, and a UTC OccurredUtc are required.");
        if (await content.GetWorkingSnapshotAsync(session.VenueId, request.MenuId, cancellationToken).ConfigureAwait(false) is null) return NotFound();
        await content.RecordHistoryAsync(new MenuHistoryEntry
        {
            VenueId = session.VenueId,
            MenuId = request.MenuId,
            Kind = request.Kind,
            Detail = request.Detail,
            Author = session.DisplayName,
            OccurredUtc = request.OccurredUtc
        }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    private BackOfficeSessionOptions? ResolveSession(string? accessToken) =>
        string.IsNullOrWhiteSpace(accessToken) ? null : backOfficeOptions
            .Get(BackOfficeAuthenticationDefaults.AuthenticationScheme).Sessions
            .Where(session => session.Enabled)
            .FirstOrDefault(session => string.Equals(session.AccessToken, accessToken, StringComparison.Ordinal));

}
