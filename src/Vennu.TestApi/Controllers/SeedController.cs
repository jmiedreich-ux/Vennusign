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
