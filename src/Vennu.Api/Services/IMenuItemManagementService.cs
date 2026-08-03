using Vennu.Core.Models;

namespace Vennu.Api.Services;

public interface IMenuItemManagementService
{
    Task<MenuItem> CreateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        string name,
        string? description,
        decimal price,
        decimal? happyHourPrice,
        CancellationToken cancellationToken = default);

    Task<MenuItem?> UpdateAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        string name,
        string? description,
        decimal price,
        decimal? happyHourPrice,
        CancellationToken cancellationToken = default);

    Task<MenuItem?> UpdatePresentationAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        bool isAvailable,
        int? quantityAvailable,
        IReadOnlyCollection<string>? tags,
        bool isPopular,
        CancellationToken cancellationToken = default);

    Task<MenuItem?> SetActiveAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<int> ReorderAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken = default);
}
