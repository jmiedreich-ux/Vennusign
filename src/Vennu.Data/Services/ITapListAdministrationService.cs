using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface ITapListAdministrationService
{
    Task<TapListSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<TapCategory> CreateCategoryAsync(Guid venueId, TapCategory value, CancellationToken cancellationToken = default);
    Task<TapCategory?> UpdateCategoryAsync(Guid venueId, Guid categoryId, TapCategory value, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(Guid venueId, Guid categoryId, CancellationToken cancellationToken = default);
    Task<TapItem> CreateItemAsync(Guid venueId, TapItem value, CancellationToken cancellationToken = default);
    Task<TapItem?> UpdateItemAsync(Guid venueId, Guid itemId, TapItem value, CancellationToken cancellationToken = default);
    Task<bool> DeleteItemAsync(Guid venueId, Guid itemId, CancellationToken cancellationToken = default);
    Task ReorderCategoriesAsync(Guid venueId, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task ReorderItemsAsync(Guid venueId, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}

public sealed record TapListSnapshot(
    IReadOnlyCollection<TapCategory> Categories,
    IReadOnlyCollection<TapItem> Items);
