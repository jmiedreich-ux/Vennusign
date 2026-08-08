using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IMenuRepository
{
    Task<Guid> CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default);

    Task<Guid> CreateSectionAsync(MenuSection section, CancellationToken cancellationToken = default);

    Task<Guid> CreateItemAsync(MenuItem item, CancellationToken cancellationToken = default);

    Task<bool> UpdateSectionAsync(MenuSection section, CancellationToken cancellationToken = default);

    Task<bool> UpdateItemAsync(MenuItem item, CancellationToken cancellationToken = default);

    Task<bool> UpdateMenuAsync(Menu menu, CancellationToken cancellationToken = default);

    Task<int> ReorderSectionsAsync(
        Guid venueId,
        Guid menuId,
        IReadOnlyCollection<Guid> sectionIds,
        DateTime updatedUtc,
        CancellationToken cancellationToken = default);

    Task<int> ReorderItemsAsync(
        Guid venueId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        DateTime updatedUtc,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    Task<IReadOnlyCollection<Menu>> GetMenusAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuSection>> GetSectionsAsync(Guid venueId, Guid menuId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MenuItem>> GetItemsAsync(Guid venueId, Guid sectionId, CancellationToken cancellationToken = default);

    async Task<IReadOnlyCollection<MenuItem>> GetActiveItemsAsync(
        Guid venueId,
        Guid sectionId,
        CancellationToken cancellationToken = default) =>
        (await GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false))
            .Where(item => item.IsActive)
            .ToArray();

}
