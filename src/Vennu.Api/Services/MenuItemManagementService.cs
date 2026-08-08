using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Services;

public sealed class MenuItemManagementService(
    IMenuRepository repository,
    IScreenUpdateNotifier notifier,
    ICapabilityActionAuthorizer capabilityAuthorizer,
    TimeProvider timeProvider) : IMenuItemManagementService
{
    public async Task<MenuItem> CreateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        string? description,
        decimal price,
        decimal? happyHourPrice,
        CancellationToken cancellationToken = default)
    {
        await RequireSectionAsync(venueId, menuId, sectionId, cancellationToken).ConfigureAwait(false);
        await RequireCapabilityChangeAsync(
            "schedule.promotion.automate",
            happyHourPrice is not null,
            "Happy-hour pricing",
            cancellationToken).ConfigureAwait(false);
        var items = await repository.GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            MenuSectionId = sectionId,
            Name = NormalizeName(name),
            Description = NormalizeDescription(description),
            Price = ValidatePrice(price, nameof(price)),
            HappyHourPrice = ValidateOptionalPrice(happyHourPrice, nameof(happyHourPrice)),
            SortOrder = items.Count == 0 ? 0 : items.Max(existing => existing.SortOrder) + 1,
            IsAvailable = true,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        await repository.CreateItemAsync(item, cancellationToken).ConfigureAwait(false);
        await NotifyAsync(venueId, menuId, sectionId, item.Id, "created", cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<MenuItem?> UpdateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        string name,
        string? description,
        decimal price,
        decimal? happyHourPrice,
        CancellationToken cancellationToken = default)
    {
        RequireId(itemId, nameof(itemId));
        if (!await SectionExistsAsync(venueId, menuId, sectionId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var items = await repository.GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false);
        var item = items.SingleOrDefault(existing => existing.Id == itemId);
        if (item is null)
        {
            return null;
        }

        await RequireCapabilityChangeAsync(
            "schedule.promotion.automate",
            item.HappyHourPrice != happyHourPrice,
            "Happy-hour pricing",
            cancellationToken).ConfigureAwait(false);
        item.Name = NormalizeName(name);
        item.Description = NormalizeDescription(description);
        item.Price = ValidatePrice(price, nameof(price));
        item.HappyHourPrice = ValidateOptionalPrice(happyHourPrice, nameof(happyHourPrice));
        item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await repository.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);
        await NotifyAsync(venueId, menuId, sectionId, item.Id, "updated", cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<MenuItem?> UpdatePresentationAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        bool isAvailable,
        int? quantityAvailable,
        IReadOnlyCollection<string>? tags,
        bool isPopular,
        CancellationToken cancellationToken = default)
    {
        RequireId(itemId, nameof(itemId));
        if (quantityAvailable < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantityAvailable), "Quantity cannot be negative.");
        }
        var normalizedTags = NormalizeTags(tags);
        if (!await SectionExistsAsync(venueId, menuId, sectionId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var items = await repository.GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false);
        var item = items.SingleOrDefault(existing => existing.Id == itemId);
        if (item is null)
        {
            return null;
        }

        await RequireCapabilityChangeAsync(
            "content.item.dietary_information_manage",
            !string.Equals(item.Tags, normalizedTags, StringComparison.Ordinal),
            "Dietary and allergen badges",
            cancellationToken).ConfigureAwait(false);
        var availabilityChanged = item.IsAvailable != isAvailable;
        item.IsAvailable = isAvailable;
        item.QuantityAvailable = quantityAvailable;
        item.Tags = normalizedTags;
        item.IsPopular = isPopular;
        item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await repository.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);

        if (availabilityChanged)
        {
            await notifier.NotifyVenueItemAvailabilityChangedAsync(
                venueId,
                item.Id.ToString(),
                item.IsAvailable,
                cancellationToken).ConfigureAwait(false);
        }
        await NotifyAsync(venueId, menuId, sectionId, item.Id, "presentation-updated", cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<MenuItem?> SetActiveAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        RequireId(itemId, nameof(itemId));
        if (!await SectionExistsAsync(venueId, menuId, sectionId, cancellationToken).ConfigureAwait(false))
        {
            return null;
        }

        var items = await repository.GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false);
        var item = items.SingleOrDefault(existing => existing.Id == itemId);
        if (item is null)
        {
            return null;
        }

        item.IsActive = isActive;
        item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await repository.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);
        await NotifyAsync(venueId, menuId, sectionId, item.Id, isActive ? "restored" : "archived", cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<int> ReorderAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        if (itemIds.Count != itemIds.Distinct().Count() || itemIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException("Item order cannot contain empty or duplicate identifiers.", nameof(itemIds));
        }
        if (!await SectionExistsAsync(venueId, menuId, sectionId, cancellationToken).ConfigureAwait(false))
        {
            throw new KeyNotFoundException("Menu section does not exist for this venue menu.");
        }
        var existing = await repository.GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false);
        if (!existing.Select(item => item.Id).Order().SequenceEqual(itemIds.Order()))
        {
            throw new ArgumentException("Item order must contain every venue menu item exactly once.", nameof(itemIds));
        }

        var changed = await repository.ReorderItemsAsync(
            venueId,
            sectionId,
            itemIds,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);
        await notifier.NotifyVenueContentUpdatedAsync(venueId, new { change = "items-reordered", menuId, sectionId }, cancellationToken).ConfigureAwait(false);
        return changed;
    }

    private async Task RequireSectionAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        if (!await SectionExistsAsync(venueId, menuId, sectionId, cancellationToken).ConfigureAwait(false))
        {
            throw new KeyNotFoundException("Menu section does not exist for this venue menu.");
        }
    }

    private async Task<bool> SectionExistsAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        CancellationToken cancellationToken)
    {
        RequireId(venueId, nameof(venueId));
        RequireId(menuId, nameof(menuId));
        RequireId(sectionId, nameof(sectionId));
        var menus = await repository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (menus.All(menu => menu.Id != menuId))
        {
            return false;
        }

        var sections = await repository.GetSectionsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        return sections.Any(section => section.Id == sectionId);
    }

    private Task NotifyAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        string change,
        CancellationToken cancellationToken) =>
        notifier.NotifyVenueContentUpdatedAsync(
            venueId,
            new { change, menuId, sectionId, itemId },
            cancellationToken);

    private async Task RequireCapabilityChangeAsync(
        string capabilityId,
        bool isChanging,
        string label,
        CancellationToken cancellationToken)
    {
        if (isChanging)
        {
            try
            {
                await capabilityAuthorizer.RequireAllowedAsync(
                    CapabilityId.Parse(capabilityId),
                    Guid.NewGuid().ToString("N"),
                    "en-US",
                    cancellationToken).ConfigureAwait(false);
            }
            catch (CapabilityDecisionDeniedException exception)
            {
                throw new ArgumentException($"{label} is unavailable: {exception.Decision.ReasonCode}.", exception);
            }
        }
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name.Trim();
        if (normalized.Length > 160)
        {
            throw new ArgumentException("Item name cannot exceed 160 characters.", nameof(name));
        }
        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalized?.Length > 1000)
        {
            throw new ArgumentException("Item description cannot exceed 1000 characters.", nameof(description));
        }
        return normalized;
    }

    private static decimal ValidatePrice(decimal price, string parameterName)
    {
        if (price is < 0 or > 999999.99m)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Price must be between 0 and 999999.99.");
        }
        return decimal.Round(price, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal? ValidateOptionalPrice(decimal? price, string parameterName) =>
        price is null ? null : ValidatePrice(price.Value, parameterName);

    private static string? NormalizeTags(IReadOnlyCollection<string>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return null;
        }
        var normalized = tags
            .Select(tag => tag?.Trim() ?? string.Empty)
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (normalized.Length > 12 || normalized.Any(tag => tag.Length > 40))
        {
            throw new ArgumentException("Use at most 12 tags of 40 characters each.", nameof(tags));
        }
        var value = string.Join(',', normalized);
        if (value.Length > 500)
        {
            throw new ArgumentException("Combined tags cannot exceed 500 characters.", nameof(tags));
        }
        return value.Length == 0 ? null : value;
    }

    private static void RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier cannot be empty.", parameterName);
        }
    }
}
