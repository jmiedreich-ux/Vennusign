using System.Globalization;
using Vennu.Api.Contracts.Display;
using Vennu.Api.Services;
using Vennu.Api.Controllers;
using Vennu.Api.Tests.TestDoubles;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Controllers;

[Trait("Category", "Unit")]
public class DisplayControllerTests
{
    /// <summary>
    /// A content repository whose latest published snapshot IS the seeded menu.
    /// The display serves the published snapshot, never the builder's live tables
    /// (menus decisions 1, 2 and 38), so a case that wants content on a screen has
    /// to publish it first - exactly as a venue would.
    /// </summary>
    private static FakeContentRepository Published(Guid venueId, Guid menuId, FakeMenuRepository menus)
    {
        var snapshot = new MenuSnapshot
        {
            Sections = menus.Sections.Select(section => new SnapshotSection
            {
                SectionId = section.Id,
                PageId = section.PageId,
                Name = section.Name,
                SortOrder = section.SortOrder,
                Items = menus.Items
                    .Where(item => item.MenuSectionId == section.Id)
                    .Select(item => new SnapshotItem
                    {
                        ItemId = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price.ToString(CultureInfo.InvariantCulture),
                        SortOrder = item.SortOrder
                    }).ToList()
            }).ToList()
        };

        var content = new FakeContentRepository();
        content.PublishEvents.Add(new MenuPublishEvent
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            MenuId = menuId,
            Version = 1,
            PublishedUtc = DateTime.UtcNow,
            Snapshot = MenuSnapshot.Serialize(snapshot)
        });
        return content;
    }

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
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Mains" }],
            Items = Enumerable.Range(1, 8).Select(index => new MenuItem
            {
                Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = sectionId,
                Name = $"Item {index}", SortOrder = index
            }).ToArray()
        };
        var sut = new DisplayController(screenRepository, new FakeVenueRepository(), menuRepository,
            contentRepository: Published(venueId, menuId, menuRepository));

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
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, Name = "Food" }],
            Items = Enumerable.Range(1, 12)
                .Select(index => new MenuItem
                {
                    Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = sectionId,
                    Name = $"Item {index}", SortOrder = index
                })
                .ToArray()
        };
        var sut = new DisplayController(screenRepository, new FakeVenueRepository(), menuRepository,
            contentRepository: Published(venueId, menuId, menuRepository));

        var result = await sut.GetContent(secondScreen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayContentResponse>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("3x2", response.PhotoGridDensity);
        Assert.Equal(["Item 5", "Item 6", "Item 7", "Item 8", "Item 9", "Item 10"],
            Assert.Single(response.Sections).Items.Select(item => item.Name));
        Assert.Equal(2, response.PhotoGridOverflowItems);
    }

    /// <summary>
    /// #960: Mana-Thai Cuisine's Appetizers page had 13 items on a 6-item (3x2) screen. Only
    /// the first 6 ever reached a guest; the other 7 - and the page's other section, had it had
    /// one - were permanently unreachable behind a "7 more items" footer nothing ever advanced.
    /// A page too big for one screen now becomes more than one screen, shown in turn.
    /// </summary>
    [Fact]
    public async Task GetContent_SplitsAnOversizedPageIntoVirtualPagesInsteadOfTruncating()
    {
        var venueId = Guid.NewGuid();
        var menuId = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var screen = new Screen { Id = Guid.NewGuid(), VenueId = venueId, PhotoGridDensity = "3x2" };
        var screenRepository = new FakeScreenRepository
        {
            GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen)
        };
        var menuRepository = new FakeMenuRepository
        {
            Menus = [new Menu { Id = menuId, VenueId = venueId, Name = "Mana-Thai Cuisine", IsActive = true }],
            Sections = [new MenuSection { Id = sectionId, VenueId = venueId, MenuId = menuId, PageId = pageId, Name = "Appetizers" }],
            Items = Enumerable.Range(1, 13)
                .Select(index => new MenuItem
                {
                    Id = Guid.NewGuid(), VenueId = venueId, MenuSectionId = sectionId,
                    Name = $"Item {index}", SortOrder = index
                })
                .ToArray()
        };

        var snapshot = new MenuSnapshot
        {
            DwellSeconds = 12,
            Pages = [new SnapshotPage { PageId = pageId, Name = "Appetizers", SortOrder = 1 }],
            Sections = menuRepository.Sections.Select(section => new SnapshotSection
            {
                SectionId = section.Id,
                PageId = section.PageId,
                Name = section.Name,
                SortOrder = section.SortOrder,
                Items = menuRepository.Items
                    .Where(item => item.MenuSectionId == section.Id)
                    .Select(item => new SnapshotItem
                    {
                        ItemId = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price.ToString(CultureInfo.InvariantCulture),
                        SortOrder = item.SortOrder
                    }).ToList()
            }).ToList()
        };
        var content = new FakeContentRepository();
        content.PublishEvents.Add(new MenuPublishEvent
        {
            Id = Guid.NewGuid(), VenueId = venueId, MenuId = menuId, Version = 1,
            PublishedUtc = DateTime.UtcNow, Snapshot = MenuSnapshot.Serialize(snapshot)
        });
        content.Assignments.Add(new MenuScreenAssignment
        {
            Id = Guid.NewGuid(), VenueId = venueId, ScreenId = screen.Id, MenuId = menuId, PageId = pageId
        });

        var sut = new DisplayController(screenRepository, new FakeVenueRepository(), menuRepository, contentRepository: content);

        var result = await sut.GetContent(screen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayContentResponse>(
            Assert.IsType<OkObjectResult>(result.Result).Value);

        // The legacy single-screen fields still only ever show screenful one - a player old
        // enough to ignore Pages truly cannot show anything past this, so it deserves to be
        // told the truth about what it is not showing, exactly as before this fix.
        Assert.Equal(6, Assert.Single(response.Sections).Items.Count);
        Assert.Equal(7, response.PhotoGridOverflowItems);

        // A rotation-aware player gets every item across three virtual screens, none of them
        // reporting a stale or borrowed overflow count.
        Assert.Equal(3, response.Pages.Count);
        Assert.All(response.Pages, page => Assert.Equal(0, page.PhotoGridOverflowItems));
        Assert.Equal(
            new[] { 6, 6, 1 },
            response.Pages.Select(page => Assert.Single(page.Sections).Items.Count));
        Assert.Equal(
            Enumerable.Range(1, 13).Select(index => $"Item {index}"),
            response.Pages.SelectMany(page => page.Sections).SelectMany(section => section.Items).Select(item => item.Name));
        Assert.Equal(12, response.PageDwellSeconds);
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
                new MenuSection { Id = activeSectionId, VenueId = venueId, MenuId = menuId, Name = "Bowls", SortOrder = 1 }
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
        var sut = new DisplayController(screenRepository, venueRepository, menuRepository,
            contentRepository: Published(venueId, menuId, menuRepository));

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

    [Fact]
    public async Task Heartbeat_RecordsGoLive_OnlyForAnOnlineReportOfAJourneysFirstScreen()
    {
        var screenId = Guid.NewGuid();
        var lastSeenUtc = default(DateTime);
        var screenRepository = new FakeScreenRepository
        {
            UpdateHeartbeatAsyncHandler = (_, seen, _, _) => { lastSeenUtc = seen; return Task.FromResult(true); }
        };
        var onboarding = new GoLiveLatchFake();
        var sut = new DisplayController(screenRepository, new FakeVenueRepository(), new FakeMenuRepository(), customerOnboarding: onboarding);

        // An offline or degraded report is not the moment a customer goes live.
        await sut.Heartbeat(screenId, new ScreenHeartbeatRequest { Status = "Offline" }, CancellationToken.None);
        Assert.Empty(onboarding.Calls);

        // The Online report is, and it carries the same timestamp written to the screen.
        await sut.Heartbeat(screenId, new ScreenHeartbeatRequest { Status = "  online  " }, CancellationToken.None);
        var call = Assert.Single(onboarding.Calls);
        Assert.Equal(screenId, call.ScreenId);
        Assert.Equal(lastSeenUtc, call.AchievedUtc);
    }

    [Fact]
    public async Task Heartbeat_ForAnUnknownScreen_DoesNotRecordGoLive()
    {
        var screenRepository = new FakeScreenRepository
        {
            UpdateHeartbeatAsyncHandler = (_, _, _, _) => Task.FromResult(false)
        };
        var onboarding = new GoLiveLatchFake();
        var sut = new DisplayController(screenRepository, new FakeVenueRepository(), new FakeMenuRepository(), customerOnboarding: onboarding);

        var result = await sut.Heartbeat(Guid.NewGuid(), new ScreenHeartbeatRequest { Status = "Online" }, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Empty(onboarding.Calls);
    }

    private sealed class GoLiveLatchFake : ICustomerOnboardingRepository
    {
        public List<(Guid ScreenId, DateTime AchievedUtc)> Calls { get; } = [];
        public Task<CustomerOnboardingState?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState> SaveAsync(CustomerOnboardingState state, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState?> GetByFirstScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState?> LatchGoLiveByFirstScreenAsync(Guid screenId, DateTime achievedUtc, CancellationToken cancellationToken = default)
        {
            Calls.Add((screenId, achievedUtc));
            return Task.FromResult<CustomerOnboardingState?>(null);
        }
    }

    [Fact]
    public async Task GetContent_AndReceipt_UseSameAuthoritativeRevision()
    {
        var screen = new Screen { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), ScreenKey = "ABC123XYZ" };
        var delivery = new DeliveryFake(new(screen.Id, screen.VenueId.Value, 7, 6, "Requested", DateTime.UtcNow));
        var screens = new FakeScreenRepository { GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen) };
        var sut = new DisplayController(screens, new FakeVenueRepository(), new FakeMenuRepository(), deliveryService: delivery);

        var content = Assert.IsType<DisplayContentResponse>(Assert.IsType<OkObjectResult>((await sut.GetContent(screen.Id, default)).Result).Value);
        var receipt = await sut.ContentReceipt(screen.Id, new ScreenContentReceiptRequest
        {
            Revision = 7, State = "Applied", ScreenKey = screen.ScreenKey, PlayerVersion = "1.7.0", ShellVersion = "2.1.0", Platform = "tizen"
        }, default);

        Assert.Equal(7, content.ContentRevision);
        Assert.Equal(7, Assert.IsType<ScreenContentReceiptResponse>(Assert.IsType<OkObjectResult>(receipt.Result).Value).AppliedRevision);
        Assert.Equal(screen.ScreenKey, delivery.Receipt?.ScreenKey);
    }

    [Fact]
    public async Task ContentReceipt_ReturnsNotFoundForRejectedIdentityOrRevision()
    {
        var sut = new DisplayController(new FakeScreenRepository(), new FakeVenueRepository(), new FakeMenuRepository(), deliveryService: new DeliveryFake(null));
        var result = await sut.ContentReceipt(Guid.NewGuid(), new ScreenContentReceiptRequest { Revision = 99, State = "Applied", ScreenKey = "FORGEDKEY" }, default);
        Assert.IsType<NotFoundResult>(result.Result);
    }

    private static DisplayController CreateController(IScreenRepository screenRepository) =>
        new(screenRepository, new FakeVenueRepository(), new FakeMenuRepository());

    [Fact]
    public async Task GetDiagnostics_ReturnsNotFound_WhenScreenDoesNotExist()
    {
        var sut = CreateController(new FakeScreenRepository());

        var result = await sut.GetDiagnostics(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDiagnostics_ReportsIdentityStalenessAndDeliveryState_WithoutContent()
    {
        var now = new DateTimeOffset(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);
        var venueId = Guid.NewGuid();
        var screen = new Screen
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            ScreenKey = "ABC123XYZ",
            Name = "Bar Screen",
            Status = "Online",
            LastSeen = now.UtcDateTime.AddSeconds(-120),
            Platform = "tizen",
            AppVersion = "2.4.0",
            WidthPixels = 1920,
            HeightPixels = 1080
        };
        var screens = new FakeScreenRepository { GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen) };
        var delivery = new DeliveryFake(new(screen.Id, venueId, 7, null, "Requested", now.UtcDateTime.AddSeconds(-10))
        {
            PlayerVersion = "1.7.0",
            ShellVersion = "2.1.0"
        });
        var sut = new DisplayController(
            screens, new FakeVenueRepository(), new FakeMenuRepository(),
            timeProvider: new FixedTimeProvider(now),
            deliveryService: delivery);

        var result = await sut.GetDiagnostics(screen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayDiagnosticsResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(screen.Id, response.ScreenId);
        Assert.Equal(venueId, response.VenueId);
        Assert.True(response.IsAssignedToVenue);
        Assert.Equal(120, response.SecondsSinceLastSeen);
        // 120 seconds since the last heartbeat is past the 90-second default threshold, so this
        // screen is stale even though its own Status column still reads "Online" -- the same gap
        // that made a screen the API called Offline look fine to the player it was up against.
        Assert.True(response.IsStale);
        Assert.Equal(7, response.AuthoritativeRevision);
        Assert.Null(response.AppliedRevision);
        Assert.Equal("Requested", response.DeliveryState);
        Assert.Equal("1.7.0", response.LastReceiptPlayerVersion);
        Assert.False(response.IsOnboardingFirstScreen);

        var json = System.Text.Json.JsonSerializer.Serialize(response);
        Assert.DoesNotContain("Weekday", json, StringComparison.Ordinal);
        Assert.DoesNotContain("theme", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("section", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetDiagnostics_ReportsOnboardingFirstScreen_WhenAJourneyNamesIt()
    {
        var screen = new Screen { Id = Guid.NewGuid(), VenueId = Guid.NewGuid(), Status = "Online" };
        var screens = new FakeScreenRepository { GetByIdAsyncHandler = (_, _) => Task.FromResult<Screen?>(screen) };
        var achievedUtc = new DateTime(2026, 8, 20, 4, 57, 7, DateTimeKind.Utc);
        var onboarding = new OnboardingLookupFake(new CustomerOnboardingState { UserId = Guid.NewGuid(), FirstScreenId = screen.Id, GoLiveAchievedUtc = achievedUtc });
        var sut = new DisplayController(screens, new FakeVenueRepository(), new FakeMenuRepository(), customerOnboarding: onboarding);

        var result = await sut.GetDiagnostics(screen.Id, CancellationToken.None);

        var response = Assert.IsType<DisplayDiagnosticsResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.True(response.IsOnboardingFirstScreen);
        Assert.Equal(achievedUtc, response.OnboardingGoLiveAchievedUtc);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class OnboardingLookupFake(CustomerOnboardingState state) : ICustomerOnboardingRepository
    {
        public Task<CustomerOnboardingState?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState> SaveAsync(CustomerOnboardingState state, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState?> LatchGoLiveByFirstScreenAsync(Guid screenId, DateTime achievedUtc, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<CustomerOnboardingState?> GetByFirstScreenIdAsync(Guid screenId, CancellationToken cancellationToken = default) =>
            Task.FromResult(state.FirstScreenId == screenId ? state : null);
    }

    private sealed class ThemeRepository(VenueTheme theme) : IVenueThemeRepository
    {
        public Task<VenueTheme?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult(theme.VenueId == venueId ? theme : null);

        public Task UpsertAsync(VenueTheme value, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class DeliveryFake(ScreenContentDelivery? value) : IScreenContentDeliveryService
    {
        public ScreenContentReceipt? Receipt { get; private set; }
        public Task<ScreenContentDelivery?> IssueAsync(Guid venueId, Guid screenId, CancellationToken cancellationToken = default) => Task.FromResult(value);
        public Task<ScreenContentDelivery?> GetLatestAsync(Guid screenId, CancellationToken cancellationToken = default) => Task.FromResult(value);
        public Task<IReadOnlyDictionary<Guid, ScreenContentDelivery>> GetLatestByVenueAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyDictionary<Guid, ScreenContentDelivery>>(new Dictionary<Guid, ScreenContentDelivery>());
        public Task<ScreenContentDelivery?> AcknowledgeAsync(Guid screenId, ScreenContentReceipt receipt, CancellationToken cancellationToken = default)
        {
            Receipt = receipt;
            return Task.FromResult(value is null ? null : value with { AppliedRevision = receipt.Revision, State = receipt.Recovered ? "Recovered" : receipt.State, AppliedUtc = DateTime.UtcNow });
        }
    }
}
