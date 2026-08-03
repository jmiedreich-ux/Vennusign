using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed class MealPeriodScheduleResolver : IMealPeriodScheduleResolver
{
    public MealPeriodScheduleResolution Resolve(
        string timezoneId,
        DateTimeOffset utcNow,
        IReadOnlyCollection<MealPeriod> mealPeriods)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timezoneId);
        ArgumentNullException.ThrowIfNull(mealPeriods);

        TimeZoneInfo timezone;
        try
        {
            timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new ArgumentException($"Timezone '{timezoneId}' was not found.", nameof(timezoneId), exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new ArgumentException($"Timezone '{timezoneId}' is invalid.", nameof(timezoneId), exception);
        }

        var localNow = TimeZoneInfo.ConvertTime(utcNow.ToUniversalTime(), timezone);
        var active = mealPeriods
            .Where(period => period.IsEnabled && IsActive(period, localNow, timezone))
            .OrderBy(period => period.SortOrder)
            .ThenBy(period => period.Id)
            .FirstOrDefault();

        var next = mealPeriods
            .Where(period => period.IsEnabled)
            .SelectMany(period => Enumerable.Range(0, 8)
                .Where(days => IsActiveDay(period.ActiveDaysMask, localNow.AddDays(days).DayOfWeek))
                .Select(days => (Period: period, Occurrence: LocalTimeOccurrenceResolver.Resolve(timezone, localNow.Date.AddDays(days).Add(period.StartLocalTime)))))
            .Where(candidate => candidate.Occurrence.ResolvedLocalTime > localNow)
            .OrderBy(candidate => candidate.Occurrence.ResolvedLocalTime)
            .ThenBy(candidate => candidate.Period.SortOrder)
            .ThenBy(candidate => candidate.Period.Id)
            .FirstOrDefault();

        return new MealPeriodScheduleResolution(
            localNow,
            active,
            next.Period,
            next.Period is null ? null : next.Occurrence.ResolvedLocalTime,
            next.Period is null ? null : next.Occurrence.Adjustment);
    }

    private static bool IsActive(MealPeriod period, DateTimeOffset localNow, TimeZoneInfo timezone) =>
        LocalTimeOccurrenceResolver.IsWindowActive(
            timezone, localNow, period.StartLocalTime, period.EndLocalTime, period.ActiveDaysMask);

    private static bool IsActiveDay(int activeDaysMask, DayOfWeek day) =>
        (activeDaysMask & (1 << (int)day)) != 0;
}
