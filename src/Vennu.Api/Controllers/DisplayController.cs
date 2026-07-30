using Microsoft.AspNetCore.Mvc;
using Vennu.Api.Contracts.Display;
using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/display")]
public class DisplayController : ControllerBase
{
    private readonly IScreenRepository screenRepository;
    private readonly IVenueRepository venueRepository;
    private readonly IMenuRepository menuRepository;
    private readonly IVenueThemeRepository? themeRepository;
    private readonly IHappyHourService? happyHourService;
    private readonly IPlaylistAdministrationService? playlistService;
    private readonly IEmergencyBroadcastService? emergencyBroadcastService;
    private readonly IDateRangePromotionService? promotionService;
    private readonly ITapListRepository? tapListRepository;
    private readonly TimeProvider timeProvider;

    public DisplayController(
        IScreenRepository screenRepository,
        IVenueRepository venueRepository,
        IMenuRepository menuRepository,
        IVenueThemeRepository? themeRepository = null,
        IHappyHourService? happyHourService = null,
        IPlaylistAdministrationService? playlistService = null,
        IEmergencyBroadcastService? emergencyBroadcastService = null,
        TimeProvider? timeProvider = null,
        IDateRangePromotionService? promotionService = null,
        ITapListRepository? tapListRepository = null) =>
        (this.screenRepository, this.venueRepository, this.menuRepository, this.themeRepository, this.happyHourService, this.playlistService, this.emergencyBroadcastService, this.promotionService, this.tapListRepository, this.timeProvider) =
        (screenRepository, venueRepository, menuRepository, themeRepository, happyHourService, playlistService, emergencyBroadcastService, promotionService, tapListRepository, timeProvider ?? TimeProvider.System);

    [HttpGet("{screenId:guid}/content")]
    [ProducesResponseType<DisplayContentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayContentResponse>> GetContent(Guid screenId, CancellationToken cancellationToken)
    {
        var screen = await screenRepository.GetByIdAsync(screenId, cancellationToken);

        if (screen is null)
        {
            return NotFound(new ProblemDetails { Title = "Screen not found.", Detail = $"Screen '{screenId}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        var response = new DisplayContentResponse
        {
            ScreenId = screen.Id,
            VenueId = screen.VenueId,
            ScreenKey = screen.ScreenKey,
            ScreenName = screen.Name,
            Status = screen.Status,
            LastSeenUtc = screen.LastSeen,
            Layout = "default"
        };

        if (!screen.VenueId.HasValue)
        {
            return Ok(response);
        }

        var venueId = screen.VenueId.Value;
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken);
        if (promotionService is not null)
        {
            var promotion = await promotionService.GetActiveAsync(
                venueId, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
            response.Promotion = promotion is null ? null : DisplayPromotionResponse.From(promotion);
        }
        response.Layout = response.Promotion?.TargetLayout is { Length: > 0 } promotionLayout
            ? ScreenLayout.Normalize(promotionLayout)
            : ScreenLayout.Normalize(screen.DisplayLayout);
        response.SplitRatio = ScreenSplitRatio.Normalize(screen.SplitRatio);
        response.HeroDwellSeconds = HeroDwellSeconds.Normalize(screen.HeroDwellSeconds);
        if (emergencyBroadcastService is not null)
        {
            var broadcast = await emergencyBroadcastService.GetActiveAsync(
                venueId, screen.Id, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false);
            response.EmergencyBroadcast = broadcast is null ? null : DisplayEmergencyBroadcastResponse.From(broadcast);
        }
        if (playlistService is not null)
        {
            response.Playlist = (await playlistService.GetActiveAsync(
                venueId, screen.Id, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false))
                .Select(slide => new DisplayPlaylistSlideResponse
                {
                    Id = slide.Id, SlideType = slide.SlideType, Title = slide.Title,
                    Body = slide.Body, MediaUrl = slide.MediaUrl, DwellSeconds = slide.DwellSeconds
                }).ToArray();
        }
        if (happyHourService is not null)
        {
            var happyHour = (await happyHourService.GetAsync(
                venueId, timeProvider.GetUtcNow(), cancellationToken).ConfigureAwait(false)).State;
            response.IsHappyHour = happyHour.IsActive;
            response.HappyHourEndsAtUtc = happyHour.EndsAtUtc;
            response.HappyHourMode = happyHour.Mode;
        }
        var theme = themeRepository is null
            ? null
            : await themeRepository.GetByVenueIdAsync(venueId, cancellationToken);
        response.Theme = new DisplayThemeResponse
        {
            BackgroundColor = theme?.BackgroundColor ?? "#111315",
            AccentColor = theme?.AccentColor ?? "#FFB74D",
            FontFamily = theme?.FontFamily ?? "Inter",
            PresetKey = theme?.PresetKey ?? "bar_classic",
            TitleColor = theme?.TitleColor ?? "#F8F5E9",
            GlowColor = theme?.GlowColor ?? "#00E5FF",
            BoardBackgroundColor = theme?.BoardBackgroundColor ?? "#071013",
            SectionColors = (theme?.SectionColors ?? "#00E5FF,#FF2BD6,#FFE66D,#7CFF6B")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            GlowIntensity = theme?.GlowIntensity ?? 1.00m,
            TitleFont = theme?.TitleFont ?? "Righteous",
            ItemFont = theme?.ItemFont ?? "Caveat"
        };
        if (response.Layout is "classic_chalkboard" or "tap_strips" or "digital_tap_board")
        {
            if (tapListRepository is not null)
            {
                response.TapCategories = await tapListRepository.GetCategoriesAsync(venueId, cancellationToken);
                response.TapItems = await tapListRepository.GetItemsAsync(venueId, cancellationToken);
            }
            return Ok(response);
        }
        var menu = (await menuRepository.GetMenusAsync(venueId, cancellationToken))
            .FirstOrDefault(candidate => candidate.IsActive);

        response.VenueName = venue?.Name;
        response.MenuName = menu?.Name;
        response.DailySpecial = menu?.DailySpecial;
        if (menu is null)
        {
            return Ok(response);
        }

        var sections = (await menuRepository.GetSectionsAsync(venueId, menu.Id, cancellationToken))
            .Where(section => section.IsActive);
        var displaySections = new List<DisplayMenuSectionResponse>();
        foreach (var section in sections)
        {
            var items = await menuRepository.GetItemsAsync(venueId, section.Id, cancellationToken);
            displaySections.Add(new DisplayMenuSectionResponse
            {
                Id = section.Id,
                Name = section.Name,
                Items = items.Select(item => new DisplayMenuItemResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    HappyHourPrice = item.HappyHourPrice,
                    IsAvailable = item.IsAvailable,
                    QuantityAvailable = item.QuantityAvailable,
                    IsPopular = item.IsPopular,
                    Tags = ParseTags(item.Tags),
                    ImageUrl = item.ImageUrl
                }).ToArray()
            });
        }

        if (!string.Equals(response.Layout, ScreenLayout.Default, StringComparison.Ordinal))
        {
            response.Sections = displaySections;
            return Ok(response);
        }

        var wallScreens = await ResolveWallScreensAsync(screen, venueId, cancellationToken);
        var screenIndex = Array.FindIndex(wallScreens, candidate => candidate.Id == screen.Id);
        screenIndex = screenIndex < 0 ? 0 : screenIndex;
        var itemOffset = wallScreens
            .Take(screenIndex)
            .Sum(candidate => PhotoGridDensity.Capacity(candidate.PhotoGridDensity));
        var screenCapacity = PhotoGridDensity.Capacity(screen.PhotoGridDensity);
        var wallCapacity = wallScreens.Sum(candidate => PhotoGridDensity.Capacity(candidate.PhotoGridDensity));
        var totalItems = displaySections.Sum(section => section.Items.Count);

        response.PhotoGridDensity = PhotoGridDensity.Normalize(screen.PhotoGridDensity);
        response.PhotoGridOverflowItems = screenIndex == wallScreens.Length - 1
            ? Math.Max(0, totalItems - wallCapacity)
            : 0;
        response.Sections = SliceSections(displaySections, itemOffset, screenCapacity);
        return Ok(response);
    }

    private async Task<Screen[]> ResolveWallScreensAsync(
        Screen screen,
        Guid venueId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(screen.WallGroup))
        {
            return [screen];
        }

        var wallScreens = (await screenRepository.GetByVenueIdAsync(venueId, cancellationToken))
            .Where(candidate => string.Equals(candidate.WallGroup, screen.WallGroup, StringComparison.Ordinal))
            .OrderBy(candidate => candidate.WallPosition ?? int.MaxValue)
            .ThenBy(candidate => candidate.Id)
            .ToArray();
        return wallScreens.Any(candidate => candidate.Id == screen.Id)
            ? wallScreens
            : [screen];
    }

    private static IReadOnlyCollection<DisplayMenuSectionResponse> SliceSections(
        IReadOnlyCollection<DisplayMenuSectionResponse> sections,
        int itemOffset,
        int capacity)
    {
        var remainingOffset = itemOffset;
        var remainingCapacity = capacity;
        var result = new List<DisplayMenuSectionResponse>();
        foreach (var section in sections)
        {
            if (remainingCapacity == 0)
            {
                break;
            }

            var available = section.Items.Skip(remainingOffset).Take(remainingCapacity).ToArray();
            remainingOffset = Math.Max(0, remainingOffset - section.Items.Count);
            if (available.Length == 0)
            {
                continue;
            }

            result.Add(new DisplayMenuSectionResponse
            {
                Id = section.Id,
                Name = section.Name,
                Items = available
            });
            remainingCapacity -= available.Length;
        }
        return result;
    }

    private static IReadOnlyCollection<string> ParseTags(string? tags) =>
        string.IsNullOrWhiteSpace(tags)
            ? []
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(tag => tag.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

    [HttpPost("{screenId:guid}/heartbeat")]
    [ProducesResponseType<ScreenHeartbeatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScreenHeartbeatResponse>> Heartbeat(Guid screenId, [FromBody] ScreenHeartbeatRequest request, CancellationToken cancellationToken)
    {
        var status = request.Status.Trim();
        var lastSeenUtc = DateTime.UtcNow;
        var updated = await screenRepository.UpdateHeartbeatAsync(screenId, lastSeenUtc, status, cancellationToken);

        if (!updated)
        {
            return NotFound(new ProblemDetails { Title = "Screen not found.", Detail = $"Screen '{screenId}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        return Ok(new ScreenHeartbeatResponse
        {
            ScreenId = screenId,
            Status = status,
            LastSeenUtc = lastSeenUtc
        });
    }
}
