using System.Collections.Concurrent;
using Vennu.Api.Notifications;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.BackgroundServices;

public sealed class ScheduledContentActivationService(
    IServiceScopeFactory scopeFactory,
    IMealPeriodScheduleResolver resolver,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider,
    ILogger<ScheduledContentActivationService> logger) : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, Guid> activePeriods = new();

    public async Task<int> CheckOnceAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var venues = await scope.ServiceProvider.GetRequiredService<IVenueRepository>()
            .GetAllAsync(cancellationToken).ConfigureAwait(false);
        var periods = scope.ServiceProvider.GetRequiredService<IMealPeriodRepository>();
        var transitions = 0;
        foreach (var venue in venues.OrderBy(item => item.Id))
        {
            var resolution = resolver.Resolve(
                venue.Timezone,
                timeProvider.GetUtcNow(),
                await periods.GetByVenueIdAsync(venue.Id, cancellationToken).ConfigureAwait(false));
            var next = resolution.ActiveMealPeriod;
            var nextId = next?.Id ?? Guid.Empty;
            if (activePeriods.TryGetValue(venue.Id, out var previous) && previous == nextId)
            {
                continue;
            }

            activePeriods[venue.Id] = nextId;
            if (previous == Guid.Empty && next is null)
            {
                continue;
            }

            await notifier.NotifyVenueContentUpdatedAsync(
                venue.Id,
                new ScheduleActivationPayload(
                    "scheduled-content-transition",
                    next?.Id,
                    next?.Name,
                    next?.TargetLayout,
                    next?.MenuFilter,
                    next?.ThemePresetKey,
                    resolution.LocalNow),
                cancellationToken).ConfigureAwait(false);
            transitions++;
        }

        if (transitions > 0)
        {
            logger.LogInformation("Published {TransitionCount} scheduled content transitions.", transitions);
        }
        return transitions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CheckOnceAsync(stoppingToken).ConfigureAwait(false);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60), timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            await CheckOnceAsync(stoppingToken).ConfigureAwait(false);
        }
    }
}

public sealed record ScheduleActivationPayload(
    string Change,
    Guid? MealPeriodId,
    string? MealPeriodName,
    string? Layout,
    string? MenuFilter,
    string? ThemePresetKey,
    DateTimeOffset VenueLocalTime);
