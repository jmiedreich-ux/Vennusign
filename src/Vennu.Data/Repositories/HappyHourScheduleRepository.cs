using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class HappyHourScheduleRepository(ISqlDataAccess dataAccess) : IHappyHourScheduleRepository
{
    public async Task<HappyHourSchedule?> GetByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.QueryAsync<HappyHourSchedule, object>(
            "dbo.HappyHourSchedules",
            new { VenueId = RequireVenueId(venueId) },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public async Task UpsertAsync(
        HappyHourSchedule schedule,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        RequireVenueId(schedule.VenueId);
        await dataAccess.MergeAllAsync(
            new[] { schedule },
            "dbo.HappyHourSchedules",
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private static Guid RequireVenueId(Guid venueId) =>
        venueId == Guid.Empty
            ? throw new ArgumentException("Identifier cannot be empty.", nameof(venueId))
            : venueId;
}
