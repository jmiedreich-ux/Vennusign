using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.PlatformOperations;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.PlatformOperations;

[ApiController]
[Route("api/platform-operations/venues/{venueId:guid}/happy-hour")]
[Route("api/admin/venues/{venueId:guid}/happy-hour")]
[Authorize(Policy = PlatformOperationsAuthenticationDefaults.AuthorizationPolicy)]
public sealed class PlatformOperationsHappyHourController(
    IHappyHourService service,
    TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HappyHourResponse>> Get(Guid venueId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(HappyHourResponse.From(
                await service.GetAsync(venueId, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false)));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPut]
    public async Task<ActionResult<HappyHourResponse>> Update(
        Guid venueId,
        HappyHourWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(HappyHourResponse.From(await service.UpdateAsync(
                venueId, request.StartLocalTime, request.EndLocalTime,
                request.ActiveDaysMask, request.IsEnabled, request.OverrideMode,
                timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false)));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }
}
