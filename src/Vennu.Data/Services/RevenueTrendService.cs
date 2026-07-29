using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class RevenueTrendService(
    IRevenueDailySnapshotRepository repository,
    TimeProvider timeProvider) : IRevenueTrendService
{
    public Task CaptureAsync(
        RevenueSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!string.Equals(snapshot.Currency, "USD", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Revenue trend snapshots support USD only.");
        }

        var capturedUtc = timeProvider.GetUtcNow().UtcDateTime;
        return repository.UpsertAsync(
            new RevenueDailySnapshot
            {
                SnapshotDateUtc = new DateTime(
                    capturedUtc.Year,
                    capturedUtc.Month,
                    capturedUtc.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                Currency = "USD",
                Mrr = snapshot.Mrr,
                Arr = snapshot.Arr,
                AverageRevenuePerActiveSubscription = snapshot.AverageRevenuePerActiveSubscription,
                ActiveSubscriptions = snapshot.ActiveSubscriptions,
                CapturedUtc = capturedUtc
            },
            cancellationToken);
    }

    public async Task<RevenueTrend> GetAsync(
        int months,
        CancellationToken cancellationToken = default)
    {
        if (months is < 1 or > 24)
        {
            throw new ArgumentOutOfRangeException(nameof(months), "Trend month limit must be between 1 and 24.");
        }

        var snapshots = (await repository
            .GetRecentMonthlyAsync(months + 1, cancellationToken)
            .ConfigureAwait(false))
            .OrderBy(snapshot => snapshot.SnapshotDateUtc)
            .ThenBy(snapshot => snapshot.CapturedUtc)
            .ToArray();
        var points = new List<RevenueTrendPoint>(snapshots.Length);

        for (var index = 0; index < snapshots.Length; index++)
        {
            var current = snapshots[index];
            decimal? changePercent = null;
            if (index > 0)
            {
                var previous = snapshots[index - 1];
                var previousMonth = StartOfMonth(previous.SnapshotDateUtc);
                if (previous.Mrr != 0m &&
                    StartOfMonth(current.SnapshotDateUtc) == previousMonth.AddMonths(1))
                {
                    changePercent = decimal.Round(
                        (current.Mrr - previous.Mrr) / previous.Mrr * 100m,
                        2,
                        MidpointRounding.AwayFromZero);
                }
            }

            points.Add(
                new RevenueTrendPoint(
                    StartOfMonth(current.SnapshotDateUtc),
                    current.Mrr,
                    current.ActiveSubscriptions,
                    changePercent));
        }

        return new RevenueTrend("USD", points.TakeLast(months).ToArray());
    }

    private static DateTime StartOfMonth(DateTime value) =>
        new(value.Year, value.Month, 1, 0, 0, 0, DateTimeKind.Utc);
}
