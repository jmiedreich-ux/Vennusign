using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vennu.Api.BackOffice;
using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Api.Controllers.TestSupport;

/// <summary>
/// Creates isolated data for automated UI tests so that concurrent specs never mutate
/// the same rows. Development only: <see cref="Seed"/> returns 404 in every other
/// environment, so the route does not exist in a deployed system.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/test")]
public sealed class TestSeedController(
    IHostEnvironment environment,
    IOptionsMonitor<BackOfficeAuthenticationOptions> backOfficeOptions,
    IMenuRepository menuRepository,
    IContentRepository libraryRepository,
    IScreenRepository screenRepository,
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
}
