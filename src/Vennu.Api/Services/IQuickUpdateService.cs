using Vennu.Core.Models;

namespace Vennu.Api.Services;

public interface IQuickUpdateService
{
    Task<Menu?> UpdateDailySpecialAsync(
        Guid venueId,
        Guid menuId,
        string? dailySpecial,
        CancellationToken cancellationToken = default);

    Task<MenuItem?> SetAvailabilityAsync(
        Guid venueId,
        Guid menuId,
        Guid sectionId,
        Guid itemId,
        bool isAvailable,
        CancellationToken cancellationToken = default);
}
