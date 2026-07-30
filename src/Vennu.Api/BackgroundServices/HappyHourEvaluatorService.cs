using System.Collections.Concurrent;
using Vennu.Api.Notifications;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.BackgroundServices;

public sealed class HappyHourEvaluatorService(
    IServiceScopeFactory scopeFactory,
    IScreenUpdateNotifier notifier,
    TimeProvider timeProvider,
    ILogger<HappyHourEvaluatorService> logger) : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, bool> states = new();

    public async Task<int> CheckOnceAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var venues = await scope.ServiceProvider.GetRequiredService<IVenueRepository>()
            .GetAllAsync(cancellationToken).ConfigureAwait(false);
        var service = scope.ServiceProvider.GetRequiredService<IHappyHourService>();
        var transitions = 0;
        foreach (var venue in venues.OrderBy(item => item.Id))
        {
            var state = (await service.GetAsync(
                venue.Id, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false)).State;
            if (states.TryGetValue(venue.Id, out var previous) && previous == state.IsActive) continue;
            states[venue.Id] = state.IsActive;
            if (!previous && !state.IsActive) continue;
            await notifier.NotifyVenueContentUpdatedAsync(
                venue.Id,
                new { change = "happy-hour-transition", isHappyHour = state.IsActive, endsAtUtc = state.EndsAtUtc, mode = state.Mode },
                cancellationToken).ConfigureAwait(false);
            transitions++;
        }
        if (transitions > 0) logger.LogInformation("Published {TransitionCount} happy-hour transitions.", transitions);
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
