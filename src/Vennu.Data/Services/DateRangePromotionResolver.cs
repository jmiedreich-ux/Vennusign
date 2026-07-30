using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed class DateRangePromotionResolver : IDateRangePromotionResolver
{
    public DateRangePromotionResolution Resolve(
        string timezoneId,
        DateTimeOffset utcNow,
        IReadOnlyCollection<DateRangePromotion> promotions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timezoneId);
        ArgumentNullException.ThrowIfNull(promotions);
        TimeZoneInfo timezone;
        try { timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId); }
        catch (TimeZoneNotFoundException exception)
        {
            throw new ArgumentException($"Timezone '{timezoneId}' was not found.", nameof(timezoneId), exception);
        }
        catch (InvalidTimeZoneException exception)
        {
            throw new ArgumentException($"Timezone '{timezoneId}' is invalid.", nameof(timezoneId), exception);
        }

        var localNow = TimeZoneInfo.ConvertTime(utcNow.ToUniversalTime(), timezone);
        var localDate = localNow.Date;
        var active = promotions
            .Where(item => item.IsEnabled
                && item.StartLocalDate.Date <= localDate
                && item.EndLocalDate.Date >= localDate)
            .OrderByDescending(item => item.Priority)
            .ThenByDescending(item => item.StartLocalDate)
            .ThenByDescending(item => item.CreatedUtc)
            .ThenBy(item => item.Id)
            .FirstOrDefault();
        return new DateRangePromotionResolution(localNow, active);
    }
}
