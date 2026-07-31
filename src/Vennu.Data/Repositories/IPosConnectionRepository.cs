using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IPosConnectionRepository
{
    Task<PosConnection?> GetAsync(
        Guid venueId,
        PosProvider provider,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<PosConnection> SaveAsync(
        Guid venueId,
        PosConnection connection,
        CancellationToken cancellationToken = default);
}
