using System.Globalization;
using Vennu.Api.Notifications;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Services;

public sealed class MenuItemManagementService(
    IMenuLibraryRepository library,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider) : IMenuItemManagementService
{
    public async Task<MenuItem> CreateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        string? description,
        decimal price,
        CancellationToken cancellationToken = default)
    {
        var ceilings = await library.GetResolvedCeilingsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var itemLimit = ceilings.TryGetValue(MenuCeilings.ItemsPerMenu, out var configured) ? configured : int.MaxValue;

        var item = new Item
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Name = NormalizeName(name),
            Description = NormalizeDescription(description),
            Price = NormalizePrice(price),
            Source = ItemSources.Manual
        };

        var outcome = await library
            .CreateItemOnMenuAsync(item, menuId, sectionId, itemLimit, cancellationToken)
            .ConfigureAwait(false);

        if (outcome.Outcome == ItemPlacementOutcomes.SectionMissing)
        {
            throw new KeyNotFoundException("Menu section does not exist for this venue menu.");
        }

        if (outcome.Outcome == ItemPlacementOutcomes.OverCeiling)
        {
            throw new ArgumentException(
                MenuCeilings.DescribeRefusal(MenuCeilings.ItemsPerMenu, outcome.ItemCountOnMenu + 1, itemLimit),
                nameof(name));
        }

        await NotifyAsync(venueId, menuId, sectionId, item.Id, "created", cancellationToken).ConfigureAwait(false);
        return ToEditorItem(item, sectionId, outcome.SortOrder);
    }

    public async Task<MenuItem?> UpdateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        string name,
        string? description,
        decimal price,
        CancellationToken cancellationToken = default)
    {
        RequireId(itemId, nameof(itemId));
        var placement = (await library.GetPlacementsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false))
            .FirstOrDefault(candidate => candidate.MenuSectionId == sectionId && candidate.ItemId == itemId);
        if (placement is null)
        {
            return null;
        }

        var item = await library.GetItemAsync(venueId, itemId, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            return null;
        }

        item.Name = NormalizeName(name);
        item.Description = NormalizeDescription(description);
        item.Price = NormalizePrice(price);
        item.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await library.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);
        await NotifyAsync(venueId, menuId, sectionId, item.Id, "updated", cancellationToken).ConfigureAwait(false);
        return ToEditorItem(item, sectionId, placement.SortOrder);
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

        var existing = (await library.GetPlacementsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false))
            .Where(placement => placement.MenuSectionId == sectionId)
            .Select(placement => placement.ItemId)
            .ToArray();
        if (existing.Length == 0)
        {
            throw new KeyNotFoundException("Menu section does not exist for this venue menu.");
        }

        if (!existing.Order().SequenceEqual(itemIds.Order()))
        {
            throw new ArgumentException("Item order must contain every placed item exactly once.", nameof(itemIds));
        }

        var changed = await library.ReorderPlacementsAsync(
            venueId,
            menuId,
            sectionId,
            itemIds,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);
        await notifier.NotifyVenueContentUpdatedAsync(
            venueId,
            new { change = "items-reordered", menuId, sectionId },
            cancellationToken).ConfigureAwait(false);
        return changed;
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

    // The editor's number input cannot type "MP", so a decimal is faithful here.
    // Stored as text so what it shows is what the board renders (Q115/Q190).
    private static string NormalizePrice(decimal price)
    {
        if (price is < 0 or > 999999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be between 0 and 999999.99.");
        }

        return decimal.Round(price, 2, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
    }

    private static MenuItem ToEditorItem(Item item, Guid sectionId, int sortOrder) => new()
    {
        Id = item.Id,
        VenueId = item.VenueId,
        MenuSectionId = sectionId,
        Name = item.Name,
        Description = item.Description,
        Price = decimal.TryParse(item.Price, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ? price : 0m,
        IsAvailable = true,
        IsActive = item.IsActive,
        SortOrder = sortOrder,
        CreatedUtc = item.CreatedUtc,
        UpdatedUtc = item.UpdatedUtc
    };

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name.Trim();
        if (normalized.Length > Item.NameMaxLength)
        {
            throw new ArgumentException($"Item name cannot exceed {Item.NameMaxLength} characters.", nameof(name));
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? description)
    {
        var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalized?.Length > Item.DescriptionMaxLength)
        {
            throw new ArgumentException($"Item description cannot exceed {Item.DescriptionMaxLength} characters.", nameof(description));
        }

        return normalized;
    }

    private static void RequireId(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Identifier cannot be empty.", parameterName);
        }
    }
}
