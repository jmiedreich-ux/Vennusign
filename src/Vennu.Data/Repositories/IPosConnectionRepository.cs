using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IPosConnectionRepository
{
    Task<PosConnection?> GetAsync(
        Guid venueId,
        PosProvider provider,
        CancellationToken cancellationToken = default);

    Task<PosConnection?> GetByExternalMerchantIdAsync(
        PosProvider provider,
        string externalMerchantId,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PosConnection>> GetAllByProviderAsync(
        PosProvider provider,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    Task<PosConnection> SaveAsync(
        Guid venueId,
        PosConnection connection,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid venueId,
        PosProvider provider,
        CancellationToken cancellationToken = default);
}
