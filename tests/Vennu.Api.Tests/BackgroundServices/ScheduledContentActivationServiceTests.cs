using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Vennu.Api.BackgroundServices;
using Vennu.Api.Notifications;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.BackgroundServices;

[Trait("Category", "Unit")]
public sealed class ScheduledContentActivationServiceTests
{
    [Fact]
    public async Task CheckOnceAsync_PublishesOnlyEffectiveTransitions()
    {
        var venue = new Venue { Id = Guid.NewGuid(), Timezone = "UTC" };
        var period = new MealPeriod
        {
            Id = Guid.NewGuid(), VenueId = venue.Id, Name = "Breakfast",
            StartLocalTime = TimeSpan.FromHours(7), EndLocalTime = TimeSpan.FromHours(11),
            ActiveDaysMask = 127, IsEnabled = true, TargetLayout = "photo_grid"
        };
        var services = new ServiceCollection()
            .AddSingleton<IVenueRepository>(new FakeVenueRepository { GetAllAsyncHandler = _ => Task.FromResult<IReadOnlyCollection<Venue>>([venue]) })
            .AddSingleton<IMealPeriodRepository>(new MealPeriods(period))
            .BuildServiceProvider();
        var notifier = new RecordingNotifier();
        var service = new ScheduledContentActivationService(
            services.GetRequiredService<IServiceScopeFactory>(),
            new MealPeriodScheduleResolver(),
            notifier,
            new FixedTimeProvider(),
            NullLogger<ScheduledContentActivationService>.Instance);

        Assert.Equal(1, await service.CheckOnceAsync());
        Assert.Equal(0, await service.CheckOnceAsync());
        Assert.Single(notifier.Payloads);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 30, 8, 0, 0, TimeSpan.Zero);
    }

    private sealed class MealPeriods(params MealPeriod[] values) : IMealPeriodRepository
    {
        public Task<IReadOnlyCollection<MealPeriod>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<MealPeriod>>(values);
        public Task<Guid> CreateAsync(MealPeriod mealPeriod, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateAsync(MealPeriod mealPeriod, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteAsync(Guid venueId, Guid mealPeriodId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class RecordingNotifier : IScreenUpdateNotifier
    {
        public List<object> Payloads { get; } = [];
        public Task NotifyVenueContentUpdatedAsync(Guid venueId, object payload, CancellationToken cancellationToken = default) { Payloads.Add(payload); return Task.CompletedTask; }
        public Task NotifyScreenContentUpdatedAsync(Guid screenId, object payload, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenThemeUpdatedAsync(Guid screenId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueThemeUpdatedAsync(Guid venueId, object theme, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenItemAvailabilityChangedAsync(Guid screenId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueItemAvailabilityChangedAsync(Guid venueId, string itemId, bool available, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyScreenSyncTickAsync(Guid screenId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyVenueSyncTickAsync(Guid venueId, long serverTimeMs, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
