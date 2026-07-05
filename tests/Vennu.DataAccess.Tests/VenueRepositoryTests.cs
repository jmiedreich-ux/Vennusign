using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public class VenueRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateAsync_AssignsIdentityAndTimestamps()
    {
        var dataAccess = new FakeSqlDataAccess();
        var sut = new VenueRepository(dataAccess);
        var venue = new Venue { Name = "Test Venue" };
        using var cancellationSource = new CancellationTokenSource();

        var venueId = await sut.CreateAsync(venue, cancellationSource.Token);

        Assert.NotEqual(Guid.Empty, venueId);
        Assert.Equal(venueId, venue.Id);
        Assert.NotEqual(default, venue.CreatedUtc);
        Assert.NotEqual(default, venue.UpdatedUtc);
        Assert.Single(dataAccess.InsertedEntities);
        Assert.Same(venue, dataAccess.InsertedEntities[0]);
        Assert.Equal(cancellationSource.Token, dataAccess.LastCancellationToken);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetAllAsync_ReturnsProjectedResults()
    {
        var expectedVenues = new[]
        {
            new Venue { Name = "One" },
            new Venue { Name = "Two" }
        };

        var dataAccess = new FakeSqlDataAccess
        {
            QueryAllHandler = type => type == typeof(Venue) ? expectedVenues : []
        };

        var sut = new VenueRepository(dataAccess);

        var venues = await sut.GetAllAsync();

        Assert.Equal(2, venues.Count);
        Assert.Equal(expectedVenues[0], venues.First());
        Assert.Equal(expectedVenues[1], venues.Last());
    }
}
