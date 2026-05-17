using Vennu.Data.Models;

namespace Vennu.Data.Repositories;

public interface IVenueRepository
{
    Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default);
}
