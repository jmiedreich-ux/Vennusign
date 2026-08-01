using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IPosCatalogMappingRepository
{
    Task<IReadOnlyCollection<PosCatalogMapping>> GetAllAsync(
        Guid venueId,
        PosProvider provider,
        CancellationToken cancellationToken = default);

    Task<PosCatalogMapping> SaveAsync(
        Guid venueId,
        PosCatalogMapping mapping,
        CancellationToken cancellationToken = default);
}
