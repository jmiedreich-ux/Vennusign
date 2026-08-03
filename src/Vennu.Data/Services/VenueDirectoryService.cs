using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class VenueDirectoryService : IVenueDirectoryService
{
    private readonly IVenueRepository venueRepository;
    private readonly IVenueSubscriptionRepository subscriptionRepository;
    private readonly ISubscriptionTierRepository tierRepository;
    private readonly IScreenRepository screenRepository;
    private readonly IVenueFeatureOverrideRepository overrideRepository;
    private readonly TimeProvider timeProvider;

    public VenueDirectoryService(
        IVenueRepository venueRepository,
        IVenueSubscriptionRepository subscriptionRepository,
        ISubscriptionTierRepository tierRepository,
        IScreenRepository screenRepository,
        IVenueFeatureOverrideRepository overrideRepository,
        TimeProvider timeProvider)
    {
        this.venueRepository = venueRepository;
        this.subscriptionRepository = subscriptionRepository;
        this.tierRepository = tierRepository;
        this.screenRepository = screenRepository;
        this.overrideRepository = overrideRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<VenueDirectoryItem>> SearchAsync(
        VenueDirectoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var venues = await venueRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var subscriptions = await subscriptionRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var tiers = await tierRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var subscriptionByVenue = subscriptions.ToDictionary(subscription => subscription.VenueId);
        var tierById = tiers.ToDictionary(tier => tier.Id);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var items = new List<VenueDirectoryItem>(venues.Count);

        foreach (var venue in venues)
        {
            subscriptionByVenue.TryGetValue(venue.Id, out var subscription);
            var tier = subscription is null ? null : tierById.GetValueOrDefault(subscription.TierId);
            var screens = await screenRepository.GetByVenueIdAsync(venue.Id, cancellationToken).ConfigureAwait(false);
            screens = screens.Where(screen => !string.Equals(screen.Status, "Archived", StringComparison.OrdinalIgnoreCase)).ToArray();
            var overrides = await overrideRepository.GetActiveByVenueAsync(venue.Id, now, cancellationToken).ConfigureAwait(false);
            var item = new VenueDirectoryItem(
                venue.Id,
                venue.Name,
                venue.Type,
                tier?.Id,
                tier?.Name,
                subscription?.Status ?? "unsubscribed",
                screens.Count,
                screens.Where(screen => screen.LastSeen.HasValue).Select(screen => screen.LastSeen).Max(),
                overrides.Count,
                ResolveHealth(screens));

            if (Matches(item, query))
            {
                items.Add(item);
            }
        }

        return items
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.VenueId)
            .ToArray();
    }

    private static string ResolveHealth(IReadOnlyCollection<Core.Models.Screen> screens)
    {
        if (screens.Count == 0) return "no_screens";
        var onlineCount = screens.Count(screen => string.Equals(screen.Status, "Online", StringComparison.OrdinalIgnoreCase));
        if (onlineCount == screens.Count) return "online";
        return onlineCount > 0 ? "degraded" : "offline";
    }

    private static bool Matches(VenueDirectoryItem item, VenueDirectoryQuery query)
    {
        return (string.IsNullOrWhiteSpace(query.Search) ||
                item.Name.Contains(query.Search.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(query.Tier) ||
                string.Equals(item.TierName, query.Tier.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(query.Status) ||
                string.Equals(item.SubscriptionStatus, query.Status.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(query.Health) ||
                string.Equals(item.Health, query.Health.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
