using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IMealPeriodScheduleResolver
{
    MealPeriodScheduleResolution Resolve(
        string timezoneId,
        DateTimeOffset utcNow,
        IReadOnlyCollection<MealPeriod> mealPeriods);
}

public sealed record MealPeriodScheduleResolution(
    DateTimeOffset LocalNow,
    MealPeriod? ActiveMealPeriod,
    MealPeriod? NextMealPeriod,
    DateTimeOffset? NextStartsLocal);
