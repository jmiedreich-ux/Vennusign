using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class VenueSupportDetailService : IVenueSupportDetailService
{
    private readonly IVenueRepository venueRepository;
    private readonly IVenueSubscriptionRepository subscriptionRepository;
    private readonly ISubscriptionTierRepository tierRepository;
    private readonly IScreenRepository screenRepository;
    private readonly IFeatureResolutionService featureResolutionService;
    private readonly IVenueFeatureOverrideRepository overrideRepository;
    private readonly TimeProvider timeProvider;

    public VenueSupportDetailService(
        IVenueRepository venueRepository,
        IVenueSubscriptionRepository subscriptionRepository,
        ISubscriptionTierRepository tierRepository,
        IScreenRepository screenRepository,
        IFeatureResolutionService featureResolutionService,
        IVenueFeatureOverrideRepository overrideRepository,
        TimeProvider timeProvider)
    {
        this.venueRepository = venueRepository;
        this.subscriptionRepository = subscriptionRepository;
        this.tierRepository = tierRepository;
        this.screenRepository = screenRepository;
        this.featureResolutionService = featureResolutionService;
        this.overrideRepository = overrideRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<VenueSupportDetail?> GetAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        if (venueId == Guid.Empty)
        {
            throw new ArgumentException("Venue ID is required.", nameof(venueId));
        }

        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        if (venue is null)
        {
            return null;
        }

        var subscription = await subscriptionRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var tiers = subscription is null
            ? []
            : await tierRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var screens = await screenRepository.GetByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        var features = await featureResolutionService.GetFeatureSetAsync(venueId, cancellationToken).ConfigureAwait(false);
        var overrides = await overrideRepository
            .GetActiveByVenueAsync(venueId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken)
            .ConfigureAwait(false);

        return new VenueSupportDetail(
            venue,
            subscription,
            subscription is null ? null : tiers.FirstOrDefault(tier => tier.Id == subscription.TierId),
            screens.OrderBy(screen => screen.Name, StringComparer.OrdinalIgnoreCase).ThenBy(screen => screen.Id).ToArray(),
            features,
            overrides.OrderBy(item => item.FeatureId).ToArray());
    }
}
