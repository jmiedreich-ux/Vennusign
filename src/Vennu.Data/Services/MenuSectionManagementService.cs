using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class MenuSectionManagementService(
    IMenuRepository repository,
    IFeatureResolutionService featureResolution,
    TimeProvider timeProvider) : IMenuSectionManagementService
{
    public async Task<MenuEditorSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        var menus = await repository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        var sections = await Task.WhenAll(menus.Select(async menu => new MenuEditorMenu(
            menu,
            await repository.GetSectionsAsync(venueId, menu.Id, cancellationToken).ConfigureAwait(false))));
        var itemGroups = await Task.WhenAll(sections
            .SelectMany(menu => menu.Sections)
            .Select(async section => new MenuEditorItemGroup(
                section.Id,
                await repository.GetItemsAsync(venueId, section.Id, cancellationToken).ConfigureAwait(false))));
        var happyHour = await featureResolution.HasFeatureAsync(venueId, "happy_hour", cancellationToken).ConfigureAwait(false);
        var allergenBadges = await featureResolution.HasFeatureAsync(venueId, "allergen_badges", cancellationToken).ConfigureAwait(false);
        var quickUpdate = await featureResolution.HasFeatureAsync(venueId, "quick_update", cancellationToken).ConfigureAwait(false);
        return new MenuEditorSnapshot(sections, itemGroups, new MenuEditorCapabilities(happyHour, allergenBadges, quickUpdate));
    }

    public async Task<MenuSection> CreateAsync(
        Guid venueId,
        Guid menuId,
        string name,
        CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        RequireId(menuId, nameof(menuId));
        var normalizedName = NormalizeName(name);
        var menus = await repository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (menus.All(menu => menu.Id != menuId))
        {
            throw new KeyNotFoundException("Menu does not exist for this venue.");
        }

        var sections = await repository.GetSectionsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var section = new MenuSection
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            MenuId = menuId,
            Name = normalizedName,
            SortOrder = sections.Count == 0 ? 0 : sections.Max(item => item.SortOrder) + 1,
            IsActive = true,
            CreatedUtc = now,
            UpdatedUtc = now
        };
        await repository.CreateSectionAsync(section, cancellationToken).ConfigureAwait(false);
        return section;
    }

    public async Task<MenuSection?> UpdateAsync(
        Guid venueId,
        Guid sectionId,
        string name,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        RequireId(sectionId, nameof(sectionId));
        var snapshot = await GetAsync(venueId, cancellationToken).ConfigureAwait(false);
        var section = snapshot.Menus.SelectMany(menu => menu.Sections).SingleOrDefault(item => item.Id == sectionId);
        if (section is null)
        {
            return null;
        }

        section.Name = NormalizeName(name);
        section.IsActive = isActive;
        section.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await repository.UpdateSectionAsync(section, cancellationToken).ConfigureAwait(false);
        return section;
    }

    public async Task<int> ReorderAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sectionIds);
        RequireId(venueId, nameof(venueId));
        RequireId(menuId, nameof(menuId));
        if (sectionIds.Count != sectionIds.Distinct().Count() || sectionIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException("Section order cannot contain empty or duplicate identifiers.", nameof(sectionIds));
        }

        var existing = await repository.GetSectionsAsync(venueId, menuId, cancellationToken).ConfigureAwait(false);
        if (!existing.Select(section => section.Id).Order().SequenceEqual(sectionIds.Order()))
        {
            throw new ArgumentException("Section order must contain every venue menu section exactly once.", nameof(sectionIds));
        }

        return await repository.ReorderSectionsAsync(
            venueId,
            menuId,
            sectionIds,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name.Trim();
        if (normalized.Length > 120)
        {
            throw new ArgumentException("Section name cannot exceed 120 characters.", nameof(name));
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
