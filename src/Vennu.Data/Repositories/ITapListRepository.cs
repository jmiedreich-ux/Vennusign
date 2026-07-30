using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ITapListRepository
{
    Task<IReadOnlyCollection<TapCategory>> GetCategoriesAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TapItem>> GetItemsAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<Guid> CreateCategoryAsync(TapCategory category, CancellationToken cancellationToken = default);
    Task<Guid> CreateItemAsync(TapItem item, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoryAsync(TapCategory category, CancellationToken cancellationToken = default);
    Task<bool> UpdateItemAsync(TapItem item, CancellationToken cancellationToken = default);
}
