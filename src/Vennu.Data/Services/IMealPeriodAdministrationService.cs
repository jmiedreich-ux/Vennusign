using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IMealPeriodAdministrationService
{
    Task<MealPeriodAdministrationSnapshot> GetAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task<MealPeriod> CreateAsync(
        Guid venueId,
        string name,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    Task<MealPeriod?> UpdateAsync(
        Guid venueId,
        Guid mealPeriodId,
        string name,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid venueId, Guid mealPeriodId, CancellationToken cancellationToken = default);
}

public sealed record MealPeriodAdministrationSnapshot(
    IReadOnlyCollection<MealPeriod> MealPeriods,
    IReadOnlyCollection<MealPeriodConflict> Conflicts);

public sealed record MealPeriodConflict(
    Guid FirstId,
    string FirstName,
    Guid SecondId,
    string SecondName);
