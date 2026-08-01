using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class PosCatalogImportService(
    IEnumerable<IPosProvider> providers,
    IPosConnectionRepository connectionRepository,
    IPosCatalogMappingRepository mappingRepository,
    IMenuRepository menuRepository,
    IPosCredentialProtector credentialProtector,
    TimeProvider timeProvider) : IPosCatalogImportService
{
    private const string CatalogMenuExternalId = "catalog-root";

    public async Task<PosCatalogImportResult> ImportAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("A non-empty venue identifier is required.", nameof(venueId));
        var provider = providers.SingleOrDefault(value => value.Provider == PosProvider.Square)
            ?? throw new InvalidOperationException("The Square catalog provider is unavailable.");
        var connection = await connectionRepository.GetAsync(venueId, PosProvider.Square, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Connect Square before importing its catalog.");
        if (connection.Status != PosConnectionStatus.Connected)
            throw new InvalidOperationException("The Square connection is not ready for catalog import.");

        var context = new PosProviderContext(
            venueId,
            connection.ExternalMerchantId,
            credentialProtector.Unprotect(connection.ProtectedAccessToken));
        var catalog = await provider.GetCatalogAsync(context, cancellationToken).ConfigureAwait(false);
        var conflicts = Validate(catalog).ToList();
        var mappings = (await mappingRepository.GetAllAsync(venueId, PosProvider.Square, cancellationToken).ConfigureAwait(false))
            .ToDictionary(value => (value.EntityType, value.ExternalId), value => value);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var menu = await ResolveMenuAsync(venueId, mappings, now, cancellationToken).ConfigureAwait(false);
        var sections = (await menuRepository.GetSectionsAsync(venueId, menu.Id, cancellationToken).ConfigureAwait(false))
            .ToDictionary(value => value.Id);
        var categoriesCreated = 0;
        var categoriesUpdated = 0;
        var itemsCreated = 0;
        var itemsUpdated = 0;
        var modifiersMapped = 0;
        var categoryIds = new Dictionary<string, Guid>(StringComparer.Ordinal);

        foreach (var category in catalog.Categories.OrderBy(value => value.SortOrder).ThenBy(value => value.ExternalId, StringComparer.Ordinal))
        {
            if (!ValidId(category.ExternalId) || string.IsNullOrWhiteSpace(category.Name)) continue;
            var key = (PosCatalogEntityType.Category, category.ExternalId.Trim());
            if (mappings.TryGetValue(key, out var mapping))
            {
                if (!sections.TryGetValue(mapping.LocalEntityId, out var section))
                {
                    conflicts.Add($"Category {category.ExternalId} maps to a missing venue section.");
                    continue;
                }
                section.Name = Truncate(category.Name, 200);
                section.UpdatedUtc = now;
                await menuRepository.UpdateSectionAsync(section, cancellationToken).ConfigureAwait(false);
                categoryIds[category.ExternalId.Trim()] = section.Id;
                categoriesUpdated++;
                continue;
            }

            var created = new MenuSection
            {
                VenueId = venueId,
                MenuId = menu.Id,
                Name = Truncate(category.Name, 200),
                SortOrder = sections.Count,
                IsActive = true
            };
            created.Id = await menuRepository.CreateSectionAsync(created, cancellationToken).ConfigureAwait(false);
            await SaveMappingAsync(venueId, PosCatalogEntityType.Category, category.ExternalId, created.Id, cancellationToken).ConfigureAwait(false);
            mappings[key] = new PosCatalogMapping { VenueId = venueId, Provider = PosProvider.Square, EntityType = key.Item1, ExternalId = key.Item2, LocalEntityId = created.Id };
            categoryIds[category.ExternalId.Trim()] = created.Id;
            sections[created.Id] = created;
            categoriesCreated++;
        }

        var itemsBySection = new Dictionary<Guid, Dictionary<Guid, MenuItem>>();
        foreach (var item in catalog.Items.OrderBy(value => value.ExternalId, StringComparer.Ordinal))
        {
            if (!ValidId(item.ExternalId) || !ValidId(item.ExternalCategoryId) || string.IsNullOrWhiteSpace(item.Name)) continue;
            if (!categoryIds.TryGetValue(item.ExternalCategoryId.Trim(), out var sectionId))
            {
                conflicts.Add($"Item {item.ExternalId} references unavailable category {item.ExternalCategoryId}.");
                continue;
            }
            if (!string.Equals(item.CurrencyCode, "USD", StringComparison.OrdinalIgnoreCase) || item.Price < 0)
            {
                conflicts.Add($"Item {item.ExternalId} has unsupported price data.");
                continue;
            }
            if (!itemsBySection.TryGetValue(sectionId, out var localItems))
            {
                localItems = (await menuRepository.GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false))
                    .ToDictionary(value => value.Id);
                itemsBySection[sectionId] = localItems;
            }
            var key = (PosCatalogEntityType.Item, item.ExternalId.Trim());
            MenuItem localItem;
            if (mappings.TryGetValue(key, out var mapping))
            {
                if (!localItems.TryGetValue(mapping.LocalEntityId, out var existingItem))
                {
                    conflicts.Add($"Item {item.ExternalId} maps to a missing venue item.");
                    continue;
                }
                localItem = existingItem;
                if (localItem.MenuSectionId != sectionId)
                {
                    conflicts.Add($"Item {item.ExternalId} changed category; move it manually before re-importing.");
                    continue;
                }
                Apply(localItem, item, now);
                await menuRepository.UpdateItemAsync(localItem, cancellationToken).ConfigureAwait(false);
                itemsUpdated++;
            }
            else
            {
                localItem = new MenuItem { VenueId = venueId, MenuSectionId = sectionId, SortOrder = localItems.Count, IsAvailable = true };
                Apply(localItem, item, now);
                localItem.Id = await menuRepository.CreateItemAsync(localItem, cancellationToken).ConfigureAwait(false);
                await SaveMappingAsync(venueId, PosCatalogEntityType.Item, item.ExternalId, localItem.Id, cancellationToken).ConfigureAwait(false);
                mappings[key] = new PosCatalogMapping { VenueId = venueId, Provider = PosProvider.Square, EntityType = key.Item1, ExternalId = key.Item2, LocalEntityId = localItem.Id };
                localItems[localItem.Id] = localItem;
                itemsCreated++;
            }

            foreach (var modifier in item.Modifiers.Where(value => ValidId(value.ExternalId)))
            {
                var modifierExternalId = $"{item.ExternalId.Trim()}:{modifier.ExternalId.Trim()}";
                var modifierKey = (PosCatalogEntityType.Modifier, modifierExternalId);
                if (!mappings.ContainsKey(modifierKey))
                {
                    await SaveMappingAsync(venueId, PosCatalogEntityType.Modifier, modifierExternalId, localItem.Id, cancellationToken).ConfigureAwait(false);
                    mappings[modifierKey] = new PosCatalogMapping { VenueId = venueId, Provider = PosProvider.Square, EntityType = modifierKey.Item1, ExternalId = modifierKey.Item2, LocalEntityId = localItem.Id };
                }
                modifiersMapped++;
            }
        }

        connection.LastSyncedUtc = now;
        await connectionRepository.SaveAsync(venueId, connection, cancellationToken).ConfigureAwait(false);
        return new PosCatalogImportResult(
            conflicts.Count == 0 ? "completed" : "completed_with_conflicts",
            categoriesCreated, categoriesUpdated, itemsCreated, itemsUpdated, modifiersMapped,
            conflicts.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray(), now);
    }

    private async Task<Menu> ResolveMenuAsync(Guid venueId, Dictionary<(PosCatalogEntityType, string), PosCatalogMapping> mappings, DateTime now, CancellationToken cancellationToken)
    {
        var menus = await menuRepository.GetMenusAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (mappings.TryGetValue((PosCatalogEntityType.Menu, CatalogMenuExternalId), out var mapping))
            return menus.SingleOrDefault(value => value.Id == mapping.LocalEntityId)
                ?? throw new InvalidOperationException("The Square catalog maps to a missing venue menu.");
        var menu = new Menu { VenueId = venueId, Name = "Square Catalog", IsActive = true, UpdatedUtc = now };
        menu.Id = await menuRepository.CreateMenuAsync(menu, cancellationToken).ConfigureAwait(false);
        await SaveMappingAsync(venueId, PosCatalogEntityType.Menu, CatalogMenuExternalId, menu.Id, cancellationToken).ConfigureAwait(false);
        return menu;
    }

    private Task<PosCatalogMapping> SaveMappingAsync(Guid venueId, PosCatalogEntityType type, string externalId, Guid localId, CancellationToken cancellationToken) =>
        mappingRepository.SaveAsync(venueId, new PosCatalogMapping
        {
            VenueId = venueId,
            Provider = PosProvider.Square,
            EntityType = type,
            ExternalId = externalId.Trim(),
            LocalEntityId = localId
        }, cancellationToken);

    private static IEnumerable<string> Validate(PosCatalogResult catalog)
    {
        foreach (var group in catalog.Categories.Where(value => ValidId(value.ExternalId)).GroupBy(value => value.ExternalId.Trim(), StringComparer.Ordinal).Where(value => value.Count() > 1))
            yield return $"Duplicate category identifier {group.Key}.";
        foreach (var group in catalog.Items.Where(value => ValidId(value.ExternalId)).GroupBy(value => value.ExternalId.Trim(), StringComparer.Ordinal).Where(value => value.Count() > 1))
            yield return $"Duplicate item identifier {group.Key}.";
        if (catalog.ContinuationToken is not null) yield return "The provider returned an incomplete paged catalog.";
    }

    private static bool ValidId(string? value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 300;

    private static void Apply(MenuItem target, PosCatalogItem source, DateTime now)
    {
        target.Name = Truncate(source.Name, 200);
        target.Description = string.IsNullOrWhiteSpace(source.Description) ? null : Truncate(source.Description, 1000);
        target.Price = source.Price;
        var tags = source.Modifiers.Count == 0 ? null : string.Join(", ", source.Modifiers.Select(value => value.Name.Trim()).Where(value => value.Length > 0).Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal));
        target.Tags = tags is { Length: > 500 } ? tags[..500] : tags;
        target.UpdatedUtc = now;
    }

    private static string Truncate(string value, int maximumLength)
    {
        var normalized = value.Trim();
        return normalized[..Math.Min(normalized.Length, maximumLength)];
    }
}
