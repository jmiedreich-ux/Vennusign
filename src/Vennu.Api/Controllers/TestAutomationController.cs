using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vennu.Api.BackOffice;
using Vennu.Api.TestAutomation;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.DataAccess;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/test-automation")]
public sealed class TestAutomationController(
    TestAutomationAuthorization authorization,
    IOptionsMonitor<BackOfficeAuthenticationOptions> backOfficeOptions,
    IContentRepository content,
    ISqlDataAccess dataAccess) : ControllerBase
{
    public sealed record BackdateAvailabilityRequest(string? AccessToken, Guid ItemId, int MinutesAgo);
    public sealed record ResetVenueRequest(string? AccessToken);

    [HttpPost("availability/backdate")]
    public async Task<IActionResult> BackdateAvailability(BackdateAvailabilityRequest request, CancellationToken cancellationToken)
    {
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
            ChangedUtc = existing.ChangedUtc.AddMinutes(-Math.Abs(request.MinutesAgo)),
            ChangedBy = existing.ChangedBy
        }, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("venues/reset")]
    public async Task<IActionResult> ResetVenue(ResetVenueRequest request, CancellationToken cancellationToken)
    {
        var session = ResolveSession(request.AccessToken);
        if (session is null || !authorization.Allows(Request, "venue.reset", session.VenueId)) return NotFound();
        const string sql = """
            DELETE t FROM dbo.MenuPublishTargets t WHERE t.VenueId = @VenueId;
            DELETE h FROM dbo.MenuHistoryEntries h WHERE h.VenueId = @VenueId;
            DELETE e FROM dbo.MenuPublishEvents e WHERE e.VenueId = @VenueId;
            DELETE a FROM dbo.MenuScreenAssignments a WHERE a.VenueId = @VenueId;
            DELETE p FROM dbo.Placements p WHERE p.VenueId = @VenueId;
            DELETE av FROM dbo.ItemAvailability av WHERE av.VenueId = @VenueId;
            DELETE i FROM dbo.Items i WHERE i.VenueId = @VenueId;
            DELETE s FROM dbo.MenuSections s WHERE s.VenueId = @VenueId;
            DELETE m FROM dbo.Menus m WHERE m.VenueId = @VenueId;
            DELETE sc FROM dbo.Screens sc WHERE sc.VenueId = @VenueId;
            SELECT 1 AS Value;
            """;
        _ = await dataAccess.ExecuteSqlQueryAsync<ResetResult, object>(sql, new { session.VenueId }, cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    private BackOfficeSessionOptions? ResolveSession(string? accessToken) =>
        string.IsNullOrWhiteSpace(accessToken) ? null : backOfficeOptions
            .Get(BackOfficeAuthenticationDefaults.AuthenticationScheme).Sessions
            .Where(session => session.Enabled)
            .FirstOrDefault(session => string.Equals(session.AccessToken, accessToken, StringComparison.Ordinal));

    private sealed class ResetResult { public int Value { get; set; } }
}
