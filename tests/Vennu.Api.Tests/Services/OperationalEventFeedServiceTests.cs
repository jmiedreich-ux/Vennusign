using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class OperationalEventFeedServiceTests
{
    [Fact]
    public async Task GetRecentAsync_OrdersEventsAndAddsVenueContext()
    {
        var venueId = Guid.NewGuid();
        var older = new OperationalEvent
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            EventType = "signup",
            Summary = "New Pro subscription",
            OccurredUtc = new DateTime(2026, 7, 28, 20, 0, 0, DateTimeKind.Utc)
        };
        var newer = new OperationalEvent
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            EventType = "upgrade",
            Summary = "Upgraded to Business",
            OccurredUtc = older.OccurredUtc.AddMinutes(5)
        };
        var service = new OperationalEventFeedService(
            new EventRepositoryFake([older, newer]),
            new FakeVenueRepository
            {
                GetAllAsyncHandler = _ => Task.FromResult<IReadOnlyCollection<Venue>>(
                    [new Venue { Id = venueId, Name = "Harbor Cafe" }])
            });

        var result = await service.GetRecentAsync();

        Assert.Equal(newer.Id, result.First().Id);
        Assert.All(result, item => Assert.Equal("Harbor Cafe", item.VenueName));
    }

    private sealed class EventRepositoryFake(IReadOnlyCollection<OperationalEvent> items) : IOperationalEventRepository
    {
        public Task AddAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyCollection<OperationalEvent>> GetRecentAsync(
            int limit,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<OperationalEvent>>(items.Take(limit).ToArray());
    }
}
