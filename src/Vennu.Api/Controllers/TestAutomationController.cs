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

    private BackOfficeSessionOptions? ResolveSession(string? accessToken) =>
        string.IsNullOrWhiteSpace(accessToken) ? null : backOfficeOptions
            .Get(BackOfficeAuthenticationDefaults.AuthenticationScheme).Sessions
            .Where(session => session.Enabled)
            .FirstOrDefault(session => string.Equals(session.AccessToken, accessToken, StringComparison.Ordinal));

}
