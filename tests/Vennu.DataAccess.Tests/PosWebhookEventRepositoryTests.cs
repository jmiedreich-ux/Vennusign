using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class PosWebhookEventRepositoryTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 1, 10, 0, TimeSpan.Zero);

    [Fact]
    public async Task EnqueueAsync_DeduplicatesByProviderAndEventId()
    {
        string? sql = null;
        object? parameters = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (value, args) =>
            {
                sql = value; parameters = args;
                return [new PosWebhookEventRepository.ChangeResult { Changed = true }];
            }
        };
        var repository = new PosWebhookEventRepository(data, new FixedTimeProvider(UtcNow));

        Assert.True(await repository.EnqueueAsync(Event()));

        Assert.Contains("Provider = @Provider AND ProviderEventId = @ProviderEventId", sql, StringComparison.Ordinal);
        Assert.Contains("UPDLOCK, HOLDLOCK", sql, StringComparison.Ordinal);
        Assert.Equal((int)PosProvider.Square, Property<int>(parameters!, "Provider"));
        Assert.Equal("event-1", Property<string>(parameters!, "ProviderEventId"));
        Assert.Equal(UtcNow.UtcDateTime, Property<DateTime>(parameters!, "ReceivedUtc"));
    }

    [Fact]
    public async Task TryClaimNextAsync_UsesLockingLeaseAndStableQueueOrder()
    {
        string? sql = null;
        var data = new FakeSqlDataAccess { ExecuteSqlQueryHandler = (value, _) => { sql = value; return []; } };
        var repository = new PosWebhookEventRepository(data, new FixedTimeProvider(UtcNow));

        Assert.Null(await repository.TryClaimNextAsync(UtcNow.UtcDateTime, UtcNow.AddMinutes(-5).UtcDateTime));

        Assert.Contains("UPDLOCK, READPAST, ROWLOCK", sql, StringComparison.Ordinal);
        Assert.Contains("Status = 1 AND StartedUtc <= @StaleBeforeUtc", sql, StringComparison.Ordinal);
        Assert.Contains("ORDER BY ReceivedUtc, Id", sql, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MarkFailedAsync_PersistsBoundedRetryState()
    {
        string? sql = null;
        var data = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (value, _) => { sql = value; return [new PosWebhookEventRepository.ChangeResult { Changed = true }]; }
        };
        var repository = new PosWebhookEventRepository(data, new FixedTimeProvider(UtcNow));

        Assert.True(await repository.MarkFailedAsync(Guid.NewGuid(), "retry", UtcNow.UtcDateTime, UtcNow.AddMinutes(2).UtcDateTime));
        Assert.Contains("NextAttemptUtc = @NextAttemptUtc", sql, StringComparison.Ordinal);
    }

    private static PosWebhookEvent Event() => new()
    {
        Provider = PosProvider.Square,
        ProviderEventId = "event-1",
        EventType = "inventory.count.updated",
        ExternalMerchantId = "merchant-1",
        Payload = "{}"
    };

    private static T Property<T>(object value, string name) => (T)value.GetType().GetProperty(name)!.GetValue(value)!;
    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider { public override DateTimeOffset GetUtcNow() => utcNow; }
}
