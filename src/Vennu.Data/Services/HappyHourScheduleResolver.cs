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
        var active = IsActive(schedule, localNow, timezone);
        return new(active, localNow, active ? ResolveEndUtc(schedule, localNow, timezone) : null, mode);
    }

    private static bool IsActive(HappyHourSchedule schedule, DateTimeOffset localNow, TimeZoneInfo timezone) =>
        LocalTimeOccurrenceResolver.IsWindowActive(
            timezone, localNow, schedule.StartLocalTime, schedule.EndLocalTime, schedule.ActiveDaysMask);

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
        var occurrence = LocalTimeOccurrenceResolver.Resolve(timezone, endDate.Add(schedule.EndLocalTime));
        return occurrence.ResolvedLocalTime.ToUniversalTime();
    }

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
