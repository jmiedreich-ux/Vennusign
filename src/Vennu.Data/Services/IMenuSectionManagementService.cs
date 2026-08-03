using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IMenuSectionManagementService
{
    Task<MenuEditorSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<Menu> CreateMenuAsync(Guid venueId, string name, CancellationToken cancellationToken = default);
    Task<MenuSection> CreateAsync(Guid venueId, Guid menuId, string name, CancellationToken cancellationToken = default);
    Task<MenuSection?> UpdateAsync(Guid venueId, Guid sectionId, string name, bool isActive, CancellationToken cancellationToken = default);
    Task<int> ReorderAsync(Guid venueId, Guid menuId, IReadOnlyCollection<Guid> sectionIds, CancellationToken cancellationToken = default);
}
