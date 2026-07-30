using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IMealPeriodRepository
{
    Task<Guid> CreateAsync(MealPeriod mealPeriod, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(MealPeriod mealPeriod, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MealPeriod>> GetByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);
}
