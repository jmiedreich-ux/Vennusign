using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public class StripeEventIdempotencyServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 7, 28, 15, 30, 0, TimeSpan.Zero);

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Claimed_event_executes_handler_and_marks_processed()
    {
        var repository = new ProcessedEventRepositoryFake { ClaimAvailable = true };
        var service = CreateService(repository);
        var executions = 0;

        var executed = await service.ExecuteOnceAsync("evt_1", "invoice.paid", _ =>
        {
            executions++;
            return Task.CompletedTask;
        });

        Assert.True(executed);
        Assert.Equal(1, executions);
        Assert.Equal("evt_1", Assert.Single(repository.ProcessedEventIds));
        Assert.Equal(UtcNow.UtcDateTime.AddMinutes(-5), repository.LastStaleBeforeUtc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Duplicate_event_does_not_execute_handler()
    {
        var repository = new ProcessedEventRepositoryFake();
        var service = CreateService(repository);
        var executions = 0;

        var executed = await service.ExecuteOnceAsync("evt_1", "invoice.paid", _ =>
        {
            executions++;
            return Task.CompletedTask;
        });

        Assert.False(executed);
        Assert.Equal(0, executions);
        Assert.Empty(repository.ProcessedEventIds);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Failed_handler_marks_event_retryable_and_rethrows()
    {
        var repository = new ProcessedEventRepositoryFake { ClaimAvailable = true };
        var service = CreateService(repository);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteOnceAsync("evt_1", "invoice.paid", _ =>
                throw new InvalidOperationException("billing update failed")));

        Assert.Equal("billing update failed", error.Message);
        var failed = Assert.Single(repository.Failed);
        Assert.Equal("evt_1", failed.EventId);
        Assert.Equal("billing update failed", failed.FailureReason);
    }

    private static StripeEventIdempotencyService CreateService(ProcessedEventRepositoryFake repository) =>
        new(repository, new FixedTimeProvider(UtcNow));

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    private sealed class ProcessedEventRepositoryFake : IProcessedStripeEventRepository
    {
        public bool ClaimAvailable { get; set; }
        public DateTime LastStaleBeforeUtc { get; private set; }
        public List<string> ProcessedEventIds { get; } = [];
        public List<(string EventId, string FailureReason)> Failed { get; } = [];

        public Task<ProcessedStripeEvent?> TryClaimAsync(
            string eventId,
            string eventType,
            DateTime utcNow,
            DateTime staleBeforeUtc,
            CancellationToken cancellationToken = default)
        {
            LastStaleBeforeUtc = staleBeforeUtc;
            return Task.FromResult<ProcessedStripeEvent?>(ClaimAvailable
                ? new ProcessedStripeEvent
                {
                    EventId = eventId,
                    EventType = eventType,
                    Status = "processing",
                    StartedUtc = utcNow
                }
                : null);
        }

        public Task<bool> MarkProcessedAsync(
            string eventId,
            DateTime processedUtc,
            CancellationToken cancellationToken = default)
        {
            ProcessedEventIds.Add(eventId);
            return Task.FromResult(true);
        }

        public Task<bool> MarkFailedAsync(
            string eventId,
            string failureReason,
            DateTime failedUtc,
            CancellationToken cancellationToken = default)
        {
            Failed.Add((eventId, failureReason));
            return Task.FromResult(true);
        }
    }
}
