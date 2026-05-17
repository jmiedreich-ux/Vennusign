#nullable enable
using RepoDb;
using Vennu.Data.Models;
using Vennu.Data.Repositories;
using Vennu.DataAccess.Infrastructure;

namespace Vennu.DataAccess.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly ISqlConnectionFactory connectionFactory;

    public VenueRepository(ISqlConnectionFactory connectionFactory) => this.connectionFactory = connectionFactory;

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

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await connection.InsertAsync(venue);
        return venue.Id;
    }

    public async Task<IReadOnlyCollection<Venue>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var venues = await connection.QueryAllAsync<Venue>();
        return venues.ToArray();
    }

    public async Task<Venue?> GetByIdAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var venues = await connection.QueryAsync<Venue>(new { Id = venueId });
        return venues.FirstOrDefault();
    }
}
