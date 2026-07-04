using Vennu.Api.Contracts.Venues;
using Vennu.Api.Controllers;
using Vennu.Api.Tests.TestDoubles;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public class VenuesControllerTests
{
    [Fact]
    public async Task CreateVenue_ReturnsCreated_AndTrimsValues()
    {
        var venueRepository = new FakeVenueRepository();
        var sut = new VenuesController(venueRepository);
        var request = new CreateVenueRequest
        {
            Name = "  Venue Name  ",
            Timezone = "  America/New_York  ",
            Type = "  Bar  ",
            PrimaryLanguage = "  en  ",
            SecondaryLanguage = "  es  "
        };

        var result = await sut.CreateVenue(request, CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
        var response = Assert.IsType<CreateVenueResponse>(created.Value);
        Assert.NotEqual(Guid.Empty, response.VenueId);
        Assert.Equal($"/api/venues/{response.VenueId}", created.Location);
        Assert.NotNull(venueRepository.LastCreatedVenue);
        Assert.Equal("Venue Name", venueRepository.LastCreatedVenue!.Name);
        Assert.Equal("America/New_York", venueRepository.LastCreatedVenue.Timezone);
        Assert.Equal("Bar", venueRepository.LastCreatedVenue.Type);
        Assert.Equal("en", venueRepository.LastCreatedVenue.PrimaryLanguage);
        Assert.Equal("es", venueRepository.LastCreatedVenue.SecondaryLanguage);
    }

    [Fact]
    public async Task CreateVenue_SetsSecondaryLanguageToNull_WhenWhitespace()
    {
        var venueRepository = new FakeVenueRepository();
        var sut = new VenuesController(venueRepository);
        var request = new CreateVenueRequest
        {
            Name = "Venue",
            Timezone = "UTC",
            Type = "Bar",
            PrimaryLanguage = "en",
            SecondaryLanguage = "   "
        };

        _ = await sut.CreateVenue(request, CancellationToken.None);

        Assert.NotNull(venueRepository.LastCreatedVenue);
        Assert.Null(venueRepository.LastCreatedVenue!.SecondaryLanguage);
    }
}
