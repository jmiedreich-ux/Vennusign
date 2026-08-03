using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Services;

public sealed class QuickUpdateService(
    IMenuRepository menuRepository,
    IVenueRepository venueRepository,
    IFeatureResolutionService featureResolution,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : IQuickUpdateService
{
    public async Task<Menu?> UpdateDailySpecialAsync(
        Guid venueId,
        Guid menuId,
        string? dailySpecial,
        CancellationToken cancellationToken = default)
    {
        await RequireFeatureAsync(venueId, cancellationToken).ConfigureAwait(false);
        var menu = (await menuRepository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(candidate => candidate.Id == menuId);
        if (menu is null)
        {
            return null;
        }

        var normalized = string.IsNullOrWhiteSpace(dailySpecial) ? null : dailySpecial.Trim();
        if (normalized?.Length > 240)
        {
            throw new ArgumentException("Daily special cannot exceed 240 characters.", nameof(dailySpecial));
        }
        menu.DailySpecial = normalized;
        menu.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await menuRepository.UpdateMenuAsync(menu, cancellationToken).ConfigureAwait(false);
        await notifier.NotifyVenueContentUpdatedAsync(
            venueId,
            new { change = "daily-special-updated", menuId, dailySpecial = normalized },
            cancellationToken).ConfigureAwait(false);
        return menu;
    }

    public async Task<MenuItem?> SetAvailabilityAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        await RequireFeatureAsync(venueId, cancellationToken).ConfigureAwait(false);
        var menus = await menuRepository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (menus.All(menu => menu.Id != menuId))
        {
            return null;
        }
        var sections = await menuRepository.GetSectionsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        if (sections.All(section => section.Id != sectionId))
        {
            return null;
        }
        var items = await menuRepository.GetActiveItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false);
        var item = items.SingleOrDefault(candidate => candidate.Id == itemId);
        if (item is null)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        item.IsAvailable = isAvailable;
        item.AvailabilityResetUtc = isAvailable
            ? null
            : await GetNextLocalMidnightUtcAsync(venueId, now, cancellationToken).ConfigureAwait(false);
        item.UpdatedUtc = now.UtcDateTime;
        await menuRepository.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);
        await notifier.NotifyVenueItemAvailabilityChangedAsync(
            venueId,
            item.Id.ToString(),
            item.IsAvailable,
            cancellationToken).ConfigureAwait(false);
        await notifier.NotifyVenueContentUpdatedAsync(
            venueId,
            new { change = "quick-availability-updated", menuId, sectionId, itemId },
            cancellationToken).ConfigureAwait(false);
        return item;
    }

    private async Task RequireFeatureAsync(Guid venueId, CancellationToken cancellationToken)
    {
        if (!await featureResolution.HasFeatureAsync(venueId, "quick_update", cancellationToken).ConfigureAwait(false))
        {
            throw new ArgumentException("Quick Update requires the corresponding venue feature.");
        }
    }

    private async Task<DateTime> GetNextLocalMidnightUtcAsync(
        Guid venueId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Venue does not exist.");
        TimeZoneInfo timezone;
        try
        {
            timezone = TimeZoneInfo.FindSystemTimeZoneById(venue.Timezone);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new InvalidOperationException($"Venue timezone '{venue.Timezone}' is invalid.", exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new InvalidOperationException($"Venue timezone '{venue.Timezone}' is invalid.", exception);
        }

        var localNow = TimeZoneInfo.ConvertTime(utcNow, timezone);
        return LocalTimeOccurrenceResolver.Resolve(timezone, localNow.Date.AddDays(1))
            .ResolvedLocalTime.UtcDateTime;
    }
}
