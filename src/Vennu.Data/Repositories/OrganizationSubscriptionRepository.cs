using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class OrganizationSubscriptionRepository(ISqlDataAccess dataAccess)
    : IOrganizationSubscriptionRepository
{
    public async Task<IReadOnlyCollection<OrganizationSubscription>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        (await dataAccess.QueryAllAsync<OrganizationSubscription>(cancellationToken).ConfigureAwait(false)).ToArray();

    public Task<OrganizationSubscription?> GetByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default) =>
        dataAccess.QueryAsync<OrganizationSubscription>(new { OrganizationId = organizationId }, cancellationToken);

    public Task<OrganizationSubscription?> GetByStripeSubscriptionIdAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSubscriptionId);
        return dataAccess.QueryAsync<OrganizationSubscription>(
            new { StripeSubscriptionId = stripeSubscriptionId.Trim() },
            cancellationToken);
    }

    public async Task<bool> SaveAsync(
        OrganizationSubscription subscription,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        if (subscription.OrganizationId == Guid.Empty)
            throw new ArgumentException("Organization ID is required.", nameof(subscription));

        var utcNow = DateTime.UtcNow;
        subscription.CreatedUtc = subscription.CreatedUtc == default ? utcNow : subscription.CreatedUtc;
        subscription.UpdatedUtc = utcNow;
        var existing = await GetByOrganizationIdAsync(subscription.OrganizationId, cancellationToken).ConfigureAwait(false);
        var affected = existing is null
            ? await dataAccess.InsertAsync(subscription, cancellationToken).ConfigureAwait(false)
            : await dataAccess.UpdateAsync(subscription, cancellationToken).ConfigureAwait(false);
        return affected > 0;
    }
}
