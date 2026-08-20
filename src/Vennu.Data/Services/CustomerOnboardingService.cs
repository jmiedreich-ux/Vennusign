using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed record PublicOnboardingPlan(
    Guid Id,
    string Name,
    string Slug,
    decimal MonthlyPrice,
    int TrialDays,
    int MaxVenues,
    int MaxScreens,
    bool MonthlyCheckoutAvailable,
    bool AnnualCheckoutAvailable);

public sealed record CustomerOnboardingProgress(
    bool Account,
    bool Plan,
    bool Venue,
    bool FirstScreen,
    bool GoLive);

public sealed record CustomerOnboardingOrganizationProfile(
    string Name,
    string? LegalName,
    string? PrimaryContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? MailingAddress);

public sealed record CustomerOnboardingSnapshot(
    Guid UserId,
    Guid? OrganizationId,
    CustomerOnboardingOrganizationProfile? Organization,
    Guid? SelectedTierId,
    Guid? VenueId,
    Guid? FirstScreenId,
    string CurrentStep,
    string EntitlementStatus,
    DateTime? TrialEndsAt,
    bool CheckoutPending,
    string FirstScreenStatus,
    DateTime? FirstScreenLastSeenUtc,
    DateTime? GoLiveAchievedUtc,
    CustomerOnboardingProgress Progress,
    DateTime UpdatedUtc);

public sealed record CustomerOnboardingVenueRequest(
    string Name,
    string Timezone,
    string Type,
    string PrimaryLanguage,
    string? SecondaryLanguage);

public interface ICustomerOnboardingService
{
    Task<IReadOnlyCollection<PublicOnboardingPlan>> GetPublicPlansAsync(CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> CreateOrganizationAsync(Guid userId, OrganizationProfile profile, CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> StartTrialAsync(Guid userId, Guid tierId, CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> CreateVenueAsync(
        Guid userId,
        CustomerOnboardingVenueRequest request,
        CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> ClaimFirstScreenAsync(
        Guid userId,
        string pairingCode,
        CancellationToken cancellationToken = default);
    Task<StripeCheckoutSessionResult> CreateCheckoutAsync(
        Guid userId,
        Guid tierId,
        CheckoutBillingInterval billingInterval,
        CancellationToken cancellationToken = default);
}

public sealed class CustomerOnboardingService(
    ICustomerOnboardingRepository onboarding,
    ISubscriptionTierRepository tiers,
    IOrganizationSubscriptionRepository subscriptions,
    IOrganizationMembershipRepository organizations,
    IIdentityMembershipService memberships,
    IOrganizationSubscriptionManagementService subscriptionManagement,
    ICheckoutSessionService checkout,
    IVenueProvisioningService venueProvisioning,
    IVenueEntitlementService venueEntitlement,
    IScreenPairingCodeRepository pairingCodes,
    IScreenRepository screens,
    TimeProvider timeProvider) : ICustomerOnboardingService
{
    public async Task<IReadOnlyCollection<PublicOnboardingPlan>> GetPublicPlansAsync(
        CancellationToken cancellationToken = default) =>
        (await tiers.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .Where(tier => tier.IsActive && tier.IsPublic)
            .OrderBy(tier => tier.Price)
            .ThenBy(tier => tier.Name, StringComparer.OrdinalIgnoreCase)
            .Select(tier => new PublicOnboardingPlan(
                tier.Id,
                tier.Name,
                tier.Slug,
                tier.Price,
                tier.TrialDays,
                tier.MaxVenues,
                tier.MaxScreens,
                !string.IsNullOrWhiteSpace(tier.StripeMonthlyPriceId),
                !string.IsNullOrWhiteSpace(tier.StripeAnnualPriceId)))
            .ToArray();

    public async Task<CustomerOnboardingSnapshot> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        RequireId(userId, nameof(userId));
        var state = await onboarding.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return await SnapshotAsync(state ?? NewState(userId), cancellationToken).ConfigureAwait(false);
    }

    public async Task<CustomerOnboardingSnapshot> CreateOrganizationAsync(
        Guid userId,
        OrganizationProfile profile,
        CancellationToken cancellationToken = default)
    {
        RequireId(userId, nameof(userId));
        var state = await onboarding.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false) ?? NewState(userId);
        if (state.OrganizationId is not null)
            throw new InvalidOperationException("This onboarding journey already has an organization.");
        var organization = await memberships.CreateOrganizationAsync(profile, userId, cancellationToken).ConfigureAwait(false);
        state.OrganizationId = organization.Id;
        state.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        state = await onboarding.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        return await SnapshotAsync(state, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CustomerOnboardingSnapshot> StartTrialAsync(
        Guid userId,
        Guid tierId,
        CancellationToken cancellationToken = default)
    {
        var (state, tier) = await RequirePlanSelectionAsync(userId, tierId, cancellationToken).ConfigureAwait(false);
        if (tier.TrialDays <= 0)
            throw new InvalidOperationException("The selected plan does not offer a no-card trial.");
        await subscriptionManagement.StartTrialAsync(state.OrganizationId!.Value, tier.Id, cancellationToken).ConfigureAwait(false);
        state.SelectedTierId = tier.Id;
        state.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        state = await onboarding.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        return await SnapshotAsync(state, cancellationToken).ConfigureAwait(false);
    }

    public async Task<StripeCheckoutSessionResult> CreateCheckoutAsync(
        Guid userId,
        Guid tierId,
        CheckoutBillingInterval billingInterval,
        CancellationToken cancellationToken = default)
    {
        var (state, tier) = await RequirePlanSelectionAsync(userId, tierId, cancellationToken).ConfigureAwait(false);
        state.SelectedTierId = tier.Id;
        state.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        await onboarding.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        return await checkout.CreateForOrganizationAsync(
            state.OrganizationId!.Value,
            tier.Id,
            billingInterval,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<CustomerOnboardingSnapshot> CreateVenueAsync(
        Guid userId,
        CustomerOnboardingVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var state = await RequireEntitledStateAsync(userId, cancellationToken).ConfigureAwait(false);
        if (state.VenueId is not null)
            throw new InvalidOperationException("This onboarding journey already has a venue.");
        var timezone = Required(request.Timezone, 100, nameof(request.Timezone));
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(timezone); }
        catch (TimeZoneNotFoundException) { throw new ArgumentException("Use a valid IANA timezone.", nameof(request.Timezone)); }
        catch (InvalidTimeZoneException) { throw new ArgumentException("Use a valid IANA timezone.", nameof(request.Timezone)); }
        var primaryLanguage = Language(request.PrimaryLanguage, nameof(request.PrimaryLanguage));
        var secondaryLanguage = string.IsNullOrWhiteSpace(request.SecondaryLanguage)
            ? null
            : Language(request.SecondaryLanguage, nameof(request.SecondaryLanguage));
        if (secondaryLanguage == primaryLanguage)
            throw new ArgumentException("Secondary language must differ from primary language.", nameof(request.SecondaryLanguage));
        var result = await venueProvisioning.ProvisionAsync(new Venue
        {
            OrganizationId = state.OrganizationId,
            Name = Required(request.Name, 200, nameof(request.Name)),
            Timezone = timezone,
            Type = Required(request.Type, 50, nameof(request.Type)),
            PrimaryLanguage = primaryLanguage,
            SecondaryLanguage = secondaryLanguage
        }, cancellationToken).ConfigureAwait(false);
        state.VenueId = result.VenueId;
        state.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        state = await onboarding.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        return await SnapshotAsync(state, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CustomerOnboardingSnapshot> ClaimFirstScreenAsync(
        Guid userId,
        string pairingCode,
        CancellationToken cancellationToken = default)
    {
        var state = await RequireEntitledStateAsync(userId, cancellationToken).ConfigureAwait(false);
        if (state.VenueId is null)
            throw new InvalidOperationException("Create a venue before pairing the first display.");
        if (state.FirstScreenId is not null)
            throw new InvalidOperationException("This onboarding journey already has a first display.");
        var code = Required(pairingCode, 6, nameof(pairingCode));
        if (code.Length != 6 || code.Any(character => !char.IsAsciiDigit(character)))
            throw new ArgumentException("Enter the six-digit code shown on the display.", nameof(pairingCode));
        await venueEntitlement.EnsureCanAddScreenAsync(state.VenueId.Value, cancellationToken).ConfigureAwait(false);
        var pairing = await pairingCodes.GetByCodeAsync(code, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The pairing code was not found. Request a fresh code on the display.");
        if (pairing.IsClaimed)
            throw new InvalidOperationException("The pairing code was already claimed. Request a fresh code on the display.");
        if (pairing.ExpiresAt <= timeProvider.GetUtcNow().UtcDateTime)
            throw new InvalidOperationException("The pairing code expired. Request a fresh code on the display.");
        var screen = await screens.GetByIdAsync(pairing.ScreenId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The display screen record was not found.");
        if (screen.VenueId is not null)
            throw new InvalidOperationException("That display is already assigned to a venue.");
        if (!await pairingCodes.ClaimAsync(code, state.VenueId.Value, cancellationToken).ConfigureAwait(false) ||
            !await screens.AssignVenueAsync(screen.Id, state.VenueId.Value, cancellationToken).ConfigureAwait(false))
            throw new InvalidOperationException("The display could not be paired. Request a fresh code and try again.");
        state.FirstScreenId = screen.Id;
        state.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        state = await onboarding.SaveAsync(state, cancellationToken).ConfigureAwait(false);
        return await SnapshotAsync(state, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(CustomerOnboardingState State, SubscriptionTier Tier)> RequirePlanSelectionAsync(
        Guid userId,
        Guid tierId,
        CancellationToken cancellationToken)
    {
        RequireId(userId, nameof(userId));
        RequireId(tierId, nameof(tierId));
        var state = await onboarding.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Create an organization before selecting a plan.");
        if (state.OrganizationId is null)
            throw new InvalidOperationException("Create an organization before selecting a plan.");
        var tier = await tiers.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("The selected plan does not exist.");
        if (!tier.IsActive || !tier.IsPublic)
            throw new InvalidOperationException("The selected plan is not available for signup.");
        var existing = await subscriptions.GetByOrganizationIdAsync(state.OrganizationId.Value, cancellationToken).ConfigureAwait(false);
        if (IsEntitled(existing, timeProvider.GetUtcNow().UtcDateTime))
            throw new InvalidOperationException("This organization already has an active entitlement.");
        return (state, tier);
    }

    private async Task<CustomerOnboardingSnapshot> SnapshotAsync(
        CustomerOnboardingState state,
        CancellationToken cancellationToken)
    {
        var subscription = state.OrganizationId is Guid organizationId
            ? await subscriptions.GetByOrganizationIdAsync(organizationId, cancellationToken).ConfigureAwait(false)
            : null;
        var organization = state.OrganizationId is Guid profileOrganizationId
            ? await organizations.GetOrganizationAsync(profileOrganizationId, cancellationToken).ConfigureAwait(false)
            : null;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var planComplete = IsEntitled(subscription, now);
        var venueComplete = state.VenueId is not null;
        var firstScreen = state.FirstScreenId is Guid firstScreenId
            ? await screens.GetByIdAsync(firstScreenId, cancellationToken).ConfigureAwait(false)
            : null;
        var firstScreenComplete = firstScreen is not null;
        var firstScreenOnlineNow = firstScreen is not null && firstScreen.Status.Equals("Online", StringComparison.OrdinalIgnoreCase);
        // Completing onboarding is an achievement, not a live device reading. HeartbeatMonitor
        // returns an Online screen to Offline after its stale threshold, so deriving completion
        // from current status sent every customer whose display was powered down back to the
        // opening checklist. The achievement is latched on the heartbeat that first reports
        // Online; current status still drives what the go-live panel says.
        var goLiveComplete = state.GoLiveAchievedUtc is not null;
        var currentStep = state.OrganizationId is null ? "account"
            : !planComplete ? "plan"
            : !venueComplete ? "venue"
            : !firstScreenComplete ? "first-screen"
            : "go-live";
        return new CustomerOnboardingSnapshot(
            state.UserId,
            state.OrganizationId,
            organization is null ? null : new CustomerOnboardingOrganizationProfile(
                organization.Name, organization.LegalName, organization.PrimaryContactName,
                organization.ContactEmail, organization.ContactPhone, organization.MailingAddress),
            state.SelectedTierId ?? subscription?.TierId,
            state.VenueId,
            state.FirstScreenId,
            currentStep,
            subscription?.Status ?? "none",
            subscription?.TrialEndsAt,
            state.SelectedTierId is not null && subscription is null,
            firstScreen is null ? "not-paired" : firstScreenOnlineNow ? "online" : "paired-offline",
            firstScreen?.LastSeen,
            state.GoLiveAchievedUtc,
            new CustomerOnboardingProgress(true, planComplete, venueComplete, firstScreenComplete, goLiveComplete),
            state.UpdatedUtc);
    }

    private async Task<CustomerOnboardingState> RequireEntitledStateAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        RequireId(userId, nameof(userId));
        var state = await onboarding.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Create an organization and select a plan before venue setup.");
        if (state.OrganizationId is null)
            throw new InvalidOperationException("Create an organization before venue setup.");
        var subscription = await subscriptions.GetByOrganizationIdAsync(state.OrganizationId.Value, cancellationToken).ConfigureAwait(false);
        if (!IsEntitled(subscription, timeProvider.GetUtcNow().UtcDateTime))
            throw new InvalidOperationException("An active trial or paid entitlement is required.");
        return state;
    }

    private static bool IsEntitled(OrganizationSubscription? subscription, DateTime utcNow) =>
        subscription?.Status == "active" ||
        subscription is { Status: "trialing", TrialEndsAt: DateTime trialEndsAt } && trialEndsAt > utcNow;

    private CustomerOnboardingState NewState(Guid userId)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        return new CustomerOnboardingState { UserId = userId, CreatedUtc = utcNow, UpdatedUtc = utcNow };
    }

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty ID is required.", parameterName);

    private static string Required(string? value, int maximumLength, string parameterName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) throw new ArgumentException("A value is required.", parameterName);
        return normalized.Length <= maximumLength
            ? normalized
            : throw new ArgumentException($"The value cannot exceed {maximumLength} characters.", parameterName);
    }

    private static string Language(string? value, string parameterName)
    {
        var normalized = Required(value, 2, parameterName).ToLowerInvariant();
        return normalized.Length == 2 && normalized.All(char.IsAsciiLetter)
            ? normalized
            : throw new ArgumentException("Use a two-letter language code.", parameterName);
    }
}
