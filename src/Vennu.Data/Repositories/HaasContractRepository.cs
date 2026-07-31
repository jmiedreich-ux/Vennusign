using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class HaasContractRepository(ISqlDataAccess dataAccess) : IHaasContractRepository
{
    public async Task<HaasContract?> GetCurrentByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        (await dataAccess.ExecuteSqlQueryAsync<HaasContract, object>(
            "SELECT TOP (1) * FROM dbo.HaasContracts WHERE VenueId = @VenueId ORDER BY StartedUtc DESC;",
            new { VenueId = venueId },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();

    public Task<HaasContract?> GetByStripeSubscriptionIdAsync(
        string stripeSubscriptionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSubscriptionId);
        return dataAccess.QueryAsync<HaasContract>(
            new { StripeSubscriptionId = stripeSubscriptionId.Trim() },
            cancellationToken);
    }

    public async Task<bool> SaveAsync(HaasContract contract, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contract);
        var utcNow = DateTime.UtcNow;
        contract.Id = contract.Id == Guid.Empty ? Guid.NewGuid() : contract.Id;
        contract.CreatedUtc = contract.CreatedUtc == default ? utcNow : contract.CreatedUtc;
        contract.UpdatedUtc = utcNow;

        var existing = await dataAccess.QueryAsync<HaasContract>(new { contract.Id }, cancellationToken)
            .ConfigureAwait(false);
        var affected = existing is null
            ? await dataAccess.InsertAsync(contract, cancellationToken).ConfigureAwait(false)
            : await dataAccess.UpdateAsync(contract, cancellationToken).ConfigureAwait(false);
        return affected > 0;
    }
}
