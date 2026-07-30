using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IHappyHourService
{
    Task<HappyHourSnapshot> GetAsync(Guid venueId, DateTimeOffset utcNow, CancellationToken cancellationToken = default);

    Task<HappyHourSnapshot> UpdateAsync(
        Guid venueId,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask,
        bool isEnabled,
        string overrideMode,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default);
}

public sealed record HappyHourSnapshot(HappyHourSchedule? Schedule, HappyHourResolution State, bool IsEntitled);
