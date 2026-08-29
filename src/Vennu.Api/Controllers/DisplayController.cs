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
        /*
         * EVERY page this screen is assigned, not just the first one found.
         *
         * FirstOrDefault was here, so a screen holding five pages was sent one and the other four
         * never reached the player at all - while the back office told the operator, on the
         * assignment page, that "a screen holding more than one page rotates between them". The
         * player could not rotate what it was never given.
         */
        var assignments = contentRepository is null
            ? []
            : (await contentRepository.GetAssignmentsAsync(venueId, cancellationToken))
                .Where(candidate => candidate.ScreenId == screen.Id)
                .ToArray();

        // The first is still what the single-page fields below describe, so nothing that reads
        // them changes meaning.
        var assignment = assignments.FirstOrDefault();

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

        /*
         * Sections for one page, built the same way for every page this screen holds.
         *
         * The body below was written for a single page and is unchanged; it is a local function
         * now so each assigned page can be drawn with it rather than the first one being special.
         */
        async Task<List<DisplayMenuSectionResponse>> SectionsForAsync(Guid? pageId)
        {
            var forPage = (publishedSnapshot.Sections ?? [])
                .Where(section => pageId is not Guid wanted || section.PageId == wanted)
                .OrderBy(section => section.SortOrder)
                .ThenBy(section => section.SectionId)
                .ToArray();
            return await BuildSectionsAsync(forPage);
        }

        static decimal ParsePrice(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0m;
            var cleaned = new string(raw.Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray())
                .Replace(",", string.Empty);
            return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) ? price : 0m;
        }

        async Task<List<DisplayMenuSectionResponse>> BuildSectionsAsync(
            IReadOnlyCollection<Vennu.Api.Services.SnapshotSection> publishedSections)
        {
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
                            /*
                             * A second instance of the currency-symbol bug fixed in MenuRepository.
                             *
                             * A published SNAPSHOT is what a real screen actually shows - not the
                             * live board query BoardItemsSql feeds. Every printed price carries the
                             * symbol it was typed with ("$7.00"), and decimal.TryParse with
                             * NumberStyles.Number rejects that exactly the way TRY_CONVERT did, so
                             * every price on a published, on-screen board rendered $0.00. Fixing the
                             * live-board path alone left the thing an actual guest sees untouched -
                             * caught only by looking at a real screenshot, not by reading code.
                             *
                             * A genuinely non-numeric price (MP, Market) still lands as 0 and still
                             * needs its own answer; this fixes the case that is simply arithmetic.
                             */
                            Price = ParsePrice(item.Price),
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
            return displaySections;
        }

        // The page this screen was assigned first - what Sections has always meant.
        var displaySections = await SectionsForAsync(assignment?.PageId);

        /*
         * And every page it holds, in the order the menu prints them, so the player can cycle.
         *
         * Only when there is more than one: a single-page screen has nothing to rotate to, and
         * sending a one-entry list would invite a player to draw a cycle that never turns.
         */
        if (assignments.Length > 1)
        {
            var pageOrder = (publishedSnapshot.Pages ?? [])
                .OrderBy(page => page.SortOrder)
                .ThenBy(page => page.PageId)
                .ToArray();

            var pages = new List<DisplayMenuPageResponse>();
            foreach (var page in pageOrder.Where(page => assignments.Any(a => a.PageId == page.PageId)))
            {
                pages.Add(new DisplayMenuPageResponse
                {
                    PageId = page.PageId,
                    Name = page.Name,
                    Sections = await SectionsForAsync(page.PageId)
                });
            }

            response.Pages = pages;
            response.PageDwellSeconds = publishedSnapshot.DwellSeconds > 0 ? publishedSnapshot.DwellSeconds : 12;
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
        var isLastOnWall = screenIndex == wallScreens.Length - 1;

        /*
         * Fit ONE page's sections to this screen's slot in the wall, and say what did not fit.
         *
         * Used only for the legacy top-level Sections/PhotoGridOverflowItems (first assigned
         * page, screenful one) - a player that does not know about Pages can never be shown
         * anything past this slice, so "N more items" is still the honest answer for it.
         */
        (IReadOnlyCollection<DisplayMenuSectionResponse> Sections, int Overflow) FitToScreen(
            IReadOnlyCollection<DisplayMenuSectionResponse> pageSections)
        {
            var totalItems = pageSections.Sum(section => section.Items.Count);
            var overflow = isLastOnWall ? Math.Max(0, totalItems - wallCapacity) : 0;
            return (SliceSections(pageSections, itemOffset, screenCapacity), overflow);
        }

        response.PhotoGridDensity = PhotoGridDensity.Normalize(screen.PhotoGridDensity);
        var (firstPageSections, firstPageOverflow) = FitToScreen(displaySections);
        response.Sections = firstPageSections;
        response.PhotoGridOverflowItems = firstPageOverflow;

        /*
         * A page that does not fit one screen gets more than one, shown in turn on the same
         * dwell timer that already rotates between real pages - instead of a "N more items"
         * footer nothing ever scrolls to. Found on Mana-Thai Cuisine's Appetizers page: 13
         * items on a 6-item screen, only 6 ever shown, and Salads - the page's other section -
         * never appeared at all.
         *
         * Every real page is expanded this way, whether the screen has one page or several, so
         * a single-page screen with too much content is treated the same as a multi-page one.
         * Pages stays empty only when the expansion still adds up to a single screen overall -
         * the existing "nothing to rotate to" rule, measured after expansion instead of before.
         */
        IEnumerable<DisplayMenuPageResponse> ExpandVirtualPages(
            Guid pageId, string? name, IReadOnlyCollection<DisplayMenuSectionResponse> sections)
        {
            var totalItems = sections.Sum(section => section.Items.Count);
            var virtualCount = wallCapacity > 0
                ? Math.Max(1, (int)Math.Ceiling(totalItems / (double)wallCapacity))
                : 1;
            for (var slide = 0; slide < virtualCount; slide++)
            {
                yield return new DisplayMenuPageResponse
                {
                    PageId = pageId,
                    Name = name,
                    Sections = SliceSections(sections, itemOffset + (slide * wallCapacity), screenCapacity),
                    PhotoGridOverflowItems = 0
                };
            }
        }

        var realPages = response.Pages.Count > 0
            ? response.Pages
            : [new DisplayMenuPageResponse { PageId = assignment?.PageId ?? Guid.Empty, Sections = displaySections }];

        var expandedPages = realPages
            .SelectMany(page => ExpandVirtualPages(page.PageId, page.Name, page.Sections))
            .ToArray();

        if (expandedPages.Length > 1)
        {
            // Reset here too: a single real page that only needed sub-paging never went
            // through the assignments.Length > 1 branch above, so this is its only chance
            // to pick up the menu's dwell time instead of the response's bare default.
            response.PageDwellSeconds = publishedSnapshot.DwellSeconds > 0 ? publishedSnapshot.DwellSeconds : 12;
        }
        response.Pages = expandedPages.Length > 1 ? expandedPages : [];

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
            LastReceiptPlayerVersion = delivery?.PlayerVersion,
            LastReceiptShellVersion = delivery?.ShellVersion,
            IsOnboardingFirstScreen = onboarding is not null,
            OnboardingGoLiveAchievedUtc = onboarding?.GoLiveAchievedUtc
        });
    }
}
