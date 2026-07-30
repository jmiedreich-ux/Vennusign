using Vennu.Data.Services;

namespace Vennu.Api.Contracts.Admin;

public sealed record MealPeriodWriteRequest(
    string Name,
    TimeSpan StartLocalTime,
    TimeSpan EndLocalTime,
    int ActiveDaysMask,
    bool IsEnabled,
    string? TargetLayout,
    string? MenuFilter,
    string? ThemePresetKey);

public sealed record MealPeriodAdministrationResponse(
    IReadOnlyCollection<Vennu.Core.Models.MealPeriod> MealPeriods,
    IReadOnlyCollection<MealPeriodConflict> Conflicts);
