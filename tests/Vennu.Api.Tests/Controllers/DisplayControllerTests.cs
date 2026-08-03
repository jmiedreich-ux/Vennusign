using Vennu.Api.Contracts.Display;
using Vennu.Api.Controllers;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public class DisplayControllerTests
{
    [Fact]
    public async Task GetContent_IncludesAuthoritativeHappyHourState()
    {
        var venueId = Guid.NewGuid();
        var screen = new Screen { Id = Guid.NewGuid(), VenueId = venueId };
        var screens = new FakeScreenRepository { GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen) };
        var happyHour = new HappyHourFake();
        var sut = new DisplayController(screens, new FakeVenueRepository(), new FakeMenuRepository(), null, happyHour);

        var result = await sut.GetContent(screen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayContentResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.True(response.IsHappyHour);
        Assert.Equal(HappyHourOverrideMode.Automatic, response.HappyHourMode);
        Assert.NotNull(response.HappyHourEndsAtUtc);
    }
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
    public async Task GetContent_ReturnsGone_WhenScreenIsArchived()
    {
        var screen = new Screen { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Status = "Archived" };
        var screens = new FakeScreenRepository { GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen) };
        var sut = CreateController(screens);

        var result = await sut.GetContent(screen.Id, CancellationToken.None);

        var gone = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status410Gone, gone.StatusCode);
    }

    private sealed class HappyHourFake : IHappyHourService
    {
        public Task<HappyHourSnapshot> GetAsync(Guid venueId, DateTimeOffset utcNow, CancellationToken cancellationToken = default) =>
            Task.FromResult(new HappyHourSnapshot(null, new HappyHourResolution(true, utcNow, utcNow.AddHours(1), HappyHourOverrideMode.Automatic), true));
        public Task<HappyHourSnapshot> UpdateAsync(Guid venueId, TimeSpan startLocalTime, TimeSpan endLocalTime, int activeDaysMask, bool isEnabled, string overrideMode, DateTimeOffset utcNow, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    [Fact]
    public async Task GetContent_ReturnsAllSectionsForClassicDiner()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var screen = new Screen
        {
            Id = Guid.NewGuid(), VenueId = venueId, DisplayLayout = "classic_diner"
        };
        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen)
        };
        var menuRepository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Dinner", DailySpecial = "Chicken pot pie", IsActive = true }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Mains", IsActive = true }],
            Items = Enumerable.Range(1, 8).Select(index => new MenuItem
            {
                Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = sectionId,
                Name = $"Item {index}", SortOrder = index
            }).ToArray()
        };
        var sut = new DisplayController(screenRepository, new FakeVenueRepository(), menuRepository);

        var result = await sut.GetContent(screen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayContentResponse>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("classic_diner", response.Layout);
        Assert.Equal("Chicken pot pie", response.DailySpecial);
        Assert.Equal(8, Assert.Single(response.Sections).Items.Count);
        Assert.Equal(0, response.PhotoGridOverflowItems);
    }

    [Fact]
    public async Task GetContent_SlicesVideoWallByPriorScreenCapacity_AndBoundsOverflow()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var firstScreen = new Screen
        {
            Id = Guid.NewGuid(), VenueId = venueId, WallGroup = "Main",
            WallPosition = 1, PhotoGridDensity = "2x2"
        };
        var secondScreen = new Screen
        {
            Id = Guid.NewGuid(), VenueId = venueId, WallGroup = "Main",
            WallPosition = 2, PhotoGridDensity = "3x2"
        };
        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(secondScreen),
            GetByVenueIdAsyncHandler = (_, _) => Task.FromResult<IReadOnlyCollection<Screen>>([secondScreen, firstScreen])
        };
        var menuRepository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Main", IsActive = true }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food", IsActive = true }],
            Items = Enumerable.Range(1, 12)
                .Select(index => new MenuItem
                {
                    Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = sectionId,
                    Name = $"Item {index}", SortOrder = index
                })
                .ToArray()
        };
        var sut = new DisplayController(screenRepository, new FakeVenueRepository(), menuRepository);

        var result = await sut.GetContent(secondScreen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayContentResponse>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("3x2", response.PhotoGridDensity);
        Assert.Equal(["Item 5", "Item 6", "Item 7", "Item 8", "Item 9", "Item 10"],
            Assert.Single(response.Sections).Items.Select(item => item.Name));
        Assert.Equal(2, response.PhotoGridOverflowItems);
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

    [Theory]
    [InlineData("classic_chalkboard")]
    [InlineData("tap_strips")]
    [InlineData("digital_tap_board")]
    public async Task GetContent_ReturnsTapLayoutData_WithoutAnActiveMenu(string layout)
    {
        var venueId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var screen = new Screen
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            DisplayLayout = layout
        };
        var screens = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen)
        };
        var taps = new FakeTapListRepository
        {
            Categories =
            [
                new TapCategory { Id = categoryId, VenueId = venueId, Name = "Draft Beer", CategoryPrice = 7m, IsActive = true }
            ],
            Items =
            [
                new TapItem
                {
                    Id = Guid.NewGuid(), VenueId = venueId, TapCategoryId = categoryId,
                    Name = "House Lager", Price = 7m, IsAvailable = false
                }
            ]
        };
        var sut = new DisplayController(
            screens,
            new FakeVenueRepository(),
            new FakeMenuRepository(),
            tapListRepository: taps);

        var result = await sut.GetContent(screen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayContentResponse>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(layout, response.Layout);
        Assert.Equal("Draft Beer", Assert.Single(response.TapCategories).Name);
        var item = Assert.Single(response.TapItems);
        Assert.Equal("House Lager", item.Name);
        Assert.False(item.IsAvailable);
        Assert.Empty(response.Sections);
    }

    [Fact]
    public async Task GetContent_IncludesPersistedVenueTheme()
    {
        var venueId = Guid.NewGuid();
        var screen = new Screen { Id = Guid.NewGuid(), VenueId = venueId };
        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen)
        };
        var themes = new ThemeRepository(new VenueTheme
        {
            VenueId = venueId,
            BackgroundColor = "#102030",
            AccentColor = "#ABCDEF",
            FontFamily = "Georgia",
            PresetKey = "violet_lounge",
            TitleColor = "#F5E9FF",
            GlowColor = "#A855F7",
            BoardBackgroundColor = "#12091C",
            SectionColors = "#C084FC,#F472B6",
            GlowIntensity = 1.15m,
            TitleFont = "Pacifico",
            ItemFont = "Kalam"
        });
        var sut = new DisplayController(
            screenRepository,
            new FakeVenueRepository(),
            new FakeMenuRepository(),
            themes);

        var result = await sut.GetContent(screen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayContentResponse>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("#102030", response.Theme.BackgroundColor);
        Assert.Equal("#ABCDEF", response.Theme.AccentColor);
        Assert.Equal("Georgia", response.Theme.FontFamily);
        Assert.Equal("violet_lounge", response.Theme.PresetKey);
        Assert.Equal("#A855F7", response.Theme.GlowColor);
        Assert.Equal(["#C084FC", "#F472B6"], response.Theme.SectionColors);
        Assert.Equal(1.15m, response.Theme.GlowIntensity);
        Assert.Equal("Pacifico", response.Theme.TitleFont);
        Assert.Equal("Kalam", response.Theme.ItemFont);
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
                new MenuItem
                {
                    Id = Guid.NewGuid(),
                    VenueId = venueId,
                    MenuSectionId = activeSectionId,
                    Name = "First",
                    Description = "Seasonal vegetables",
                    Price = 12.5m,
                    HappyHourPrice = 10m,
                    QuantityAvailable = 3,
                    IsPopular = true,
                    Tags = "Vegan, GF, vegan",
                    ImageUrl = "https://cdn.example/first.jpg",
                    SortOrder = 1
                }
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
        var first = section.Items.First();
        Assert.Equal("https://cdn.example/first.jpg", first.ImageUrl);
        Assert.Equal(10m, first.HappyHourPrice);
        Assert.Equal(3, first.QuantityAvailable);
        Assert.True(first.IsPopular);
        Assert.Equal(new[] { "Vegan", "GF" }, first.Tags);
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

    private sealed class ThemeRepository(VenueTheme theme) : IVenueThemeRepository
    {
        public Task<VenueTheme?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(theme.VenueId == venueId ? theme : null);

        public Task UpsertAsync(VenueTheme value, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
