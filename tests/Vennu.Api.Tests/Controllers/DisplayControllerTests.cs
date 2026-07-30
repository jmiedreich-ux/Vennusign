using Vennu.Api.Contracts.Display;
using Vennu.Api.Controllers;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public class DisplayControllerTests
{
    [Fact]
    public async Task GetContent_ReturnsNotFound_WhenScreenDoesNotExist()
    {
        var screenRepository = new FakeScreenRepository();
        var sut = CreateController(screenRepository);

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

        var sut = CreateController(screenRepository);

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
    public async Task GetContent_ReturnsActiveMenuAsPhotoGrid_InStableOrder()
    {
        var screenId = Guid.NewGuid();
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var activeSectionId = Guid.NewGuid();
        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(new Screen { Id = screenId, VenueId = venueId })
        };
        var venueRepository = new FakeVenueRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Venue?>(new Venue { Id = venueId, Name = "Juniper Cafe" })
        };
        var menuRepository = new FakeMenuRepository
        {
            Menus =
            [
                new Menu { Id = Guid.NewGuid(), VenueId = venueId, Name = "Old", IsActive = false },
                new Menu { Id = menuId, VenueId = venueId, Name = "Lunch", IsActive = true }
            ],
            Sections =
            [
                new MenuSection { Id = Guid.NewGuid(), VenueId = venueId, MenuId = menuId, Name = "Hidden", IsActive = false, SortOrder = 0 },
                new MenuSection { Id = activeSectionId, VenueId = venueId, MenuId = menuId, Name = "Bowls", IsActive = true, SortOrder = 1 }
            ],
            Items =
            [
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = activeSectionId, Name = "Second", Price = 14m, SortOrder = 2 },
                new MenuItem { Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = activeSectionId, Name = "First", Description = "Seasonal vegetables", Price = 12.5m, ImageUrl = "https://cdn.example/first.jpg", SortOrder = 1 }
            ]
        };
        var sut = new DisplayController(screenRepository, venueRepository, menuRepository);

        var result = await sut.GetContent(screenId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<DisplayContentResponse>(ok.Value);
        Assert.Equal("photo_grid", response.Layout);
        Assert.Equal("Juniper Cafe", response.VenueName);
        Assert.Equal("Lunch", response.MenuName);
        var section = Assert.Single(response.Sections);
        Assert.Equal("Bowls", section.Name);
        Assert.Equal(new[] { "First", "Second" }, section.Items.Select(item => item.Name));
        Assert.Equal("https://cdn.example/first.jpg", section.Items.First().ImageUrl);
    }

    [Fact]
    public async Task Heartbeat_ReturnsNotFound_WhenScreenMissing()
    {
        var screenRepository = new FakeScreenRepository
        {
            UpdateHeartbeatAsyncHandler = (_, _, _, _) => Task.FromResult(false)
        };

        var sut = CreateController(screenRepository);

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

        var sut = CreateController(screenRepository);
        var request = new ScreenHeartbeatRequest { Status = "  Online  " };

        var result = await sut.Heartbeat(screenId, request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ScreenHeartbeatResponse>(ok.Value);
        Assert.Equal(screenId, response.ScreenId);
        Assert.Equal("Online", response.Status);
        Assert.Equal("Online", capturedStatus);
    }

    private static DisplayController CreateController(IScreenRepository screenRepository) =>
        new(screenRepository, new FakeVenueRepository(), new FakeMenuRepository());
}
