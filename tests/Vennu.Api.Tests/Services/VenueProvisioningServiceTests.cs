using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class VenueProvisioningServiceTests
{
    [Fact]
    public async Task ProvisionAsync_NormalizesVenueAndStartsStarterTrial()
    {
        var starter = new SubscriptionTier { Id = Guid.NewGuid(), Slug = "starter", IsActive = true };
        var venues = new VenueRepositoryFake();
        var subscriptions = new SubscriptionManagementFake();
        var service = new VenueProvisioningService(
            venues,
            new TierRepositoryFake(starter),
            subscriptions);

        var result = await service.ProvisionAsync(new Venue
        {
            Name = "  Harbor Café ",
            Timezone = " America/New_York ",
            Type = " Café ",
            PrimaryLanguage = " en ",
            SecondaryLanguage = " "
        });

        Assert.Equal(venues.VenueId, result.VenueId);
        Assert.Equal("Harbor Café", venues.CreatedVenue?.Name);
        Assert.Equal("America/New_York", venues.CreatedVenue?.Timezone);
        Assert.Null(venues.CreatedVenue?.SecondaryLanguage);
        Assert.Equal((venues.VenueId, starter.Id), subscriptions.StartedTrial);
        Assert.Equal("trialing", result.Subscription.Status);
    }

    [Fact]
    public async Task ProvisionAsync_DoesNotCreateVenue_WhenStarterTierIsUnavailable()
    {
        var venues = new VenueRepositoryFake();
        var service = new VenueProvisioningService(
            venues,
            new TierRepositoryFake(null),
            new SubscriptionManagementFake());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ProvisionAsync(ValidVenue()));

        Assert.Contains("Starter", exception.Message);
        Assert.Null(venues.CreatedVenue);
    }

    [Fact]
    public async Task ProvisionAsync_PropagatesDuplicateSubscriptionProtection()
    {
        var subscriptions = new SubscriptionManagementFake
        {
            StartTrialException = new InvalidOperationException("A subscription already exists for this venue.")
        };
        var service = new VenueProvisioningService(
            new VenueRepositoryFake(),
            new TierRepositoryFake(new SubscriptionTier { Id = Guid.NewGuid(), Slug = "starter", IsActive = true }),
            subscriptions);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ProvisionAsync(ValidVenue()));

        Assert.Contains("already exists", exception.Message);
    }

    private static Venue ValidVenue() => new()
    {
        Name = "Harbor Café",
        Timezone = "UTC",
        Type = "Café",
        PrimaryLanguage = "en"
    };

    private sealed class VenueRepositoryFake : IVenueRepository
    {
        public Guid VenueId { get; } = Guid.NewGuid();
        public Venue? CreatedVenue { get; private set; }
        public Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            CreatedVenue = venue;
            venue.Id = VenueId;
            return Task.FromResult(VenueId);
        }

        public Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Venue>>([]);

        public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Venue?>(null);
    }

    private sealed class TierRepositoryFake(SubscriptionTier? starterTier) : ISubscriptionTierRepository
    {
        public Task<SubscriptionTier?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
            Task.FromResult(slug == "starter" ? starterTier : null);

        public Task<IReadOnlyCollection<SubscriptionTier>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<SubscriptionTier>>(starterTier is null ? [] : [starterTier]);

        public Task<SubscriptionTier?> GetByIdAsync(Guid tierId, CancellationToken cancellationToken = default) =>
            Task.FromResult(starterTier?.Id == tierId ? starterTier : null);

        public Task<IReadOnlyCollection<TierFeature>> GetFeaturesAsync(Guid tierId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<TierFeature>>([]);

        public Task<bool> CreateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<bool> UpdateAsync(SubscriptionTier tier, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }

    private sealed class SubscriptionManagementFake : ISubscriptionManagementService
    {
        public (Guid VenueId, Guid TierId)? StartedTrial { get; private set; }
        public Exception? StartTrialException { get; init; }

        public Task<VenueSubscription> StartTrialAsync(Guid venueId, Guid tierId, CancellationToken cancellationToken = default)
        {
            if (StartTrialException is not null)
            {
                return Task.FromException<VenueSubscription>(StartTrialException);
            }

            StartedTrial = (venueId, tierId);
            return Task.FromResult(new VenueSubscription
            {
                VenueId = venueId,
                TierId = tierId,
                Status = "trialing"
            });
        }

        public Task<VenueSubscription> ChangeTierAsync(Guid venueId, Guid tierId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<VenueSubscription> SetStatusAsync(Guid venueId, string status, DateTime? currentPeriodEnd = null, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<int> ExpireTrialsAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
