using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class BillingCatalogService : IBillingCatalogService
{
    private const decimal AnnualBillingMonths = 10m;
    private readonly IBillingCatalogRepository repository;
    private readonly TimeProvider timeProvider;

    public BillingCatalogService(IBillingCatalogRepository repository, TimeProvider timeProvider)
    {
        this.repository = repository;
        this.timeProvider = timeProvider;
    }

    public async Task<IReadOnlyCollection<BillingCatalogItem>> GetPublicCatalogAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken).ConfigureAwait(false))
            .Where(tier => tier.IsActive && tier.IsPublic)
            .OrderBy(tier => tier.Price)
            .Select(ToCatalogItem)
            .ToArray();

    public async Task<BillingCatalogItem> ConfigureStripeAsync(
        Guid tierId,
        string productId,
        string monthlyPriceId,
        string annualPriceId,
        CancellationToken cancellationToken = default)
    {
        if (tierId == Guid.Empty)
        {
            throw new ArgumentException("Tier ID is required.", nameof(tierId));
        }

        productId = NormalizeIdentifier(productId, nameof(productId));
        monthlyPriceId = NormalizeIdentifier(monthlyPriceId, nameof(monthlyPriceId));
        annualPriceId = NormalizeIdentifier(annualPriceId, nameof(annualPriceId));
        if (monthlyPriceId.Equals(annualPriceId, StringComparison.Ordinal))
        {
            throw new ArgumentException("Monthly and annual Stripe price IDs must be different.");
        }

        var tier = await repository.GetByIdAsync(tierId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Subscription tier '{tierId}' does not exist.");

        await EnsureIdentifierAvailableAsync(
            await repository.GetByStripeProductIdAsync(productId, cancellationToken).ConfigureAwait(false),
            tierId,
            productId).ConfigureAwait(false);
        await EnsureIdentifierAvailableAsync(
            await repository.GetByStripePriceIdAsync(monthlyPriceId, cancellationToken).ConfigureAwait(false),
            tierId,
            monthlyPriceId).ConfigureAwait(false);
        await EnsureIdentifierAvailableAsync(
            await repository.GetByStripePriceIdAsync(annualPriceId, cancellationToken).ConfigureAwait(false),
            tierId,
            annualPriceId).ConfigureAwait(false);

        tier.StripeProductId = productId;
        tier.StripeMonthlyPriceId = monthlyPriceId;
        tier.StripeAnnualPriceId = annualPriceId;
        tier.UpdatedUtc = timeProvider.GetUtcNow().UtcDateTime;
        if (!await repository.SaveAsync(tier, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException("The Stripe billing catalog metadata could not be persisted.");
        }

        return ToCatalogItem(tier);
    }

    private static Task EnsureIdentifierAvailableAsync(
        SubscriptionTier? existing,
        Guid tierId,
        string identifier)
    {
        if (existing is not null && existing.Id != tierId)
        {
            throw new InvalidOperationException($"Stripe identifier '{identifier}' is already assigned to another tier.");
        }

        return Task.CompletedTask;
    }

    private static string NormalizeIdentifier(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    private static BillingCatalogItem ToCatalogItem(SubscriptionTier tier) =>
        new(
            tier.Id,
            tier.Name,
            tier.Slug,
            tier.Price,
            tier.Price * AnnualBillingMonths,
            tier.MaxScreens,
            tier.StripeProductId,
            tier.StripeMonthlyPriceId,
            tier.StripeAnnualPriceId);
}
