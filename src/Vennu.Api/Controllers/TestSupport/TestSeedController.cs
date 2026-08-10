using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vennu.Api.BackOffice;
using Vennu.Api.Services;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.DataAccess;

namespace Vennu.Api.Controllers.TestSupport;

/// <summary>
/// Creates isolated data for automated UI tests so that concurrent specs never mutate
/// the same rows.
///
/// Every action returns 404 outside Development. That is a gate, not a deployment
/// boundary - the route still exists, and a remote test slot can carry the
/// Development setting - so anything here that DESTROYS data carries its own gates
/// as well. See <see cref="SeedScale"/>.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/test")]
public sealed class TestSeedController(
    IHostEnvironment environment,
    IOptionsMonitor<BackOfficeAuthenticationOptions> backOfficeOptions,
    IConfiguration configuration,
    IMenuRepository menuRepository,
    IContentRepository libraryRepository,
    IScreenRepository screenRepository,
    ContentService content,
    ISqlDataAccess dataAccess,
    TimeProvider timeProvider) : ControllerBase
{
    public sealed record SeedRequest
    {
        /// <summary>
        /// Access token whose venue the seeded data belongs to. Required: it is the
        /// credential for this endpoint as well as the venue selector, so seeding is
        /// not possible without already knowing a configured session token.
        /// </summary>
        public string? AccessToken { get; init; }

        /// <summary>Seed a screen as well as menu content. Screens are only needed by delivery cases.</summary>
        public bool IncludeScreen { get; init; } = true;

        /// <summary>Optional label woven into seeded names to make failures readable.</summary>
        public string? Label { get; init; }
    }

    public sealed record SeedResponse
    {
        public required Guid OrganizationId { get; init; }
        public required Guid VenueId { get; init; }
        public required Guid MenuId { get; init; }
        public required Guid SectionId { get; init; }
        public required Guid ItemId { get; init; }
        public required string MenuName { get; init; }
        public required string SectionName { get; init; }
        public required string ItemName { get; init; }
        public required string ItemDescription { get; init; }
        public required decimal ItemPrice { get; init; }
        public Guid? ScreenId { get; init; }
        public string? ScreenKey { get; init; }
    }

    [HttpPost("seed")]
    public async Task<ActionResult<SeedResponse>> Seed(
        [FromBody] SeedRequest? request,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        request ??= new SeedRequest();
        // These options derive from AuthenticationSchemeOptions, so they are registered
        // under the scheme name rather than as the default unnamed instance.
        var sessions = backOfficeOptions
            .Get(BackOfficeAuthenticationDefaults.AuthenticationScheme)
            .Sessions.Where(session => session.Enabled).ToList();
        if (sessions.Count == 0)
        {
            return Problem("No enabled Back Office sessions are configured, so there is no venue to seed into.", statusCode: StatusCodes.Status409Conflict);
        }

        // Defence in depth behind the Development gate, and it removes an easy mistake:
        // defaulting to the first session silently seeded a different venue than the
        // caller expected. An unmatched or absent token is reported identically so the
        // endpoint does not confirm which tokens exist.
        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return NotFound();
        }

        var session = sessions.FirstOrDefault(candidate =>
            string.Equals(candidate.AccessToken, request.AccessToken, StringComparison.Ordinal));
        if (session is null)
        {
            return NotFound();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var label = string.IsNullOrWhiteSpace(request.Label) ? "seed" : request.Label.Trim();

        var menuId = await menuRepository.CreateMenuAsync(
            new Menu { Id = Guid.NewGuid(), VenueId = session.VenueId, Name = $"{label} menu {suffix}", IsActive = true },
            cancellationToken).ConfigureAwait(false);

        var sectionId = await menuRepository.CreateSectionAsync(
            new MenuSection { Id = Guid.NewGuid(), VenueId = session.VenueId, MenuId = menuId, Name = $"{label} section {suffix}", SortOrder = 0, IsActive = true },
            cancellationToken).ConfigureAwait(false);

        var itemName = $"{label} item {suffix}";
        const string itemDescription = "Seeded for an automated UI test.";
        const decimal itemPrice = 4.50m;

        // The seed writes what the product reads: a library item placed on the
        // section. Prices are stored exactly as typed (Q115/Q190).
        var item = new Item
        {
            Id = Guid.NewGuid(),
            VenueId = session.VenueId,
            Name = itemName,
            Description = itemDescription,
            Price = itemPrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Source = ItemSources.Manual
        };
        var ceilings = await libraryRepository
            .GetResolvedCeilingsAsync(session.VenueId, cancellationToken)
            .ConfigureAwait(false);
        var outcome = await libraryRepository.CreateItemOnMenuAsync(
            item,
            menuId,
            sectionId,
            ceilings.TryGetValue(MenuCeilings.ItemsPerMenu, out var itemLimit) ? itemLimit : int.MaxValue,
            cancellationToken).ConfigureAwait(false);
        if (outcome.Outcome != ItemPlacementOutcomes.Created)
        {
            return Problem($"Seeding the item failed: {outcome.Outcome}.", statusCode: StatusCodes.Status409Conflict);
        }

        var itemId = item.Id;

        Guid? screenId = null;
        string? screenKey = null;
        if (request.IncludeScreen)
        {
            // UX_Screens_ScreenKey is globally unique and the column is nvarchar(9),
            // so the key has to be short and generated rather than suffixed.
            screenKey = $"t{suffix}";
            screenId = await screenRepository.CreateAsync(
                new Screen
                {
                    Id = Guid.NewGuid(),
                    VenueId = session.VenueId,
                    ScreenKey = screenKey,
                    Name = $"{label} screen {suffix}",
                    Location = "Automated test",
                    Status = "Offline",
                    Platform = "web",
                    AppVersion = "ui-test"
                },
                cancellationToken).ConfigureAwait(false);
        }

        return Ok(new SeedResponse
        {
            OrganizationId = session.OrganizationId,
            VenueId = session.VenueId,
            MenuId = menuId,
            SectionId = sectionId,
            ItemId = itemId,
            MenuName = $"{label} menu {suffix}",
            SectionName = $"{label} section {suffix}",
            ItemName = itemName,
            ItemDescription = itemDescription,
            ItemPrice = itemPrice,
            ScreenId = screenId,
            ScreenKey = screenKey
        });
    }

    /// <summary>
    /// Off unless something deliberately turns it on. `scripts/start-ui-test-env.ps1`
    /// sets it for a local UI run; nothing else does, so a deployed slot that
    /// happens to carry the Development environment still refuses.
    /// </summary>
    private const string ScaleSeedEnabledKey = "TestSupport:ScaleSeedEnabled";

    /// <summary>
    /// The only venue this action will ever clear — created by the owner
    /// acceptance fixture purely so the Menus shelf can be measured at scale.
    /// </summary>
    private static readonly Guid ScaleSeedVenueId = Guid.Parse("73000000-0000-0000-0000-000000000002");

    public sealed record ScaleSeedRequest
    {
        public string? AccessToken { get; init; }

        /// <summary>Total menus to leave in the venue. Q176's check is thirteen.</summary>
        public int Menus { get; init; } = 13;

        /// <summary>Screens to leave in the venue. Q176's check is twenty.</summary>
        public int Screens { get; init; } = 20;
    }

    public sealed record ScaleSeedMenu
    {
        public required Guid MenuId { get; init; }
        public required string Name { get; init; }
        /// <summary>on-screens | pending-changes | put-away | never-published</summary>
        public required string State { get; init; }
        public required IReadOnlyCollection<Guid> ScreenIds { get; init; }
    }

    public sealed record ScaleSeedResponse
    {
        public required Guid VenueId { get; init; }
        public required IReadOnlyCollection<ScaleSeedMenu> SeededMenus { get; init; }
        public required IReadOnlyCollection<Guid> ScreenIds { get; init; }
    }

    /// <summary>
    /// Fills a venue with a shelf big enough to change shape, and leaves it in a
    /// known state every time.
    ///
    /// Built out of the product's own write paths - create, place, assign, publish -
    /// rather than SQL that inserts publish events directly. A fixture that writes
    /// its own snapshot JSON is a second implementation of the snapshot's shape, and
    /// the second implementation drifts; that is exactly how milestone 1's in-memory
    /// repository came to disagree with the database it stood in for.
    ///
    /// The venue is cleared first, so the shelf is deterministic however many times
    /// this runs. That is also why the scale check has a venue of its own: the
    /// default one accumulates menus from every spec that seeds, so nothing there
    /// could ever assert "exactly this many" while the suite runs in parallel.
    /// </summary>
    [HttpPost("seed/scale")]
    public async Task<ActionResult<ScaleSeedResponse>> SeedScale(
        [FromBody] ScaleSeedRequest? request,
        CancellationToken cancellationToken)
    {
        // Independent review, 2026-08-10: this action DELETES a venue's screens,
        // menus, items, placements, availability, assignments, publish events and
        // history. It previously refused one hard-coded venue and accepted every
        // other, so any enabled session token authorised erasing its own venue —
        // and IsDevelopment() is a setting, not a deployment boundary: a remote
        // test slot can carry it.
        //
        // Now three independent gates, each of which alone refuses the request:
        // the environment, an explicit opt-in that is off unless something sets
        // it, and an allowlist of exactly one venue that exists for this and
        // nothing else. A venue session token no longer authorises "delete
        // everything in my venue"; it only identifies the caller.
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        if (!configuration.GetValue<bool>(ScaleSeedEnabledKey))
        {
            return NotFound();
        }

        request ??= new ScaleSeedRequest();
        var session = ResolveSession(request.AccessToken);
        if (session is null)
        {
            return NotFound();
        }

        if (session.VenueId != ScaleSeedVenueId)
        {
            // An allowlist, not a denylist. The scale venue is created by the
            // owner-acceptance fixture for this purpose; every other venue in
            // every configuration is refused, including ones nobody has thought of.
            return Problem(
                "The scale seed clears the venue it seeds into, so it runs against the scale fixture venue and no other.",
                statusCode: StatusCodes.Status409Conflict);
        }

        var menus = Math.Clamp(request.Menus, 1, 40);
        var screens = Math.Clamp(request.Screens, 1, 40);

        await ClearVenueContentAsync(session.VenueId, cancellationToken).ConfigureAwait(false);

        var screenIds = new List<Guid>();
        for (var index = 0; index < screens; index++)
        {
            screenIds.Add(await screenRepository.CreateAsync(
                new Screen
                {
                    Id = Guid.NewGuid(),
                    VenueId = session.VenueId,
                    // Globally unique and nvarchar(9), so short and generated.
                    ScreenKey = $"x{Guid.NewGuid().ToString("N")[..8]}",
                    Name = $"Scale screen {index + 1:00}",
                    Location = "Scale check",
                    Status = "Offline",
                    Platform = "web",
                    AppVersion = "ui-test"
                },
                cancellationToken).ConfigureAwait(false));
        }

        var ceilings = await libraryRepository.GetResolvedCeilingsAsync(session.VenueId, cancellationToken).ConfigureAwait(false);
        var itemLimit = ceilings.TryGetValue(MenuCeilings.ItemsPerMenu, out var limit) ? limit : int.MaxValue;

        var seeded = new List<ScaleSeedMenu>();
        var nextScreen = 0;

        for (var index = 0; index < menus; index++)
        {
            // A deterministic spread, so every card state the shelf can draw is on
            // the shelf: one menu spanning enough screens to exercise Q169's
            // top-three cap, two holding changes, one put away, one never
            // published, and the rest ordinary.
            var state = index switch
            {
                0 => "on-screens",
                1 or 2 => "pending-changes",
                3 => "put-away",
                4 => "never-published",
                _ => "on-screens"
            };

            var name = $"Scale menu {index + 1:00}";
            var menuId = await menuRepository.CreateMenuAsync(
                new Menu { Id = Guid.NewGuid(), VenueId = session.VenueId, Name = name, IsActive = true },
                cancellationToken).ConfigureAwait(false);

            var sectionId = await menuRepository.CreateSectionAsync(
                new MenuSection
                {
                    Id = Guid.NewGuid(),
                    VenueId = session.VenueId,
                    MenuId = menuId,
                    Name = index % 2 == 0 ? "Drinks" : "Snacks",
                    SortOrder = 0,
                    IsActive = true
                },
                cancellationToken).ConfigureAwait(false);

            // Two items, one of them market-priced, so a card's board proves prices
            // render exactly as typed (Q115/Q190) rather than as numbers.
            foreach (var (itemName, price) in new[] { ($"Scale item {index + 1:00}", "8.5"), ("Market catch", "MP") })
            {
                var outcome = await libraryRepository.CreateItemOnMenuAsync(
                    new Item
                    {
                        Id = Guid.NewGuid(),
                        VenueId = session.VenueId,
                        Name = itemName,
                        Description = "Seeded for the shelf-at-scale check.",
                        Price = price,
                        Source = ItemSources.Manual
                    },
                    menuId,
                    sectionId,
                    itemLimit,
                    cancellationToken).ConfigureAwait(false);

                if (outcome.Outcome != ItemPlacementOutcomes.Created)
                {
                    return Problem($"Seeding an item failed: {outcome.Outcome}.", statusCode: StatusCodes.Status409Conflict);
                }
            }

            var assigned = new List<Guid>();
            if (state != "never-published")
            {
                // The first menu takes several screens so the headline's screen-count
                // phrasing has something to cap.
                var take = index == 0 ? Math.Min(6, screenIds.Count) : 1;
                for (var screen = 0; screen < take && nextScreen < screenIds.Count; screen++, nextScreen++)
                {
                    await content.AssignAsync(session.VenueId, screenIds[nextScreen], menuId, "scale seed", cancellationToken)
                        .ConfigureAwait(false);
                    assigned.Add(screenIds[nextScreen]);
                }

                if (assigned.Count > 0)
                {
                    // Published through the product, so the snapshot, the delivery
                    // rows and the history entry are made by the code that owns them.
                    _ = await content.PublishAsync(session.VenueId, menuId, "scale seed", cancellationToken).ConfigureAwait(false);
                }
            }

            if (state == "pending-changes")
            {
                // An edit after the publish, so this card has changes waiting.
                var item = (await libraryRepository.GetItemsAsync(session.VenueId, cancellationToken).ConfigureAwait(false))
                    .First(candidate => candidate.Name == $"Scale item {index + 1:00}");
                item.Price = "9.5";
                _ = await libraryRepository.UpdateItemAsync(item, cancellationToken).ConfigureAwait(false);
            }

            if (state == "put-away")
            {
                // Take off, publish that, then put away - the only route onto the
                // shelf's Not in use strip, because a menu still on a screen is
                // never shelved underneath the person.
                if (assigned.Count > 0)
                {
                    _ = await content.QueueTakeOffScreensAsync(session.VenueId, menuId, "scale seed", cancellationToken).ConfigureAwait(false);
                    _ = await content.PublishAsync(session.VenueId, menuId, "scale seed", cancellationToken).ConfigureAwait(false);
                    assigned.Clear();
                }

                _ = await content.SetPutAwayAsync(session.VenueId, menuId, true, "scale seed", cancellationToken).ConfigureAwait(false);
            }

            seeded.Add(new ScaleSeedMenu { MenuId = menuId, Name = name, State = state, ScreenIds = assigned });
        }

        return Ok(new ScaleSeedResponse
        {
            VenueId = session.VenueId,
            SeededMenus = seeded,
            ScreenIds = screenIds
        });
    }

    private BackOfficeSessionOptions? ResolveSession(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken)) return null;

        return backOfficeOptions
            .Get(BackOfficeAuthenticationDefaults.AuthenticationScheme)
            .Sessions
            .Where(session => session.Enabled)
            .FirstOrDefault(session => string.Equals(session.AccessToken, accessToken, StringComparison.Ordinal));
    }

    /// <summary>
    /// Empties the scale venue, children first.
    ///
    /// In SQL rather than through the API for the same reason the seed prune is:
    /// a deployed system carries no delete-everything-in-this-venue endpoint, and
    /// it should not gain one to make a test convenient.
    /// </summary>
    private async Task ClearVenueContentAsync(Guid venueId, CancellationToken cancellationToken)
    {
        const string clearSql = """
            DELETE t FROM dbo.MenuPublishTargets t WHERE t.VenueId = @VenueId;
            DELETE h FROM dbo.MenuHistoryEntries h WHERE h.VenueId = @VenueId;
            DELETE e FROM dbo.MenuPublishEvents e WHERE e.VenueId = @VenueId;
            DELETE a FROM dbo.MenuScreenAssignments a WHERE a.VenueId = @VenueId;
            DELETE p FROM dbo.Placements p WHERE p.VenueId = @VenueId;
            DELETE av FROM dbo.ItemAvailability av WHERE av.VenueId = @VenueId;
            DELETE i FROM dbo.Items i WHERE i.VenueId = @VenueId;
            DELETE s FROM dbo.MenuSections s WHERE s.VenueId = @VenueId;
            DELETE m FROM dbo.Menus m WHERE m.VenueId = @VenueId;
            DELETE sc FROM dbo.Screens sc WHERE sc.VenueId = @VenueId;
            SELECT 1 AS Value;
            """;

        _ = await dataAccess
            .ExecuteSqlQueryAsync<ClearedRow, object>(clearSql, new { VenueId = venueId }, cancellationToken)
            .ConfigureAwait(false);
    }

    private sealed class ClearedRow
    {
        public int Value { get; set; }
    }
}
