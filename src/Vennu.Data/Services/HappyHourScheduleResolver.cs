using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed class HappyHourScheduleResolver : IHappyHourScheduleResolver
{
    public HappyHourResolution Resolve(
        string timezoneId,
        DateTimeOffset utcNow,
        HappyHourSchedule? schedule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timezoneId);
        var timezone = ResolveTimezone(timezoneId);
        var localNow = TimeZoneInfo.ConvertTime(utcNow.ToUniversalTime(), timezone);
        if (schedule is null || !schedule.IsEnabled)
        {
            return new(false, localNow, null, HappyHourOverrideMode.Automatic);
        }

        var mode = HappyHourOverrideMode.Normalize(schedule.OverrideMode);
        if (mode == HappyHourOverrideMode.ForceOn) return new(true, localNow, null, mode);
        if (mode == HappyHourOverrideMode.ForceOff) return new(false, localNow, null, mode);
        var active = IsActive(schedule, localNow);
        return new(active, localNow, active ? ResolveEndUtc(schedule, localNow, timezone) : null, mode);
    }

    private static bool IsActive(HappyHourSchedule schedule, DateTimeOffset localNow)
    {
        if (schedule.StartLocalTime == schedule.EndLocalTime) return false;
        var time = localNow.TimeOfDay;
        if (schedule.StartLocalTime < schedule.EndLocalTime)
        {
            return IsActiveDay(schedule.ActiveDaysMask, localNow.DayOfWeek)
                && time >= schedule.StartLocalTime && time < schedule.EndLocalTime;
        }
        return time >= schedule.StartLocalTime
            ? IsActiveDay(schedule.ActiveDaysMask, localNow.DayOfWeek)
            : time < schedule.EndLocalTime && IsActiveDay(schedule.ActiveDaysMask, localNow.AddDays(-1).DayOfWeek);
    }

    private static DateTimeOffset ResolveEndUtc(
        HappyHourSchedule schedule,
        DateTimeOffset localNow,
        TimeZoneInfo timezone)
    {
        var endDate = localNow.Date;
        if (schedule.StartLocalTime >= schedule.EndLocalTime && localNow.TimeOfDay >= schedule.StartLocalTime)
        {
            endDate = endDate.AddDays(1);
        }
        var localEnd = DateTime.SpecifyKind(endDate.Add(schedule.EndLocalTime), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localEnd, timezone), TimeSpan.Zero);
    }

    private static bool IsActiveDay(int mask, DayOfWeek day) => (mask & (1 << (int)day)) != 0;

    private static TimeZoneInfo ResolveTimezone(string timezoneId)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new ArgumentException($"Timezone '{timezoneId}' is invalid.", nameof(timezoneId), exception);
        }
    }
}
