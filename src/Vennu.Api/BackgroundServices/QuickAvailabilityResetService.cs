using Vennu.Api.Notifications;
using Vennu.Data.Repositories;

namespace Vennu.Api.BackgroundServices;

public sealed class QuickAvailabilityResetService(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<QuickAvailabilityResetService> logger) : BackgroundService
{
    public async Task<int> CheckOnceAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        var notifier = scope.ServiceProvider.GetRequiredService<IScreenUpdateNotifier>();
        var restored = await repository.RestoreExpiredAvailabilityAsync(
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken).ConfigureAwait(false);
        foreach (var item in restored)
        {
            await notifier.NotifyVenueItemAvailabilityChangedAsync(
                item.VenueId,
                item.ItemId.ToString(),
                true,
                cancellationToken).ConfigureAwait(false);
        }
        if (restored.Count > 0)
        {
            logger.LogInformation("Restored {ItemCount} quick-update items after venue-local midnight.", restored.Count);
        }
        return restored.Count;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CheckOnceAsync(stoppingToken).ConfigureAwait(false);
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1), timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            await CheckOnceAsync(stoppingToken).ConfigureAwait(false);
        }
    }
}
