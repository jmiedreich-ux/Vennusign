using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class HappyHourService(
    IHappyHourScheduleRepository repository,
    IVenueRepository venues,
    IHappyHourScheduleResolver resolver) : IHappyHourService
{
    public async Task<HappyHourSnapshot> GetAsync(
        Guid venueId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        RequireVenueId(venueId);
        var venue = await venues.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Venue does not exist.");
        var schedule = await repository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var state = resolver.Resolve(venue.Timezone, utcNow, schedule);
        return new(schedule, state, true);
    }

    public async Task<HappyHourSnapshot> UpdateAsync(
        Guid venueId,
        TimeSpan startLocalTime,
        TimeSpan endLocalTime,
        int activeDaysMask,
        bool isEnabled,
        string overrideMode,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
    {
        RequireVenueId(venueId);
        ValidateWindow(startLocalTime, endLocalTime, activeDaysMask);
        var schedule = new HappyHourSchedule
        {
            VenueId = venueId,
            StartLocalTime = startLocalTime,
            EndLocalTime = endLocalTime,
            ActiveDaysMask = activeDaysMask,
            IsEnabled = isEnabled,
            OverrideMode = HappyHourOverrideMode.Normalize(overrideMode),
            UpdatedUtc = utcNow.UtcDateTime
        };
        await repository.UpsertAsync(schedule, cancellationToken).ConfigureAwait(false);
        return await GetAsync(venueId, utcNow, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateWindow(TimeSpan start, TimeSpan end, int activeDaysMask)
    {
        if (start < TimeSpan.Zero || start >= TimeSpan.FromDays(1)) throw new ArgumentOutOfRangeException(nameof(start));
        if (end < TimeSpan.Zero || end >= TimeSpan.FromDays(1)) throw new ArgumentOutOfRangeException(nameof(end));
        if (start == end) throw new ArgumentException("Start and end times must differ.");
        if (activeDaysMask is < 1 or > 127) throw new ArgumentOutOfRangeException(nameof(activeDaysMask));
    }

    private static void RequireVenueId(Guid venueId)
    {
        if (venueId == Guid.Empty) throw new ArgumentException("Identifier cannot be empty.", nameof(venueId));
    }
}
