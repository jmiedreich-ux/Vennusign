using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class RevenueDailySnapshotRepository(ISqlDataAccess dataAccess)
    : IRevenueDailySnapshotRepository
{
    private const string UpsertSql = """
        MERGE dbo.RevenueDailySnapshots WITH (HOLDLOCK) AS target
        USING (SELECT @SnapshotDateUtc AS SnapshotDateUtc) AS source
           ON target.SnapshotDateUtc = source.SnapshotDateUtc
        WHEN MATCHED
          THEN UPDATE
             SET Currency = @Currency,
                 Mrr = @Mrr,
                 Arr = @Arr,
                 AverageRevenuePerActiveSubscription = @AverageRevenuePerActiveSubscription,
                 ActiveSubscriptions = @ActiveSubscriptions,
                 CapturedUtc = @CapturedUtc
        WHEN NOT MATCHED
          THEN INSERT (
               SnapshotDateUtc,
               Currency,
               Mrr,
               Arr,
               AverageRevenuePerActiveSubscription,
               ActiveSubscriptions,
               CapturedUtc)
               VALUES (
               @SnapshotDateUtc,
               @Currency,
               @Mrr,
               @Arr,
               @AverageRevenuePerActiveSubscription,
               @ActiveSubscriptions,
               @CapturedUtc)
        OUTPUT inserted.SnapshotDateUtc,
               inserted.Currency,
               inserted.Mrr,
               inserted.Arr,
               inserted.AverageRevenuePerActiveSubscription,
               inserted.ActiveSubscriptions,
               inserted.CapturedUtc;
        """;

    private const string RecentMonthlySql = """
        WITH RankedSnapshots AS
        (
            SELECT SnapshotDateUtc,
                   Currency,
                   Mrr,
                   Arr,
                   AverageRevenuePerActiveSubscription,
                   ActiveSubscriptions,
                   CapturedUtc,
                   ROW_NUMBER() OVER (
                       PARTITION BY YEAR(SnapshotDateUtc), MONTH(SnapshotDateUtc)
                       ORDER BY SnapshotDateUtc DESC, CapturedUtc DESC) AS MonthRank
            FROM dbo.RevenueDailySnapshots
        )
        SELECT TOP (@Limit)
               SnapshotDateUtc,
               Currency,
               Mrr,
               Arr,
               AverageRevenuePerActiveSubscription,
               ActiveSubscriptions,
               CapturedUtc
        FROM RankedSnapshots
        WHERE MonthRank = 1
        ORDER BY SnapshotDateUtc DESC, CapturedUtc DESC;
        """;

    public async Task UpsertAsync(
        RevenueDailySnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _ = await dataAccess.ExecuteSqlQueryAsync<RevenueDailySnapshot, RevenueDailySnapshot>(
            UpsertSql,
            snapshot,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RevenueDailySnapshot>> GetRecentMonthlyAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 25)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Monthly snapshot limit must be between 1 and 25.");
        }

        return (await dataAccess.ExecuteSqlQueryAsync<RevenueDailySnapshot, object>(
            RecentMonthlySql,
            new { Limit = limit },
            cancellationToken).ConfigureAwait(false)).ToArray();
    }
}
