using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class OperationalDashboardService(
    IVenueRepository venueRepository,
    IVenueSubscriptionRepository subscriptionRepository,
    IScreenRepository screenRepository,
    TimeProvider timeProvider) : IOperationalDashboardService
{
    public async Task<OperationalDashboard> GetAsync(CancellationToken cancellationToken = default)
    {
        var venuesTask = venueRepository.GetAllAsync(cancellationToken);
        var subscriptionsTask = subscriptionRepository.GetAllAsync(cancellationToken);
        var screensTask = screenRepository.GetAllAsync(cancellationToken);
        await Task.WhenAll(venuesTask, subscriptionsTask, screensTask).ConfigureAwait(false);

        var venues = venuesTask.Result;
        var subscriptions = subscriptionsTask.Result;
        var screens = screensTask.Result;
        var venueNames = venues.ToDictionary(venue => venue.Id, venue => venue.Name);
        var canceledCutoff = timeProvider.GetUtcNow().UtcDateTime.AddDays(-30);
        var health = screens
            .Select(screen => new OperationalScreenHealth(
                screen.Id,
                screen.VenueId,
                screen.VenueId is not null && venueNames.TryGetValue(screen.VenueId.Value, out var venueName)
                    ? venueName
                    : "Unassigned",
                screen.Name,
                screen.Location,
                NormalizeStatus(screen.Status),
                screen.LastSeen))
            .OrderBy(item => item.VenueName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.ScreenName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.ScreenId)
            .ToArray();

        return new OperationalDashboard(
            venues.Count,
            subscriptions.Count(subscription => subscription.Status.Equals("active", StringComparison.OrdinalIgnoreCase)),
            subscriptions.Count(subscription => subscription.Status.Equals("trialing", StringComparison.OrdinalIgnoreCase)),
            subscriptions.Count(subscription =>
                subscription.Status.Equals("canceled", StringComparison.OrdinalIgnoreCase) &&
                subscription.UpdatedUtc >= canceledCutoff),
            health.Count(screen => screen.Status == "online"),
            health.Count(screen => screen.Status != "online"),
            health);
    }

    private static string NormalizeStatus(string? status) =>
        string.Equals(status, "online", StringComparison.OrdinalIgnoreCase) ? "online" : "offline";
}
