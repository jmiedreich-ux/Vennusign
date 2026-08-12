using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class MenuSectionManagementService(
    IMenuRepository repository,
    IContentRepository libraryRepository,
    ICapabilityDecisionService decisions,
    TimeProvider timeProvider) : IMenuSectionManagementService
{
    public async Task<MenuEditorSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        var menus = await repository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        var sections = await Task.WhenAll(menus.Select(async menu => new MenuEditorMenu(
            menu,
            await repository.GetSectionsAsync(venueId, menu.Id, cancellationToken).ConfigureAwait(false))));

        // The editor reads the same placements a publish snapshots. It keeps the
        // legacy item shape until milestone 3 replaces this surface, so a price
        // that is not a number ("MP") renders as 0 here while the board and the
        // library keep it exactly as typed.
        var placed = await libraryRepository.GetPlacedItemsForVenueAsync(venueId, cancellationToken).ConfigureAwait(false);
        var placedBySection = placed
            .GroupBy(item => item.MenuSectionId)
            .ToDictionary(group => group.Key, group => group.Select(ToEditorItem).ToArray());
        var itemGroups = sections
            .SelectMany(menu => menu.Sections)
            .Select(section => new MenuEditorItemGroup(
                section.Id,
                placedBySection.TryGetValue(section.Id, out var items) ? items : []))
            .ToArray();
        var capabilityResults = await decisions.EvaluateBatchAsync(
            [
                CapabilityId.Parse("schedule.promotion.automate"),
                CapabilityId.Parse("content.item.dietary_information_manage"),
                CapabilityId.Parse("content.item.availability_update")
            ],
            Guid.NewGuid().ToString("N"),
            "en-US",
            cancellationToken).ConfigureAwait(false);
        var allowed = capabilityResults.Where(result => result.IsAllowed).Select(result => result.Capability.Value).ToHashSet(StringComparer.Ordinal);
        var happyHour = allowed.Contains("schedule.promotion.automate");
        var allergenBadges = allowed.Contains("content.item.dietary_information_manage");
        var quickUpdate = allowed.Contains("content.item.availability_update");
        return new MenuEditorSnapshot(sections, itemGroups, new MenuEditorCapabilities(happyHour, allergenBadges, quickUpdate));
    }

    public Task<Menu> CreateMenuAsync(
        Guid venueId,
        string name,
        CancellationToken cancellationToken = default) =>
        CreateMenuAsync(venueId, name, null, cancellationToken);

    public async Task<Menu> CreateMenuAsync(
        Guid venueId,
        string name,
        string? theme,
        CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        var normalizedName = NormalizeMenuName(name);
        var menus = await repository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (menus.Any(menu => string.Equals(menu.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("A menu with that name already exists.", nameof(name));
        }

        var ceilings = await libraryRepository
            .GetResolvedCeilingsAsync(venueId, cancellationToken)
            .ConfigureAwait(false);
        var menuLimit = ceilings.TryGetValue(MenuCeilings.MenusPerVenue, out var configured) ? configured : int.MaxValue;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var menu = new Menu
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Name = normalizedName,
            Theme = string.IsNullOrWhiteSpace(theme) ? null : theme.Trim(),
            IsActive = true,
            CreatedUtc = now,
            UpdatedUtc = now
        };

        // The ceiling is enforced under the same lock as the insert, so two
        // requests arriving at limit-minus-one cannot both get in (Q201). The
        // refusal is a plain sentence, never a quiet failure.
        var outcome = await libraryRepository
            .CreateMenuWithinCeilingAsync(menu, menuLimit, cancellationToken)
            .ConfigureAwait(false);
        if (!outcome.Created)
        {
            throw new ArgumentException(
                MenuCeilings.DescribeRefusal(MenuCeilings.MenusPerVenue, outcome.ActiveMenuCount + 1, menuLimit),
                nameof(name));
        }

        return menu;
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

    private static MenuItem ToEditorItem(PlacedMenuItem placed) => new()
    {
        Id = placed.ItemId,
        MenuSectionId = placed.MenuSectionId,
        Name = placed.Name,
        Description = placed.Description,
        Price = decimal.TryParse(
            placed.Price,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var price) ? price : 0m,
        IsAvailable = placed.IsAvailable,
        IsActive = placed.IsActive,
        SortOrder = placed.SortOrder,
        CreatedUtc = placed.CreatedUtc,
        UpdatedUtc = placed.UpdatedUtc
    };

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

    private static string NormalizeMenuName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var normalized = name.Trim();
        if (normalized.Length > 200)
        {
            throw new ArgumentException("Menu name cannot exceed 200 characters.", nameof(name));
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
