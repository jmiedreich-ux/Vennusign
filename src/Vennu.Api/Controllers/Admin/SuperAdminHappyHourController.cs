using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Admin;
using Vennu.Api.Contracts.Admin;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/venues/{venueId:guid}/happy-hour")]
[Authorize(Policy = SuperAdminAuthenticationDefaults.AuthorizationPolicy)]
public sealed class SuperAdminHappyHourController(
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
