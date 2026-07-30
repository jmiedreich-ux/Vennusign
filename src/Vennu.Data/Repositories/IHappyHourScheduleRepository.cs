using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IHappyHourScheduleRepository
{
    Task<HappyHourSchedule?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);

    Task UpsertAsync(HappyHourSchedule schedule, CancellationToken cancellationToken = default);
}
