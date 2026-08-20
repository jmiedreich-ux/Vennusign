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

    /// <summary>
    /// Items placed on a section of a board, in placement order.
    ///
    /// Defaulted so the many fakes that never read a board do not have to implement
    /// it; the SQL repository overrides it, and anything that reaches this default
    /// is asking a fake for content it was never given.
    /// </summary>
    Task<IReadOnlyCollection<MenuItem>> GetBoardItemsAsync(
        Guid venueId,
        Guid sectionId,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("This repository does not serve board content.");

    /// <summary>Sections belonging to one page, for a screen assigned that page.</summary>
    Task<IReadOnlyCollection<MenuSection>> GetSectionsForPageAsync(
        Guid venueId,
        Guid pageId,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("This repository does not serve board content.");

    /// <summary>Board items that are active, which is what a screen may show.</summary>
    async Task<IReadOnlyCollection<MenuItem>> GetActiveBoardItemsAsync(
        Guid venueId,
        Guid sectionId,
        CancellationToken cancellationToken = default) =>
        (await GetBoardItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false))
            .Where(item => item.IsActive)
            .ToArray();

    async Task<IReadOnlyCollection<MenuItem>> GetActiveItemsAsync(
        Guid venueId,
        Guid sectionId,
        CancellationToken cancellationToken = default) =>
        (await GetItemsAsync(venueId, sectionId, cancellationToken).ConfigureAwait(false))
            .Where(item => item.IsActive)
            .ToArray();

}
