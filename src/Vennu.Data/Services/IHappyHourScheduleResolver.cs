using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IHappyHourScheduleResolver
{
    HappyHourResolution Resolve(string timezoneId, DateTimeOffset utcNow, HappyHourSchedule? schedule);
}

public sealed record HappyHourResolution(
    bool IsActive,
    DateTimeOffset LocalNow,
    DateTimeOffset? EndsAtUtc,
    string Mode);
