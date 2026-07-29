using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IRevenueDailySnapshotRepository
{
    Task UpsertAsync(
        RevenueDailySnapshot snapshot,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<RevenueDailySnapshot>> GetRecentMonthlyAsync(
        int limit,
        CancellationToken cancellationToken = default);
}
