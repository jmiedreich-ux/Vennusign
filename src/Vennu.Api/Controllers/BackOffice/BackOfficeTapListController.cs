using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.BackOffice;
using Vennu.Api.Contracts.PlatformOperations;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers.BackOffice;

[ApiController]
[Route("api/back-office/venues/{venueId:guid}/tap-list")]
[Route("api/venue-admin/venues/{venueId:guid}/tap-list")]
[Authorize(Policy = BackOfficeAuthenticationDefaults.AuthorizationPolicy)]
[BackOfficeVenueScope]
[RequireCapability("content.item.update")]
public sealed class BackOfficeTapListController(
    ITapListAdministrationService service,
    IScreenUpdateNotifier notifier) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TapListAdministrationResponse>> Get(Guid venueId, CancellationToken cancellationToken)
    {
        try
        {
            var snapshot = await service.GetAsync(venueId, cancellationToken).ConfigureAwait(false);
            return Ok(new TapListAdministrationResponse(snapshot.Categories, snapshot.Items));
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPost("categories")]
    public async Task<ActionResult<TapCategory>> CreateCategory(Guid venueId, TapCategoryWriteRequest request, CancellationToken cancellationToken) =>
        await ExecuteCreateAsync(
            venueId,
            () => service.CreateCategoryAsync(venueId, Category(request), cancellationToken),
            cancellationToken).ConfigureAwait(false);

    [HttpPut("categories/{categoryId:guid}")]
    public async Task<ActionResult<TapCategory>> UpdateCategory(
        Guid venueId, Guid categoryId, TapCategoryWriteRequest request, CancellationToken cancellationToken) =>
        await ExecuteUpdateAsync(
            venueId,
            () => service.UpdateCategoryAsync(venueId, categoryId, Category(request), cancellationToken),
            cancellationToken).ConfigureAwait(false);

    [HttpDelete("categories/{categoryId:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid venueId, Guid categoryId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await service.DeleteCategoryAsync(venueId, categoryId, cancellationToken).ConfigureAwait(false)) return NotFound();
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException exception) { return Conflict(new ProblemDetails { Title = "Category is in use.", Detail = exception.Message, Status = 409 }); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPost("items")]
    public async Task<ActionResult<TapItem>> CreateItem(Guid venueId, TapItemWriteRequest request, CancellationToken cancellationToken) =>
        await ExecuteCreateAsync(
            venueId,
            () => service.CreateItemAsync(venueId, Item(request), cancellationToken),
            cancellationToken).ConfigureAwait(false);

    [HttpPut("items/{itemId:guid}")]
    public async Task<ActionResult<TapItem>> UpdateItem(
        Guid venueId, Guid itemId, TapItemWriteRequest request, CancellationToken cancellationToken) =>
        await ExecuteUpdateAsync(
            venueId,
            () => service.UpdateItemAsync(venueId, itemId, Item(request), cancellationToken),
            cancellationToken).ConfigureAwait(false);

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid venueId, Guid itemId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await service.DeleteItemAsync(venueId, itemId, cancellationToken).ConfigureAwait(false)) return NotFound();
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    [HttpPut("categories/order")]
    public Task<IActionResult> ReorderCategories(Guid venueId, TapOrderRequest request, CancellationToken cancellationToken) =>
        ReorderAsync(venueId, () => service.ReorderCategoriesAsync(venueId, request.Ids, cancellationToken), cancellationToken);

    [HttpPut("items/order")]
    public Task<IActionResult> ReorderItems(Guid venueId, TapOrderRequest request, CancellationToken cancellationToken) =>
        ReorderAsync(venueId, () => service.ReorderItemsAsync(venueId, request.Ids, cancellationToken), cancellationToken);

    private async Task<ActionResult<T>> ExecuteCreateAsync<T>(
        Guid venueId, Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        try
        {
            var value = await operation().ConfigureAwait(false);
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(Get), new { venueId }, value);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    private async Task<ActionResult<T>> ExecuteUpdateAsync<T>(
        Guid venueId, Func<Task<T?>> operation, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var value = await operation().ConfigureAwait(false);
            if (value is null) return NotFound();
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return Ok(value);
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    private async Task<IActionResult> ReorderAsync(Guid venueId, Func<Task> operation, CancellationToken cancellationToken)
    {
        try
        {
            await operation().ConfigureAwait(false);
            await NotifyAsync(venueId, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    private Task NotifyAsync(Guid venueId, CancellationToken cancellationToken) =>
        notifier.NotifyVenueContentUpdatedAsync(venueId, new { change = "tap-list" }, cancellationToken);

    private static TapCategory Category(TapCategoryWriteRequest value) =>
        new() { Name = value.Name, CategoryPrice = value.CategoryPrice, IsActive = value.IsActive };

    private static TapItem Item(TapItemWriteRequest value) =>
        new()
        {
            TapCategoryId = value.TapCategoryId, Name = value.Name, Style = value.Style,
            Abv = value.Abv, Ibu = value.Ibu, Description = value.Description, Price = value.Price,
            GlassColor = value.GlassColor, NameColor = value.NameColor,
            IsAvailable = value.IsAvailable, IsComingSoon = value.IsComingSoon
        };
}
