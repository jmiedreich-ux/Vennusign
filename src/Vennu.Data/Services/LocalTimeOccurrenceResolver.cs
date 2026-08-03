namespace Vennu.Data.Services;

public enum LocalTimeOccurrenceAdjustment
{
    Exact,
    AdvancedAfterGap,
    EarlierAmbiguousOccurrence
}

public sealed record LocalTimeOccurrence(
    DateTime RequestedLocalTime,
    DateTimeOffset ResolvedLocalTime,
    LocalTimeOccurrenceAdjustment Adjustment);

public static class LocalTimeOccurrenceResolver
{
    private const int MaximumGapMinutes = 180;

    public static LocalTimeOccurrence Resolve(TimeZoneInfo timezone, DateTime localDateTime)
    {
        ArgumentNullException.ThrowIfNull(timezone);
        var requested = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        var resolved = requested;
        var adjustment = LocalTimeOccurrenceAdjustment.Exact;

        for (var minute = 0; timezone.IsInvalidTime(resolved); minute++)
        {
            if (minute >= MaximumGapMinutes)
            {
                throw new InvalidTimeZoneException($"Timezone '{timezone.Id}' has an unsupported local-time gap.");
            }
            resolved = resolved.AddMinutes(1);
            adjustment = LocalTimeOccurrenceAdjustment.AdvancedAfterGap;
        }

        TimeSpan offset;
        if (timezone.IsAmbiguousTime(resolved))
        {
            // The larger offset maps to the earlier UTC instant.
            offset = timezone.GetAmbiguousTimeOffsets(resolved).Max();
            adjustment = LocalTimeOccurrenceAdjustment.EarlierAmbiguousOccurrence;
        }
        else
        {
            offset = timezone.GetUtcOffset(resolved);
        }

        return new(requested, new DateTimeOffset(resolved, offset), adjustment);
    }

    public static bool IsWindowActive(
        TimeZoneInfo timezone,
        DateTimeOffset localNow,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask)
    {
        if (startLocalTime == endLocalTime) return false;
        var nowUtc = localNow.ToUniversalTime();
        foreach (var scheduleDate in new[] { localNow.Date.AddDays(-1), localNow.Date })
        {
            if ((activeDaysMask & (1 << (int)scheduleDate.DayOfWeek)) == 0) continue;
            var endDate = startLocalTime < endLocalTime ? scheduleDate : scheduleDate.AddDays(1);
            var start = Resolve(timezone, scheduleDate.Add(startLocalTime)).ResolvedLocalTime.ToUniversalTime();
            var end = Resolve(timezone, endDate.Add(endLocalTime)).ResolvedLocalTime.ToUniversalTime();
            if (nowUtc >= start && nowUtc < end) return true;
        }
        return false;
    }
}
