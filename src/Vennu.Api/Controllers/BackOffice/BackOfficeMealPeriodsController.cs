using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/venues/{venueId:guid}/meal-periods")]
[Route("api/venue-admin/venues/{venueId:guid}/meal-periods")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[BackOfficeVenueScope]
[RequireCapability("schedule.entry.manage")]
public sealed class BackOfficeMealPeriodsController(
    IMealPeriodAdministrationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MealPeriodAdministrationResponse>> Get(Guid venueId, CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await service.GetAsync(venueId, cancellationToken).ConfigureAwait(false);
            return Ok(new MealPeriodAdministrationResponse(snapshot.MealPeriods, snapshot.Conflicts,
                snapshot.VenueLocalNow, snapshot.ActiveMealPeriodId, snapshot.NextMealPeriodId, snapshot.NextStartsLocal,
                snapshot.NextStartAdjustment));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<MealPeriod>> Create(
        Guid venueId,
        MealPeriodWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(
                venueId, request.Name, request.StartLocalTime, request.EndLocalTime,
                request.ActiveDaysMask, request.IsEnabled, request.TargetLayout,
                request.MenuFilter, request.ThemePresetKey, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId }, created);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("{mealPeriodId:guid}")]
    public async Task<ActionResult<MealPeriod>> Update(
        Guid venueId,
        Guid mealPeriodId,
        MealPeriodWriteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.UpdateAsync(
                venueId, mealPeriodId, request.Name, request.StartLocalTime, request.EndLocalTime,
                request.ActiveDaysMask, request.IsEnabled, request.TargetLayout,
                request.MenuFilter, request.ThemePresetKey, cancellationToken).ConfigureAwait(false);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpDelete("{mealPeriodId:guid}")]
    public async Task<IActionResult> Delete(Guid venueId, Guid mealPeriodId, CancellationToken cancellationToken)
    {
        try
        {
            return await service.DeleteAsync(venueId, mealPeriodId, cancellationToken).ConfigureAwait(false)
                ? NoContent()
                : NotFound();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(exception.Message);
        }
    }

    [HttpPut("order")]
    public async Task<ActionResult<IReadOnlyCollection<MealPeriod>>> Reorder(
        Guid venueId, MealPeriodReorderRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await service.ReorderAsync(venueId, request.OrderedIds, cancellationToken).ConfigureAwait(false)); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }
}
