using Microsoft.Extensions.Options;
using Vennu.Data.Repositories;

namespace Vennu.Api.BackgroundServices;

public sealed class HeartbeatMonitor : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly HeartbeatMonitorOptions options;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<HeartbeatMonitor> logger;

    public HeartbeatMonitor(
        IServiceScopeFactory scopeFactory,
        IOptions<HeartbeatMonitorOptions> options,
        TimeProvider timeProvider,
        ILogger<HeartbeatMonitor> logger)
    {
        this.scopeFactory = scopeFactory;
        this.options = options.Value;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<int> CheckOnceAsync(CancellationToken cancellationToken = default)
    {
        var cutoffUtc = timeProvider.GetUtcNow().UtcDateTime - options.StaleThreshold;
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IScreenRepository>();
        var updated = await repository.MarkStaleOnlineScreensOfflineAsync(cutoffUtc, cancellationToken);

        if (updated > 0)
        {
            logger.LogInformation("Marked {ScreenCount} stale screens offline.", updated);
        }

        return updated;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CheckOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(options.CheckInterval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CheckOnceAsync(stoppingToken);
        }
    }
}
