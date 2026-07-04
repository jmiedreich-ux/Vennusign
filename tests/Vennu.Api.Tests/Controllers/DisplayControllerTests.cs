using Vennu.Api.Contracts.Display;
using Vennu.Api.Controllers;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Data.Models;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public class DisplayControllerTests
{
    [Fact]
    public async Task GetContent_ReturnsNotFound_WhenScreenDoesNotExist()
    {
        var screenRepository = new FakeScreenRepository();
        var sut = new DisplayController(screenRepository);

        var result = await sut.GetContent(Guid.NewGuid(), CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task GetContent_ReturnsScreenContent_WhenScreenExists()
    {
        var screenId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var screen = new Screen
        {
            Id = screenId,
            VenueId = venueId,
            ScreenKey = "sc-abc123",
            Name = "North Wall",
            Status = "Online",
            LastSeen = DateTime.UtcNow
        };

        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen)
        };

        var sut = new DisplayController(screenRepository);

        var result = await sut.GetContent(screenId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<DisplayContentResponse>(ok.Value);
        Assert.Equal(screenId, response.ScreenId);
        Assert.Equal(venueId, response.VenueId);
        Assert.Equal("sc-abc123", response.ScreenKey);
        Assert.Equal("North Wall", response.ScreenName);
        Assert.Equal("Online", response.Status);
    }

    [Fact]
    public async Task Heartbeat_ReturnsNotFound_WhenScreenMissing()
    {
        var screenRepository = new FakeScreenRepository
        {
            UpdateHeartbeatAsyncHandler = (_, _, _, _) => Task.FromResult(false)
        };

        var sut = new DisplayController(screenRepository);

        var result = await sut.Heartbeat(Guid.NewGuid(), new ScreenHeartbeatRequest { Status = "Online" }, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [Fact]
    public async Task Heartbeat_ReturnsOk_AndTrimsStatus()
    {
        var capturedStatus = string.Empty;
        var screenId = Guid.NewGuid();
        var screenRepository = new FakeScreenRepository
        {
            UpdateHeartbeatAsyncHandler = (_, _, status, _) =>
            {
                capturedStatus = status;
                return Task.FromResult(true);
            }
        };

        var sut = new DisplayController(screenRepository);
        var request = new ScreenHeartbeatRequest { Status = "  Online  " };

        var result = await sut.Heartbeat(screenId, request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ScreenHeartbeatResponse>(ok.Value);
        Assert.Equal(screenId, response.ScreenId);
        Assert.Equal("Online", response.Status);
        Assert.Equal("Online", capturedStatus);
    }
}
