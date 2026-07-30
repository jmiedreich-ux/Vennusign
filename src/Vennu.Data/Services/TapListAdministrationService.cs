using System.Text.RegularExpressions;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed partial class TapListAdministrationService(
    ITapListRepository repository,
    IVenueRepository venues,
    TimeProvider timeProvider) : ITapListAdministrationService
{
    public async Task<TapListSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        Require(venueId, nameof(venueId));
        return new(
            await repository.GetCategoriesAsync(venueId, cancellationToken).ConfigureAwait(false),
            await repository.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false));
    }

    public async Task<TapCategory> CreateCategoryAsync(
        Guid venueId, TapCategory value, CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var existing = await repository.GetCategoriesAsync(venueId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var category = Normalize(value, venueId, Guid.NewGuid(), existing.Count, now, now);
        await repository.CreateCategoryAsync(category, cancellationToken).ConfigureAwait(false);
        return category;
    }

    public async Task<TapCategory?> UpdateCategoryAsync(
        Guid venueId, Guid categoryId, TapCategory value, CancellationToken cancellationToken = default)
    {
        var existing = (await repository.GetCategoriesAsync(Require(venueId, nameof(venueId)), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(row => row.Id == Require(categoryId, nameof(categoryId)));
        if (existing is null) return null;
        var category = Normalize(value, venueId, existing.Id, existing.SortOrder, existing.CreatedUtc, timeProvider.GetUtcNow().UtcDateTime);
        await repository.UpdateCategoryAsync(category, cancellationToken).ConfigureAwait(false);
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(
        Guid venueId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetItemsAsync(Require(venueId, nameof(venueId)), cancellationToken).ConfigureAwait(false);
        if (items.Any(item => item.TapCategoryId == Require(categoryId, nameof(categoryId))))
            throw new InvalidOperationException("Move or delete category items before deleting the category.");
        return await repository.DeleteCategoryAsync(venueId, categoryId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TapItem> CreateItemAsync(
        Guid venueId, TapItem value, CancellationToken cancellationToken = default)
    {
        await RequireVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        await RequireCategoryAsync(venueId, value.TapCategoryId, cancellationToken).ConfigureAwait(false);
        var existing = await repository.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var item = Normalize(value, venueId, Guid.NewGuid(), existing.Count, now, now);
        await repository.CreateItemAsync(item, cancellationToken).ConfigureAwait(false);
        return item;
    }

    public async Task<TapItem?> UpdateItemAsync(
        Guid venueId, Guid itemId, TapItem value, CancellationToken cancellationToken = default)
    {
        var existing = (await repository.GetItemsAsync(Require(venueId, nameof(venueId)), cancellationToken).ConfigureAwait(false))
            .SingleOrDefault(row => row.Id == Require(itemId, nameof(itemId)));
        if (existing is null) return null;
        await RequireCategoryAsync(venueId, value.TapCategoryId, cancellationToken).ConfigureAwait(false);
        var item = Normalize(value, venueId, existing.Id, existing.SortOrder, existing.CreatedUtc, timeProvider.GetUtcNow().UtcDateTime);
        await repository.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);
        return item;
    }

    public Task<bool> DeleteItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default) =>
        repository.DeleteItemAsync(Require(venueId, nameof(venueId)), Require(itemId, nameof(itemId)), cancellationToken);

    public async Task ReorderCategoriesAsync(Guid venueId, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        await ValidateOrderAsync(venueId, ids, true, cancellationToken).ConfigureAwait(false);
        await repository.ReorderCategoriesAsync(venueId, ids, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
    }

    public async Task ReorderItemsAsync(Guid venueId, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        await ValidateOrderAsync(venueId, ids, false, cancellationToken).ConfigureAwait(false);
        await repository.ReorderItemsAsync(venueId, ids, timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
    }

    private async Task ValidateOrderAsync(Guid venueId, IReadOnlyCollection<Guid> ids, bool categories, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(ids);
        Require(venueId, nameof(venueId));
        if (ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count) throw new ArgumentException("Order must contain unique non-empty identifiers.", nameof(ids));
        var expected = categories
            ? (await repository.GetCategoriesAsync(venueId, cancellationToken).ConfigureAwait(false)).Select(row => row.Id)
            : (await repository.GetItemsAsync(venueId, cancellationToken).ConfigureAwait(false)).Select(row => row.Id);
        if (!expected.Order().SequenceEqual(ids.Order())) throw new ArgumentException("Order must contain every venue row exactly once.", nameof(ids));
    }

    private async Task RequireVenueAsync(Guid venueId, CancellationToken cancellationToken)
    {
        if (await venues.GetByIdAsync(Require(venueId, nameof(venueId)), cancellationToken).ConfigureAwait(false) is null)
            throw new KeyNotFoundException("Venue was not found.");
    }

    private async Task RequireCategoryAsync(Guid venueId, Guid? categoryId, CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue) return;
        if (!(await repository.GetCategoriesAsync(venueId, cancellationToken).ConfigureAwait(false)).Any(row => row.Id == categoryId))
            throw new ArgumentException("Tap category does not belong to the venue.", nameof(categoryId));
    }

    private static TapCategory Normalize(TapCategory value, Guid venueId, Guid id, int sortOrder, DateTime createdUtc, DateTime updatedUtc)
    {
        ArgumentNullException.ThrowIfNull(value);
        var name = Text(value.Name, 120, true)!;
        if (value.CategoryPrice < 0) throw new ArgumentOutOfRangeException(nameof(value.CategoryPrice));
        return new() { Id = id, VenueId = venueId, Name = name, CategoryPrice = value.CategoryPrice, SortOrder = sortOrder, IsActive = value.IsActive, CreatedUtc = createdUtc, UpdatedUtc = updatedUtc };
    }

    private static TapItem Normalize(TapItem value, Guid venueId, Guid id, int sortOrder, DateTime createdUtc, DateTime updatedUtc)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Price < 0) throw new ArgumentOutOfRangeException(nameof(value.Price));
        if (value.Abv is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(value.Abv));
        if (value.Ibu is < 0 or > 1000) throw new ArgumentOutOfRangeException(nameof(value.Ibu));
        return new()
        {
            Id = id, VenueId = venueId, TapCategoryId = value.TapCategoryId,
            Name = Text(value.Name, 200, true)!, Style = Text(value.Style, 160),
            Abv = value.Abv, Ibu = value.Ibu, Description = Text(value.Description, 1000),
            Price = value.Price, GlassColor = Color(value.GlassColor), NameColor = Color(value.NameColor),
            IsAvailable = value.IsAvailable, IsComingSoon = value.IsComingSoon, SortOrder = sortOrder,
            CreatedUtc = createdUtc, UpdatedUtc = updatedUtc
        };
    }

    private static string? Text(string? value, int max, bool required = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return required ? throw new ArgumentException("Value is required.") : null;
        var normalized = value.Trim();
        return normalized.Length <= max ? normalized : throw new ArgumentException($"Value cannot exceed {max} characters.");
    }

    private static string? Color(string? value)
    {
        var normalized = Text(value, 7);
        return normalized is null || HexColor().IsMatch(normalized)
            ? normalized?.ToUpperInvariant()
            : throw new ArgumentException("Color must be a six-digit hex value.");
    }

    private static Guid Require(Guid value, string parameterName) =>
        value == Guid.Empty ? throw new ArgumentException("Identifier cannot be empty.", parameterName) : value;

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexColor();
}
