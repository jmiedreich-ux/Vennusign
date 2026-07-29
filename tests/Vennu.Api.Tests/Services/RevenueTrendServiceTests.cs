using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class RevenueTrendServiceTests
{
    [Fact]
    public async Task CaptureAsync_UpsertsOneSnapshotPerUtcDay()
    {
        var repository = new RepositoryFake();
        var service = new RevenueTrendService(
            repository,
            new FixedTimeProvider(new DateTimeOffset(2026, 7, 29, 0, 15, 0, TimeSpan.Zero)));

        await service.CaptureAsync(Snapshot(100m, 4));
        await service.CaptureAsync(Snapshot(125m, 5));

        var stored = Assert.Single(repository.Items);
        Assert.Equal(new DateTime(2026, 7, 29, 0, 0, 0, DateTimeKind.Utc), stored.SnapshotDateUtc);
        Assert.Equal(125m, stored.Mrr);
        Assert.Equal(5, stored.ActiveSubscriptions);
        Assert.Equal(2, repository.UpsertCalls);
    }

    [Fact]
    public async Task GetAsync_ReturnsBoundedAscendingTrendWithDeterministicChange()
    {
        var repository = new RepositoryFake
        {
            Items =
            [
                Daily(2026, 6, 180m, 8),
                Daily(2026, 4, 120m, 6),
                Daily(2026, 3, 100m, 5)
            ]
        };
        var service = new RevenueTrendService(repository, TimeProvider.System);

        var result = await service.GetAsync(2);

        Assert.Equal("USD", result.Currency);
        Assert.Collection(
            result.Points,
            april =>
            {
                Assert.Equal(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), april.MonthUtc);
                Assert.Equal(20m, april.MrrChangePercent);
            },
            june =>
            {
                Assert.Equal(new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), june.MonthUtc);
                Assert.Null(june.MrrChangePercent);
            });
        Assert.Equal(3, repository.LastRequestedLimit);
    }

    [Fact]
    public async Task GetAsync_DoesNotFabricatePercentageFromZeroPriorMrr()
    {
        var repository = new RepositoryFake
        {
            Items =
            [
                Daily(2026, 1, 0m, 0),
                Daily(2026, 2, 50m, 2)
            ]
        };
        var service = new RevenueTrendService(repository, TimeProvider.System);

        var result = await service.GetAsync(12);

        Assert.Null(result.Points.Last().MrrChangePercent);
    }

    [Fact]
    public async Task CaptureAsync_RejectsNonUsdSnapshot()
    {
        var repository = new RepositoryFake();
        var service = new RevenueTrendService(repository, TimeProvider.System);
        var snapshot = Snapshot(100m, 4) with { Currency = "EUR" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CaptureAsync(snapshot));

        Assert.Empty(repository.Items);
    }

    private static RevenueSnapshot Snapshot(decimal mrr, int subscriptions) =>
        new(
            "USD",
            mrr,
            mrr * 12m,
            subscriptions == 0 ? 0m : mrr / subscriptions,
            subscriptions,
            [],
            0m,
            []);

    private static RevenueDailySnapshot Daily(
        int year,
        int month,
        decimal mrr,
        int subscriptions) =>
        new()
        {
            SnapshotDateUtc = new DateTime(year, month, DateTime.DaysInMonth(year, month), 0, 0, 0, DateTimeKind.Utc),
            Currency = "USD",
            Mrr = mrr,
            Arr = mrr * 12m,
            ActiveSubscriptions = subscriptions,
            CapturedUtc = new DateTime(year, month, DateTime.DaysInMonth(year, month), 12, 0, 0, DateTimeKind.Utc)
        };

    private sealed class RepositoryFake : IRevenueDailySnapshotRepository
    {
        private readonly Dictionary<DateTime, RevenueDailySnapshot> byDate = [];

        public List<RevenueDailySnapshot> Items
        {
            get => byDate.Values.ToList();
            init
            {
                foreach (var item in value)
                {
                    byDate[item.SnapshotDateUtc] = item;
                }
            }
        }

        public int UpsertCalls { get; private set; }
        public int LastRequestedLimit { get; private set; }

        public Task UpsertAsync(
            RevenueDailySnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            UpsertCalls++;
            byDate[snapshot.SnapshotDateUtc] = snapshot;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<RevenueDailySnapshot>> GetRecentMonthlyAsync(
            int limit,
            CancellationToken cancellationToken = default)
        {
            LastRequestedLimit = limit;
            return Task.FromResult<IReadOnlyCollection<RevenueDailySnapshot>>(
                byDate.Values.OrderByDescending(item => item.SnapshotDateUtc).Take(limit).ToArray());
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
