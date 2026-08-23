using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vennu.Api.BackgroundServices;
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
    private readonly IScreenContentDeliveryService? deliveryService;
    private readonly ICustomerOnboardingRepository? customerOnboarding;
    private readonly IContentRepository? contentRepository;
    private readonly TimeSpan heartbeatStaleThreshold;

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
        ITapListRepository? tapListRepository = null,
        IScreenContentDeliveryService? deliveryService = null,
        ICustomerOnboardingRepository? customerOnboarding = null,
        IContentRepository? contentRepository = null,
        IOptions<HeartbeatMonitorOptions>? heartbeatMonitorOptions = null) =>
        (this.screenRepository, this.venueRepository, this.menuRepository, this.themeRepository, this.happyHourService, this.playlistService, this.emergencyBroadcastService, this.promotionService, this.tapListRepository, this.timeProvider, this.deliveryService, this.customerOnboarding, this.contentRepository, this.heartbeatStaleThreshold) =
        (screenRepository, venueRepository, menuRepository, themeRepository, happyHourService, playlistService, emergencyBroadcastService, promotionService, tapListRepository, timeProvider ?? TimeProvider.System, deliveryService, customerOnboarding, contentRepository, heartbeatMonitorOptions?.Value.StaleThreshold ?? TimeSpan.FromSeconds(90));

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

        if (string.Equals(screen.Status, "Archived", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status410Gone, new ProblemDetails
            {
                Title = "Screen archived.",
                Detail = "Restore this screen in Back Office before using the player.",
                Status = StatusCodes.Status410Gone
            });
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
        response.ContentRevision = deliveryService is null
            ? null
            : (await deliveryService.GetLatestAsync(screenId, cancellationToken).ConfigureAwait(false))?.AuthoritativeRevision;

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
        // What this screen was actually assigned, which is the whole point of
        // assigning it. Reading "the venue's first active menu and all of its
        // sections" instead meant every screen in a venue showed the same thing
        // and a second menu could never be reached.
        var assignment = contentRepository is null
            ? null
            : (await contentRepository.GetAssignmentsAsync(venueId, cancellationToken))
                .FirstOrDefault(candidate => candidate.ScreenId == screen.Id);

        var menus = await menuRepository.GetMenusAsync(venueId, cancellationToken);
        var menu = assignment is null
            ? menus.FirstOrDefault(candidate => candidate.IsActive)
            : menus.FirstOrDefault(candidate => candidate.Id == assignment.MenuId)
              ?? menus.FirstOrDefault(candidate => candidate.IsActive);

        response.VenueName = venue?.Name;
        response.MenuName = menu?.Name;
        response.DailySpecial = menu?.DailySpecial;
        if (menu is null)
        {
            return Ok(response);
        }

        // Decision 1: nothing reaches a screen without a deliberate act, and decision
        // 2 puts every edit - price, copy, structure, layout - in one queue that ships
        // on publish. So the board is composed from the last published snapshot and
        // never from the builder's live tables. Reading Placements/MenuSections here
        // made every keystroke live the moment it was typed and left publish as
        // bookkeeping, which is decision 1 exactly inverted.
        var publishedSnapshot = contentRepository is null
            ? null
            : MenuSnapshot.Parse(await contentRepository
                .GetLatestPublishedSnapshotAsync(venueId, menu.Id, cancellationToken)
                .ConfigureAwait(false));

        // Decision 14: until something has been published, a screen shows the venue's
        // chosen fallback. Never the draft, which is the whole point, and never the
        // live tables as a "better than blank" consolation.
        if (publishedSnapshot is null)
        {
            return Ok(response);
        }

        // An assigned screen shows that page. An unassigned one keeps the older
        // behaviour rather than going blank.
        var publishedSections = (publishedSnapshot.Sections ?? [])
            .Where(section => assignment?.PageId is not Guid assignedPage || section.PageId == assignedPage)
            .OrderBy(section => section.SortOrder)
            .ThenBy(section => section.SectionId)
            .ToArray();

        var displaySections = new List<DisplayMenuSectionResponse>();
        foreach (var section in publishedSections)
        {
            // The snapshot is the authority for what is on the board, in what order,
            // and with what name, copy and price. The live row is read only for what
            // a snapshot does not carry (the image) and for availability, which by
            // decision 3 is a fact about tonight and never waits for a publish.
            var live = (await menuRepository.GetActiveBoardItemsAsync(venueId, section.SectionId, cancellationToken))
                .GroupBy(item => item.Id)
                .ToDictionary(group => group.Key, group => group.First());

            displaySections.Add(new DisplayMenuSectionResponse
            {
                Id = section.SectionId,
                Name = section.Name ?? string.Empty,
                Items = (section.Items ?? [])
                    .OrderBy(item => item.SortOrder)
                    .ThenBy(item => item.ItemId)
                    .Select(item =>
                    {
                        live.TryGetValue(item.ItemId, out var current);
                        return new DisplayMenuItemResponse
                        {
                            Id = item.ItemId,
                            Name = item.Name ?? string.Empty,
                            Description = item.Description,
                            // A snapshot keeps the price exactly as it was typed. The
                            // display contract carries a decimal, so a market price or
                            // a dash lands as 0 - the same conversion BoardItemsSql did
                            // with TRY_CONVERT. See the note on this in issue #739.
                            Price = decimal.TryParse(
                                item.Price,
                                NumberStyles.Number,
                                CultureInfo.InvariantCulture,
                                out var price) ? price : 0m,
                            HappyHourPrice = current?.HappyHourPrice,
                            // An item published and since deleted from the builder is
                            // still on the wall, so a missing live row means available
                            // rather than hidden.
                            IsAvailable = current?.IsAvailable ?? true,
                            QuantityAvailable = current?.QuantityAvailable,
                            IsPopular = current?.IsPopular ?? false,
                            Tags = ParseTags(current?.Tags),
                            ImageUrl = current?.ImageUrl
                        };
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
            .Where(candidate => !string.Equals(candidate.Status, "Archived", StringComparison.OrdinalIgnoreCase)
                && string.Equals(candidate.WallGroup, screen.WallGroup, StringComparison.Ordinal))
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
        var platform = ScreenPlatform.NormalizeOptional(request.Platform);
        var appVersion = string.IsNullOrWhiteSpace(request.AppVersion) ? null : request.AppVersion.Trim();
        var updated = await screenRepository.UpdateHeartbeatAsync(
            screenId,
            lastSeenUtc,
            status,
            platform,
            appVersion,
            cancellationToken);

        if (!updated)
        {
            return NotFound(new ProblemDetails { Title = "Screen not found.", Detail = $"Screen '{screenId}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        // This heartbeat is the only moment Vennusign can observe a display coming online, and
        // it is the moment onboarding's go-live step is satisfied. Latch it here rather than in
        // the onboarding read: a player that comes online while nobody has the onboarding page
        // open would otherwise never be recorded, and the customer would be returned to the
        // opening checklist on their next sign-in. The latch is a no-op for every screen that no
        // onboarding journey names as its first display, and for every heartbeat after the first.
        if (customerOnboarding is not null && status.Equals("Online", StringComparison.OrdinalIgnoreCase))
        {
            await customerOnboarding.LatchGoLiveByFirstScreenAsync(screenId, lastSeenUtc, cancellationToken);
        }

        return Ok(new ScreenHeartbeatResponse
        {
            ScreenId = screenId,
            Status = status,
            LastSeenUtc = lastSeenUtc,
            Platform = platform,
            AppVersion = appVersion
        });
    }

    [HttpPost("{screenId:guid}/content-receipts")]
    public async Task<ActionResult<ScreenContentReceiptResponse>> ContentReceipt(
        Guid screenId, ScreenContentReceiptRequest request, CancellationToken cancellationToken)
    {
        if (deliveryService is null) return Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
        try
        {
            var delivery = await deliveryService.AcknowledgeAsync(screenId, new ScreenContentReceipt(
                request.Revision, request.State, request.ScreenKey, request.PlayerVersion, request.ShellVersion,
                request.Platform, request.FailureCode, request.FailureDetail, request.Recovered), cancellationToken).ConfigureAwait(false);
            return delivery is null
                ? NotFound()
                : Ok(new ScreenContentReceiptResponse(delivery.AuthoritativeRevision, delivery.AppliedRevision, delivery.State, delivery.AppliedUtc));
        }
        catch (ArgumentException exception) { return ValidationProblem(exception.Message); }
    }

    // Anonymous like the other player endpoints - the point is diagnosing a screen from
    // whatever a person has in hand at the venue, with no sign-in. Identifiers, states and
    // timestamps only: no menu content, no customer PII, no organisation detail.
    [HttpGet("{screenId:guid}/diagnostics")]
    [ProducesResponseType<DisplayDiagnosticsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisplayDiagnosticsResponse>> GetDiagnostics(Guid screenId, CancellationToken cancellationToken)
    {
        var screen = await screenRepository.GetByIdAsync(screenId, cancellationToken);

        if (screen is null)
        {
            return NotFound(new ProblemDetails { Title = "Screen not found.", Detail = $"Screen '{screenId}' was not found.", Status = StatusCodes.Status404NotFound });
        }

        var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
        var secondsSinceLastSeen = screen.LastSeen is { } lastSeen ? (nowUtc - lastSeen).TotalSeconds : (double?)null;

        var delivery = deliveryService is null ? null : await deliveryService.GetLatestAsync(screenId, cancellationToken);
        var onboarding = customerOnboarding is null ? null : await customerOnboarding.GetByFirstScreenIdAsync(screenId, cancellationToken);

        return Ok(new DisplayDiagnosticsResponse
        {
            ScreenId = screen.Id,
            VenueId = screen.VenueId,
            ScreenKey = screen.ScreenKey,
            ScreenName = screen.Name,
            IsAssignedToVenue = screen.VenueId is not null,
            Status = screen.Status,
            LastSeenUtc = screen.LastSeen,
            SecondsSinceLastSeen = secondsSinceLastSeen,
            IsStale = secondsSinceLastSeen is null || secondsSinceLastSeen > heartbeatStaleThreshold.TotalSeconds,
            Platform = screen.Platform,
            AppVersion = screen.AppVersion,
            DesiredAppVersion = screen.DesiredAppVersion,
            ConfiguredWidthPixels = screen.WidthPixels,
            ConfiguredHeightPixels = screen.HeightPixels,
            AuthoritativeRevision = delivery?.AuthoritativeRevision,
            AppliedRevision = delivery?.AppliedRevision,
            DeliveryState = delivery?.State,
            DeliveryRequestedUtc = delivery?.RequestedUtc,
            DeliveryReceivedUtc = delivery?.ReceivedUtc,
            DeliveryAppliedUtc = delivery?.AppliedUtc,
            DeliveryFailureCode = delivery?.FailureCode,
            DeliveryFailureDetail = delivery?.FailureDetail,
            LastReceiptPlayerVersion = delivery?.PlayerVersion,
            LastReceiptShellVersion = delivery?.ShellVersion,
            IsOnboardingFirstScreen = onboarding is not null,
            OnboardingGoLiveAchievedUtc = onboarding?.GoLiveAchievedUtc
        });
    }
}
