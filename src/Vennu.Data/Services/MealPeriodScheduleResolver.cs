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
            .Where(period => period.IsEnabled && IsActive(period, localNow))
            .OrderBy(period => period.SortOrder)
            .ThenBy(period => period.Id)
            .FirstOrDefault();

        var next = mealPeriods
            .Where(period => period.IsEnabled)
            .SelectMany(period => Enumerable.Range(0, 8)
                .Where(days => IsActiveDay(period.ActiveDaysMask, localNow.AddDays(days).DayOfWeek))
                .Select(days => (Period: period, Starts: LocalOccurrence(timezone, localNow.Date.AddDays(days).Add(period.StartLocalTime)))))
            .Where(candidate => candidate.Starts > localNow)
            .OrderBy(candidate => candidate.Starts)
            .ThenBy(candidate => candidate.Period.SortOrder)
            .ThenBy(candidate => candidate.Period.Id)
            .FirstOrDefault();

        return new MealPeriodScheduleResolution(localNow, active, next.Period, next.Period is null ? null : next.Starts);
    }

    private static DateTimeOffset LocalOccurrence(TimeZoneInfo timezone, DateTime localDateTime)
    {
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(unspecified, timezone);
        return TimeZoneInfo.ConvertTime(new DateTimeOffset(utc, TimeSpan.Zero), timezone);
    }

    private static bool IsActive(MealPeriod period, DateTimeOffset localNow)
    {
        var localTime = localNow.TimeOfDay;
        if (period.StartLocalTime == period.EndLocalTime)
        {
            return false;
        }

        if (period.StartLocalTime < period.EndLocalTime)
        {
            return IsActiveDay(period.ActiveDaysMask, localNow.DayOfWeek)
                && localTime >= period.StartLocalTime
                && localTime < period.EndLocalTime;
        }

        if (localTime >= period.StartLocalTime)
        {
            return IsActiveDay(period.ActiveDaysMask, localNow.DayOfWeek);
        }

        return localTime < period.EndLocalTime
            && IsActiveDay(period.ActiveDaysMask, localNow.AddDays(-1).DayOfWeek);
    }

    private static bool IsActiveDay(int activeDaysMask, DayOfWeek day) =>
        (activeDaysMask & (1 << (int)day)) != 0;
}
