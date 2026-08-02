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

public sealed record CustomerOnboardingSnapshot(
    Guid UserId,
    Guid? OrganizationId,
    Guid? SelectedTierId,
    Guid? VenueId,
    Guid? FirstScreenId,
    string CurrentStep,
    string EntitlementStatus,
    DateTime? TrialEndsAt,
    bool CheckoutPending,
    CustomerOnboardingProgress Progress,
    DateTime UpdatedUtc);

public interface ICustomerOnboardingService
{
    Task<IReadOnlyCollection<PublicOnboardingPlan>> GetPublicPlansAsync(CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> CreateOrganizationAsync(Guid userId, string name, CancellationToken cancellationToken = default);
    Task<CustomerOnboardingSnapshot> StartTrialAsync(Guid userId, Guid tierId, CancellationToken cancellationToken = default);
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
    IIdentityMembershipService memberships,
    IOrganizationSubscriptionManagementService subscriptionManagement,
    ICheckoutSessionService checkout,
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
        string name,
        CancellationToken cancellationToken = default)
    {
        RequireId(userId, nameof(userId));
        var state = await onboarding.GetByUserIdAsync(userId, cancellationToken).ConfigureAwait(false) ?? NewState(userId);
        if (state.OrganizationId is not null)
            throw new InvalidOperationException("This onboarding journey already has an organization.");
        var organization = await memberships.CreateOrganizationAsync(name, userId, cancellationToken).ConfigureAwait(false);
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
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var planComplete = IsEntitled(subscription, now);
        var venueComplete = state.VenueId is not null;
        var firstScreenComplete = state.FirstScreenId is not null;
        var currentStep = state.OrganizationId is null ? "account"
            : !planComplete ? "plan"
            : !venueComplete ? "venue"
            : !firstScreenComplete ? "first-screen"
            : "go-live";
        return new CustomerOnboardingSnapshot(
            state.UserId,
            state.OrganizationId,
            state.SelectedTierId ?? subscription?.TierId,
            state.VenueId,
            state.FirstScreenId,
            currentStep,
            subscription?.Status ?? "none",
            subscription?.TrialEndsAt,
            state.SelectedTierId is not null && subscription is null,
            new CustomerOnboardingProgress(true, planComplete, venueComplete, firstScreenComplete, firstScreenComplete),
            state.UpdatedUtc);
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
}
