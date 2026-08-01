using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.BackgroundServices;

[Trait("Category", "Unit")]
public sealed class ToastPollingServiceTests
{
    [Fact]
    public async Task CheckOnceAsync_PreventsOverlap()
    {
        var coordinator = new BlockingCoordinator();
        var service = CreateService(coordinator);

        var first = service.CheckOnceAsync();
        await coordinator.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var overlap = await service.CheckOnceAsync();
        coordinator.Release.TrySetResult(true);
        var completed = await first;

        Assert.True(overlap.OverlapSkipped);
        Assert.False(completed.OverlapSkipped);
        Assert.Equal(1, completed.Succeeded);
    }

    [Fact]
    public async Task CheckOnceAsync_IsolatesConnectionFailure()
    {
        var venues = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var coordinator = new ResultCoordinator(venues, venues[1]);
        var service = CreateService(coordinator);

        var result = await service.CheckOnceAsync();

        Assert.Equal(3, result.ConnectionsDue);
        Assert.Equal(2, result.Succeeded);
        Assert.Equal(1, result.Failed);
        Assert.Equal(venues, coordinator.Attempts);
    }

    [Fact]
    public async Task CheckOnceAsync_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var coordinator = new CancellingCoordinator();
        var service = CreateService(coordinator);
        var task = service.CheckOnceAsync(cancellation.Token);
        await coordinator.Entered.Task.WaitAsync(TimeSpan.FromSeconds(2));

        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task Coordinator_SuccessClearsBackoffAndSchedulesHourlyPoll()
    {
        var now = new DateTimeOffset(2026, 8, 1, 3, 0, 0, TimeSpan.Zero);
        var connection = WithState(Connected(), failures: 3, errorCode: "toast_provider_unavailable");
        var repository = new ConnectionRepositoryFake(connection);
        var provider = BuildProvider(repository, new ProviderFake(new PosInventoryResult([], now)), new SyncFake());
        var coordinator = CreateCoordinator(provider, now);

        var result = await coordinator.PollAsync(connection.VenueId);

        Assert.True(result.Succeeded);
        Assert.Equal(0, repository.Connection.ConsecutiveSyncFailures);
        Assert.Null(repository.Connection.LastSyncErrorCode);
        Assert.Equal(now.UtcDateTime, repository.Connection.LastSyncedUtc);
        Assert.Equal(now.UtcDateTime.AddHours(1), repository.Connection.NextSyncAttemptUtc);
    }

    [Fact]
    public async Task Coordinator_FailureUsesBoundedExponentialBackoffWithoutRawError()
    {
        var now = new DateTimeOffset(2026, 8, 1, 3, 0, 0, TimeSpan.Zero);
        var connection = WithState(Connected(), failures: 2);
        var repository = new ConnectionRepositoryFake(connection);
        var provider = BuildProvider(repository, new ProviderFake(new HttpRequestException("provider detail")), new SyncFake());
        var coordinator = CreateCoordinator(provider, now);

        var result = await coordinator.PollAsync(connection.VenueId);

        Assert.False(result.Succeeded);
        Assert.Equal("toast_provider_unavailable", result.ErrorCode);
        Assert.Equal(3, repository.Connection.ConsecutiveSyncFailures);
        Assert.Equal(now.UtcDateTime.AddMinutes(20), repository.Connection.NextSyncAttemptUtc);
        Assert.Equal("toast_provider_unavailable", repository.Connection.LastSyncErrorCode);
        Assert.DoesNotContain("provider detail", repository.Connection.LastSyncErrorCode!);
    }

    private static ToastPollingService CreateService(IToastPollingCoordinator coordinator) =>
        new(coordinator, Options.Create(new ToastPollingOptions { InterConnectionDelay = TimeSpan.Zero }), TimeProvider.System, NullLogger<ToastPollingService>.Instance);

    private static ToastPollingCoordinator CreateCoordinator(ServiceProvider provider, DateTimeOffset now) =>
        new(provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new ToastPollingOptions
            {
                PollInterval = TimeSpan.FromHours(1),
                InitialFailureBackoff = TimeSpan.FromMinutes(5),
                MaximumFailureBackoff = TimeSpan.FromHours(1)
            }),
            new FixedTimeProvider(now),
            NullLogger<ToastPollingCoordinator>.Instance);

    private static ServiceProvider BuildProvider(
        ConnectionRepositoryFake connections,
        IPosProvider posProvider,
        IToastInventorySyncService sync)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IPosConnectionRepository>(connections);
        services.AddSingleton<IPosCatalogMappingRepository>(new MappingRepositoryFake());
        services.AddSingleton<IPosCredentialProtector>(new ProtectorFake());
        services.AddSingleton(posProvider);
        services.AddSingleton(sync);
        return services.BuildServiceProvider();
    }

    private static PosConnection Connected() => new()
    {
        Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Provider = PosProvider.Toast,
        Status = PosConnectionStatus.Connected, ExternalMerchantId = Guid.NewGuid().ToString(),
        ProtectedAccessToken = "protected-token"
    };

    private static PosConnection WithState(PosConnection value, int failures, string? errorCode = null)
    {
        value.ConsecutiveSyncFailures = failures;
        value.LastSyncErrorCode = errorCode;
        return value;
    }

    private sealed class BlockingCoordinator : IToastPollingCoordinator
    {
        public TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<bool> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<IReadOnlyCollection<Guid>> GetDueVenueIdsAsync(DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Guid>>([Guid.NewGuid()]);
        public async Task<ToastPollingConnectionResult> PollAsync(Guid venueId, CancellationToken cancellationToken = default) { Entered.TrySetResult(true); await Release.Task.WaitAsync(cancellationToken); return new(true); }
    }

    private sealed class ResultCoordinator(IReadOnlyCollection<Guid> venues, Guid failing) : IToastPollingCoordinator
    {
        public List<Guid> Attempts { get; } = [];
        public Task<IReadOnlyCollection<Guid>> GetDueVenueIdsAsync(DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult(venues);
        public Task<ToastPollingConnectionResult> PollAsync(Guid venueId, CancellationToken cancellationToken = default) { Attempts.Add(venueId); return Task.FromResult(new ToastPollingConnectionResult(venueId != failing)); }
    }

    private sealed class CancellingCoordinator : IToastPollingCoordinator
    {
        public TaskCompletionSource<bool> Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<IReadOnlyCollection<Guid>> GetDueVenueIdsAsync(DateTime utcNow, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<Guid>>([Guid.NewGuid()]);
        public async Task<ToastPollingConnectionResult> PollAsync(Guid venueId, CancellationToken cancellationToken = default) { Entered.TrySetResult(true); await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken); return new(true); }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider { public override DateTimeOffset GetUtcNow() => now; }

    private sealed class ConnectionRepositoryFake(PosConnection connection) : IPosConnectionRepository
    {
        public PosConnection Connection { get; private set; } = connection;
        public Task<PosConnection?> GetAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult<PosConnection?>(venueId == Connection.VenueId && provider == PosProvider.Toast ? Connection : null);
        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PosConnection>>([Connection]);
        public Task<IReadOnlyCollection<PosConnection>> GetAllByProviderAsync(PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PosConnection>>([Connection]);
        public Task<PosConnection> SaveAsync(Guid venueId, PosConnection value, CancellationToken cancellationToken = default) { Connection = value; return Task.FromResult(value); }
        public Task<bool> DeleteAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class MappingRepositoryFake : IPosCatalogMappingRepository
    {
        public Task<IReadOnlyCollection<PosCatalogMapping>> GetAllAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PosCatalogMapping>>([]);
        public Task<MenuItem?> GetMappedItemAsync(Guid venueId, PosProvider provider, string externalItemId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PosCatalogMapping> SaveAsync(Guid venueId, PosCatalogMapping mapping, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class ProviderFake : IPosProvider
    {
        private readonly PosInventoryResult? result;
        private readonly Exception? exception;
        public ProviderFake(PosInventoryResult result) => this.result = result;
        public ProviderFake(Exception exception) => this.exception = exception;
        public PosProvider Provider => PosProvider.Toast;
        public Task<PosCatalogResult> GetCatalogAsync(PosProviderContext context, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PosInventoryResult> GetInventoryAsync(PosProviderContext context, CancellationToken cancellationToken = default) => exception is null ? Task.FromResult(result!) : Task.FromException<PosInventoryResult>(exception);
    }

    private sealed class SyncFake : IToastInventorySyncService
    {
        public Task<ToastInventoryApplyResult> ApplyItemAsync(Guid venueId, PosInventoryItem inventory, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<ToastInventoryApplyResult> ApplySnapshotAsync(Guid venueId, IReadOnlyCollection<PosInventoryItem> inventory, CancellationToken cancellationToken = default) => Task.FromResult(new ToastInventoryApplyResult(inventory.Count, 0, 0));
    }

    private sealed class ProtectorFake : IPosCredentialProtector
    {
        public string Protect(string plaintext) => throw new NotSupportedException();
        public string Unprotect(string protectedValue) => "access-token";
    }
}
