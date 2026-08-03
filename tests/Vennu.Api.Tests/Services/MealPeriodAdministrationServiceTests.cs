using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class MealPeriodAdministrationServiceTests
{
    [Fact]
    public async Task GetAsync_ReportsRegularAndOvernightConflicts()
    {
        var venueId = Guid.NewGuid();
        var repository = new RepositoryFake(
            Period(venueId, "Late", 22, 2, DayOfWeek.Friday, 0),
            Period(venueId, "Special", 1, 3, DayOfWeek.Saturday, 1));
        var service = new MealPeriodAdministrationService(repository, new FixedTimeProvider());

        var snapshot = await service.GetAsync(venueId);

        var conflict = Assert.Single(snapshot.Conflicts);
        Assert.Equal("Late", conflict.FirstName);
        Assert.Equal("Special", conflict.SecondName);
    }

    [Fact]
    public async Task CreateAsync_TrimsNameAndAppendsPriority()
    {
        var venueId = Guid.NewGuid();
        var repository = new RepositoryFake(Period(venueId, "Breakfast", 7, 11, DayOfWeek.Monday, 4));
        var service = new MealPeriodAdministrationService(repository, new FixedTimeProvider());

        var created = await service.CreateAsync(
            venueId, "  Lunch  ", TimeSpan.FromHours(11), TimeSpan.FromHours(15), 127, true);

        Assert.Equal("Lunch", created.Name);
        Assert.Equal(5, created.SortOrder);
        Assert.Equal(new DateTime(2026, 7, 30, 6, 30, 0, DateTimeKind.Utc), created.CreatedUtc);
        Assert.Same(created, repository.Created);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotCrossVenueBoundary()
    {
        var repository = new RepositoryFake(Period(Guid.NewGuid(), "Other", 7, 11, DayOfWeek.Monday, 0));
        var service = new MealPeriodAdministrationService(repository, new FixedTimeProvider());

        var result = await service.UpdateAsync(
            Guid.NewGuid(), Guid.NewGuid(), "Changed", TimeSpan.FromHours(8), TimeSpan.FromHours(12), 127, true);

        Assert.Null(result);
        Assert.Null(repository.Updated);
    }

    [Fact]
    public async Task CreateAsync_RejectsEmptyActiveDays()
    {
        var service = new MealPeriodAdministrationService(new RepositoryFake(), new FixedTimeProvider());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CreateAsync(Guid.NewGuid(), "Breakfast", TimeSpan.FromHours(7), TimeSpan.FromHours(11), 0, true));
    }

    [Fact]
    public async Task ReorderAsync_PersistsCompleteVenueOrder()
    {
        var venueId = Guid.NewGuid();
        var first = Period(venueId, "Breakfast", 7, 11, DayOfWeek.Monday, 0);
        var second = Period(venueId, "Lunch", 11, 15, DayOfWeek.Monday, 1);
        var repository = new RepositoryFake(first, second);
        var service = new MealPeriodAdministrationService(repository, new FixedTimeProvider());

        var result = await service.ReorderAsync(venueId, [second.Id, first.Id]);

        Assert.Collection(result,
            item => { Assert.Same(second, item); Assert.Equal(0, item.SortOrder); },
            item => { Assert.Same(first, item); Assert.Equal(1, item.SortOrder); });
        Assert.Equal(2, repository.UpdateCount);
    }

    [Fact]
    public async Task ReorderAsync_RejectsPartialOrder()
    {
        var venueId = Guid.NewGuid();
        var first = Period(venueId, "Breakfast", 7, 11, DayOfWeek.Monday, 0);
        var second = Period(venueId, "Lunch", 11, 15, DayOfWeek.Monday, 1);
        var service = new MealPeriodAdministrationService(new RepositoryFake(first, second), new FixedTimeProvider());

        await Assert.ThrowsAsync<ArgumentException>(() => service.ReorderAsync(venueId, [first.Id]));
    }

    private static MealPeriod Period(Guid venueId, string name, int start, int end, DayOfWeek day, int order) => new()
    {
        Id = Guid.NewGuid(),
        VenueId = venueId,
        Name = name,
        StartLocalTime = TimeSpan.FromHours(start),
        EndLocalTime = TimeSpan.FromHours(end),
        ActiveDaysMask = 1 << (int)day,
        IsEnabled = true,
        SortOrder = order
    };

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 7, 30, 6, 30, 0, TimeSpan.Zero);
    }

    private sealed class RepositoryFake(params MealPeriod[] periods) : IMealPeriodRepository
    {
        private readonly List<MealPeriod> values = [.. periods];
        public MealPeriod? Created { get; private set; }
        public MealPeriod? Updated { get; private set; }
        public int UpdateCount { get; private set; }

        public Task<Guid> CreateAsync(MealPeriod mealPeriod, CancellationToken cancellationToken = default)
        {
            Created = mealPeriod; values.Add(mealPeriod); return Task.FromResult(mealPeriod.Id);
        }
        public Task<bool> UpdateAsync(MealPeriod mealPeriod, CancellationToken cancellationToken = default)
        {
            Updated = mealPeriod; UpdateCount++; return Task.FromResult(true);
        }
        public Task<bool> DeleteAsync(Guid venueId, Guid mealPeriodId, CancellationToken cancellationToken = default) =>
            Task.FromResult(values.RemoveAll(item => item.VenueId == venueId && item.Id == mealPeriodId) > 0);
        public Task<IReadOnlyCollection<MealPeriod>> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<MealPeriod>>(values.Where(item => item.VenueId == venueId).OrderBy(item => item.SortOrder).ToArray());
    }
}
