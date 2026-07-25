using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Vennu.Api.BackgroundServices;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.BackgroundServices;

public class HeartbeatMonitorTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CheckOnceAsync_UsesNinetySecondCutoff()
    {
        var now = new DateTimeOffset(2026, 7, 25, 1, 30, 0, TimeSpan.Zero);
        DateTime? receivedCutoff = null;
        var repository = new FakeScreenRepository
        {
            MarkStaleOnlineScreensOfflineAsyncHandler = (cutoff, _) =>
            {
                receivedCutoff = cutoff;
                return Task.FromResult(2);
            }
        };
        using var provider = BuildProvider(repository);
        var sut = CreateMonitor(provider, now);

        var updated = await sut.CheckOnceAsync();

        Assert.Equal(2, updated);
        Assert.Equal(now.UtcDateTime.AddSeconds(-90), receivedCutoff);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CheckOnceAsync_CanRunRepeatedly()
    {
        var calls = 0;
        var repository = new FakeScreenRepository
        {
            MarkStaleOnlineScreensOfflineAsyncHandler = (_, _) => Task.FromResult(++calls == 1 ? 1 : 0)
        };
        using var provider = BuildProvider(repository);
        var sut = CreateMonitor(provider, DateTimeOffset.UtcNow);

        var first = await sut.CheckOnceAsync();
        var second = await sut.CheckOnceAsync();

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Equal(2, calls);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CheckOnceAsync_PropagatesCancellation()
    {
        var repository = new FakeScreenRepository
        {
            MarkStaleOnlineScreensOfflineAsyncHandler = (_, token) => Task.FromCanceled<int>(token)
        };
        using var provider = BuildProvider(repository);
        var sut = CreateMonitor(provider, DateTimeOffset.UtcNow);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.CheckOnceAsync(cancellationSource.Token));
    }

    private static ServiceProvider BuildProvider(IScreenRepository repository)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => repository);
        return services.BuildServiceProvider();
    }

    private static HeartbeatMonitor CreateMonitor(IServiceProvider provider, DateTimeOffset now)
    {
        var options = Options.Create(new HeartbeatMonitorOptions
        {
            CheckInterval = TimeSpan.FromSeconds(30),
            StaleThreshold = TimeSpan.FromSeconds(90)
        });

        return new HeartbeatMonitor(
            provider.GetRequiredService<IServiceScopeFactory>(),
            options,
            new FixedTimeProvider(now),
            NullLogger<HeartbeatMonitor>.Instance);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
