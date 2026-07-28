namespace Vennu.Data.Services;

public sealed record OperationalEventFeedItem(
    Guid Id,
    Guid VenueId,
    string VenueName,
    string EventType,
    string Summary,
    DateTime OccurredUtc);
