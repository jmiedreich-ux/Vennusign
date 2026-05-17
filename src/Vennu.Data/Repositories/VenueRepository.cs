using Vennu.DataAccess;
using Vennu.Data.Models;

namespace Vennu.Data.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly ISqlDataAccess dataAccess;

    public VenueRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<Guid> CreateAsync(Venue venue, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(venue);

        if (venue.Id == Guid.Empty)
        {
            venue.Id = Guid.NewGuid();
        }

        var utcNow = DateTime.UtcNow;
        venue.CreatedUtc = venue.CreatedUtc == default ? utcNow : venue.CreatedUtc;
        venue.UpdatedUtc = venue.UpdatedUtc == default ? utcNow : venue.UpdatedUtc;

        await dataAccess.InsertAsync(venue, cancellationToken).ConfigureAwait(false);
        return venue.Id;
    }

    public async Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Venue> venues = (await dataAccess.QueryAllAsync<Venue>(cancellationToken).ConfigureAwait(false)).ToArray();
        return venues;
    }

    public Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        return dataAccess.QueryAsync<Venue>(new { Id = venueId }, cancellationToken);
    }
}
