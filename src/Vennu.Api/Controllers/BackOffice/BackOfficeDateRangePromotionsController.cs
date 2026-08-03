using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Notifications;
using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/venues/{venueId:guid}/date-range-promotions")]
[Route("api/venue-admin/venues/{venueId:guid}/date-range-promotions")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[BackOfficeVenueScope]
public sealed class BackOfficeDateRangePromotionsController(
    IDateRangePromotionService service,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DateRangePromotion>>> Get(
        Guid venueId, CancellationToken cancellationToken)
    {
        try { return Ok(await service.GetAsync(venueId, cancellationToken).ConfigureAwait(false)); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPost]
    public async Task<ActionResult<DateRangePromotion>> Create(
        Guid venueId, DateRangePromotionWriteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(
                venueId, ToModel(request), timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId }, created);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPut("{promotionId:guid}")]
    public async Task<ActionResult<DateRangePromotion>> Update(
        Guid venueId, Guid promotionId, DateRangePromotionWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.UpdateAsync(
                venueId, promotionId, ToModel(request), timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
            if (updated is null) return NotFound();
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return Ok(updated);
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpDelete("{promotionId:guid}")]
    public async Task<IActionResult> Archive(Guid venueId, Guid promotionId, CancellationToken cancellationToken)
    {
        try
        {
            if (await service.ArchiveAsync(
                venueId, promotionId, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false) is null)
                return NotFound();
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    private Task NotifyAsync(Guid venueId, CancellationToken cancellationToken) =>
        notifier.NotifyVenueContentUpdatedAsync(
            venueId, new { change = "date-range-promotions" }, cancellationToken);

    private static DateRangePromotion ToModel(DateRangePromotionWriteRequest request) => new()
    {
        Name = request.Name,
        StartLocalDate = request.StartLocalDate,
        EndLocalDate = request.EndLocalDate,
        TargetLayout = string.IsNullOrWhiteSpace(request.TargetLayout) ? null : ScreenLayout.Normalize(request.TargetLayout),
        Title = request.Title,
        Body = request.Body,
        Priority = request.Priority,
        IsEnabled = request.IsEnabled
    };
}
