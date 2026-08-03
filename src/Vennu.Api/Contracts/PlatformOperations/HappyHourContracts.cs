using Vennu.Data.Services;

namespace Vennu.Api.Contracts.PlatformOperations;

public sealed record HappyHourWriteRequest(
    TimeSpan StartLocalTime,
    TimeSpan EndLocalTime,
    int ActiveDaysMask,
    bool IsEnabled,
    string OverrideMode);

public sealed record HappyHourResponse(
    Vennu.Core.Models.HappyHourSchedule? Schedule,
    bool IsActive,
    DateTimeOffset? EndsAtUtc,
    string Mode,
    bool IsEntitled)
{
    public static HappyHourResponse From(HappyHourSnapshot snapshot) =>
        new(snapshot.Schedule, snapshot.State.IsActive, snapshot.State.EndsAtUtc, snapshot.State.Mode, snapshot.IsEntitled);
}
