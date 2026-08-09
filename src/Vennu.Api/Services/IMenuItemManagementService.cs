using Vennu.Core.Models;

namespace Vennu.Api.Services;

/// <summary>
/// The legacy editor's item surface, consolidated onto the venue item library:
/// every write lands in Items/Placements, so an edit made here is part of the
/// working state the derived draft compares and a publish ships. The MenuItem
/// return shape survives only until milestone 3 replaces the editor.
/// </summary>
public interface IMenuItemManagementService
{
    Task<MenuItem> CreateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        string? description,
        decimal price,
        CancellationToken cancellationToken = default);

    Task<MenuItem?> UpdateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        string name,
        string? description,
        decimal price,
        CancellationToken cancellationToken = default);

    Task<int> ReorderAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken = default);
}
