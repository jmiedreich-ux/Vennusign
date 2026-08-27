using Microsoft.AspNetCore.Mvc;

namespace Vennu.TestApi.Controllers;

[ApiController]
[Route("api/test/seed")]
public sealed class SeedController(SeedService seed, ProductApiClient product) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SeedResponse>> Create(SeedRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await seed.SeedAsync(request ?? new SeedRequest(null), cancellationToken).ConfigureAwait(false));
        }
        catch (ProductApiException exception)
        {
            return StatusCode(exception.StatusCode, exception.Message);
        }
    }

    [HttpPost("backdate-availability")]
    public async Task<IActionResult> BackdateAvailability(BackdateAvailabilityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await product.SendAutomationAsync("/api/test-automation/availability/backdate", request, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (ProductApiException exception)
        {
            return StatusCode(exception.StatusCode, exception.Message);
        }
    }

    [HttpPost("history-at")]
    public async Task<IActionResult> WriteHistoryAt(WriteHistoryAtRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await product.SendAutomationAsync("/api/test-automation/history/write-at", request, cancellationToken)
                .ConfigureAwait(false);
            return NoContent();
        }
        catch (ProductApiException exception)
        {
            return StatusCode(exception.StatusCode, exception.Message);
        }
    }

    /// <summary>
    /// Puts away the menus a finished test created, so the shared venue stops filling up.
    ///
    /// Put away rather than deleted, because nothing deletes a menu in this build - and put away is
    /// exactly the right meaning anyway: a put-away menu does not count against the venue's ceiling,
    /// which is the thing that was overflowing.
    ///
    /// Every failure here is swallowed on purpose. This runs after a test has already reached its
    /// verdict, and a cleanup that turns a passing test red tells nobody anything about the product.
    /// The venue's raised ceiling is what stops a missed cleanup mattering.
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult<CleanupResponse>> Cleanup(CleanupRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.AccessToken)) return Ok(new CleanupResponse(0));

        var putAway = 0;
        foreach (var menuId in (request.MenuIds ?? []).Distinct())
        {
            try
            {
                await product.SendAsync(HttpMethod.Put, $"/api/back-office/menus/{menuId}/put-away",
                    request.AccessToken, new { isPutAway = true }, cancellationToken).ConfigureAwait(false);
                putAway++;
            }
            catch (ProductApiException)
            {
                // Already gone, still on a screen, or reset out from under us. None of those are
                // this endpoint's business.
            }
        }

        return Ok(new CleanupResponse(putAway));
    }

    [HttpPost("scale")]
    public async Task<ActionResult<ScaleSeedResponse>> Scale(ScaleSeedRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await seed.SeedScaleAsync(request, cancellationToken).ConfigureAwait(false));
        }
        catch (ProductApiException exception)
        {
            return StatusCode(exception.StatusCode, exception.Message);
        }
    }
}
