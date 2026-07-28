namespace Vennu.Data.Services;

public interface IOperationalEventFeedService
{
    Task<IReadOnlyCollection<OperationalEventFeedItem>> GetRecentAsync(
        int limit = 20,
        CancellationToken cancellationToken = default);
}
