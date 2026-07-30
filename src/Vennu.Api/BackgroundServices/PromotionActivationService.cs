using System.Collections.Concurrent;
using Vennu.Api.Notifications;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.BackgroundServices;

public sealed class PromotionActivationService(
    IServiceScopeFactory scopeFactory,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider,
    ILogger<PromotionActivationService> logger) : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, Guid> activePromotions = new();

    public async Task<int> CheckOnceAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var venues = await scope.ServiceProvider.GetRequiredService<IVenueRepository>()
            .GetAllAsync(cancellationToken).ConfigureAwait(false);
        var service = scope.ServiceProvider.GetRequiredService<IDateRangePromotionService>();
        var transitions = 0;
        foreach (var venue in venues.OrderBy(item => item.Id))
        {
            var active = await service.GetActiveAsync(
                venue.Id, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
            var nextId = active?.Id ?? Guid.Empty;
            if (activePromotions.TryGetValue(venue.Id, out var previous) && previous == nextId)
                continue;
            activePromotions[venue.Id] = nextId;
            if (previous == Guid.Empty && active is null) continue;
            await notifier.NotifyVenueContentUpdatedAsync(
                venue.Id,
                new
                {
                    change = "date-range-promotion-transition",
                    promotionId = active?.Id,
                    promotionName = active?.Name,
                    layout = active?.TargetLayout,
                    title = active?.Title,
                    body = active?.Body
                },
                cancellationToken).ConfigureAwait(false);
            transitions++;
        }
        if (transitions > 0)
            logger.LogInformation("Published {TransitionCount} promotion transitions.", transitions);
        return transitions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CheckOnceAsync(stoppingToken).ConfigureAwait(false);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60), timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            await CheckOnceAsync(stoppingToken).ConfigureAwait(false);
    }
}
