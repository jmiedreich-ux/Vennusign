using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class OperationalEventRepository(ISqlDataAccess dataAccess) : IOperationalEventRepository
{
    private const string RecentSql = """
        SELECT TOP (@Limit)
               Id,
               VenueId,
               EventType,
               Summary,
               OccurredUtc
        FROM dbo.OperationalEvents
        ORDER BY OccurredUtc DESC, Id DESC;
        """;

    public async Task AddAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operationalEvent);
        await dataAccess.MergeAllAsync(
            new[] { operationalEvent },
            "dbo.OperationalEvents",
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<OperationalEvent>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Recent-event limit must be between 1 and 100.");
        }

        return (await dataAccess.ExecuteSqlQueryAsync<OperationalEvent, object>(
            RecentSql,
            new { Limit = limit },
            cancellationToken).ConfigureAwait(false)).ToArray();
    }
}
