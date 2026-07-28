using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class OperationalEventFeedService(
    IOperationalEventRepository eventRepository,
    IVenueRepository venueRepository) : IOperationalEventFeedService
{
    public async Task<IReadOnlyCollection<OperationalEventFeedItem>> GetRecentAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var eventsTask = eventRepository.GetRecentAsync(limit, cancellationToken);
        var venuesTask = venueRepository.GetAllAsync(cancellationToken);
        await Task.WhenAll(eventsTask, venuesTask).ConfigureAwait(false);
        var venueNames = venuesTask.Result.ToDictionary(venue => venue.Id, venue => venue.Name);

        return eventsTask.Result
            .OrderByDescending(item => item.OccurredUtc)
            .ThenByDescending(item => item.Id)
            .Select(item => new OperationalEventFeedItem(
                item.Id,
                item.VenueId,
                venueNames.GetValueOrDefault(item.VenueId, "Unknown venue"),
                item.EventType,
                item.Summary,
                item.OccurredUtc))
            .ToArray();
    }
}
