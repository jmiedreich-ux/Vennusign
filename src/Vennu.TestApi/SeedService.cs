namespace Vennu.TestApi;

public sealed class SeedService(ProductApiClient product)
{
    public async Task<SeedResponse> SeedAsync(SeedRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken))
            throw new ProductApiException(StatusCodes.Status404NotFound, "");

        var token = request.AccessToken;
        var session = await product.SendAsync<SessionResponse>(
            HttpMethod.Get, "/api/back-office/session", token, null, cancellationToken).ConfigureAwait(false);

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var label = string.IsNullOrWhiteSpace(request.Label) ? "seed" : request.Label.Trim();
        if (!ScreenSeedStates.IsSupported(request.ScreenState))
            throw new ProductApiException(StatusCodes.Status400BadRequest, "Unsupported screen seed state.");
        var menuName = $"{label} menu {suffix}";
        const string itemDescription = "Seeded for an automated UI test.";
        const decimal itemPrice = 4.50m;

        var menu = await product.SendAsync<MenuResponse>(
            HttpMethod.Post, "/api/back-office/menus", token, new { name = menuName }, cancellationToken).ConfigureAwait(false);
        var pages = (await product.SendAsync<IReadOnlyCollection<PageResponse>>(
            HttpMethod.Get, $"/api/back-office/content/menus/{menu.Id}/pages", token, null, cancellationToken).ConfigureAwait(false)).ToList();
        for (var pageIndex = pages.Count; pageIndex < Math.Clamp(request.PageCount, 1, 20); pageIndex++)
        {
            pages.Add(await product.SendAsync<PageResponse>(HttpMethod.Post,
                $"/api/back-office/content/menus/{menu.Id}/pages", token, new { name = $"{label} page {pageIndex + 1:00} {suffix}" }, cancellationToken).ConfigureAwait(false));
        }
        var sectionCount = Math.Clamp(request.SectionCount, 1, 20);
        var itemsPerSection = Math.Clamp(request.ItemsPerSection, 1, 100);
        var knownNames = request.LibraryItemNames?
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
        var sections = new List<SeedSection>();
        var items = new List<SeedItem>();
        var knownIndex = 0;
        for (var sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
        {
            var sectionName = sectionCount == 1
                ? $"{label} section {suffix}"
                : $"{label} section {sectionIndex + 1:00} {suffix}";
            var section = await product.SendAsync<SectionResponse>(HttpMethod.Post,
                $"/api/back-office/content/menus/{menu.Id}/sections", token, new { name = sectionName, pageId = pages[sectionIndex % pages.Count].PageId }, cancellationToken).ConfigureAwait(false);
            sections.Add(new SeedSection(section.SectionId, pages[sectionIndex % pages.Count].PageId, section.Name, section.SortOrder));
            for (var itemIndex = 0; itemIndex < itemsPerSection; itemIndex++)
            {
                var itemName = knownIndex < knownNames.Length
                    ? knownNames[knownIndex++]
                    : $"{label} item {sectionIndex + 1:00}-{itemIndex + 1:00} {suffix}";
                var placement = await product.SendAsync<PlaceResponse>(HttpMethod.Post,
                    $"/api/back-office/content/menus/{menu.Id}/sections/{section.SectionId}/items", token,
                    new { name = itemName }, cancellationToken).ConfigureAwait(false);
                var itemId = placement.ItemId ?? throw new InvalidOperationException("The product API did not identify the item it created.");
                await product.SendAsync(HttpMethod.Put, $"/api/back-office/content/items/{itemId}", token,
                    new { name = itemName, description = itemDescription, price = itemPrice.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                    cancellationToken).ConfigureAwait(false);
                items.Add(new SeedItem(itemId, section.SectionId, itemName, itemPrice.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            }
        }

        var firstSection = sections[0];
        var firstItem = items[0];

        Guid? screenId = null;
        string? screenKey = null;
        if (request.IncludeScreen)
        {
            var screen = request.ScreenState == ScreenSeedStates.NeverPaired
                ? await RegisterScreenAsync($"{label} screen {suffix}", request.ScreenWidthPixels, request.ScreenHeightPixels, cancellationToken).ConfigureAwait(false)
                : await CreatePairedScreenAsync(token, $"{label} screen {suffix}", request.ScreenWidthPixels, request.ScreenHeightPixels, cancellationToken).ConfigureAwait(false);
            screenId = screen.ScreenId;
            screenKey = screen.ScreenKey;
            if (request.ScreenState == ScreenSeedStates.Online)
            {
                _ = await product.SendPublicAsync<HeartbeatResponse>(HttpMethod.Post, $"/api/display/{screen.ScreenId}/heartbeat",
                    new { status = "Online", platform = "web", appVersion = "ui-test" }, cancellationToken).ConfigureAwait(false);
            }
            else if (request.ScreenState == ScreenSeedStates.HasNotTakenThisYet)
            {
                await product.SendAsync(HttpMethod.Put, $"/api/back-office/content/screens/{screen.ScreenId}/menu", token,
                    new { menuId = menu.Id, pageId = pages[0].PageId }, cancellationToken).ConfigureAwait(false);
                await product.SendAsync(HttpMethod.Post, $"/api/back-office/content/menus/{menu.Id}/publish", token, null, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return new SeedResponse(
            session.OrganizationId ?? Guid.Empty,
            session.VenueId,
            menu.Id,
            firstSection.SectionId,
            firstItem.ItemId,
            menuName,
            firstSection.Name,
            firstItem.Name,
            itemDescription,
            itemPrice,
            screenId,
            screenKey,
            sections,
            items,
            request.IncludeScreen ? request.ScreenState : null,
            pages.Select(page => new SeedPage(page.PageId, page.Name, page.SortOrder)).ToArray());
    }

    public async Task<ScaleSeedResponse> SeedScaleAsync(ScaleSeedRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken)) throw new ProductApiException(StatusCodes.Status404NotFound, "");
        var token = request.AccessToken;
        var session = await product.SendAsync<SessionResponse>(HttpMethod.Get, "/api/back-office/session", token, null, cancellationToken)
            .ConfigureAwait(false);
        await product.SendAutomationAsync("/api/test-automation/venues/reset", new { accessToken = token }, cancellationToken)
            .ConfigureAwait(false);

        var screenIds = new List<Guid>();
        for (var index = 0; index < Math.Clamp(request.Screens, 1, 40); index++)
        {
            var screen = await CreatePairedScreenAsync(token, $"Scale screen {index + 1:00}", 1920, 1080, cancellationToken).ConfigureAwait(false);
            screenIds.Add(screen.ScreenId);
        }

        var seeded = new List<ScaleSeedMenu>();
        var nextScreen = 0;
        var menuCount = Math.Clamp(request.Menus, 1, 40);
        for (var index = 0; index < menuCount; index++)
        {
            var requestedState = ScaleState(index);
            var name = $"Scale menu {index + 1:00}";
            var menu = await product.SendAsync<MenuResponse>(HttpMethod.Post, "/api/back-office/menus", token, new { name }, cancellationToken)
                .ConfigureAwait(false);
            var menuPages = await product.SendAsync<PageResponse[]>(HttpMethod.Get,
                $"/api/back-office/content/menus/{menu.Id}/pages", token, null, cancellationToken).ConfigureAwait(false);
            var defaultPageId = menuPages.OrderBy(page => page.SortOrder).First().PageId;
            var section = await product.SendAsync<SectionResponse>(HttpMethod.Post,
                $"/api/back-office/content/menus/{menu.Id}/sections", token,
                new { name = index % 2 == 0 ? "Drinks" : "Snacks", pageId = defaultPageId }, cancellationToken).ConfigureAwait(false);

            Guid firstItemId = Guid.Empty;
            foreach (var (itemName, price) in new[] { ($"Scale item {index + 1:00}", "8.5"), ("Market catch", "MP") })
            {
                var placed = await product.SendAsync<PlaceResponse>(HttpMethod.Post,
                    $"/api/back-office/content/menus/{menu.Id}/sections/{section.SectionId}/items", token,
                    new { name = itemName }, cancellationToken).ConfigureAwait(false);
                var itemId = placed.ItemId ?? throw new InvalidOperationException("The product API omitted a seeded item id.");
                if (firstItemId == Guid.Empty) firstItemId = itemId;
                await product.SendAsync(HttpMethod.Put, $"/api/back-office/content/items/{itemId}", token,
                    new { name = itemName, description = "Seeded for the shelf-at-scale check.", price }, cancellationToken).ConfigureAwait(false);
            }

            var assigned = new List<Guid>();
            if (requestedState != "never-published")
            {
                var screensStillNeeded = Enumerable.Range(index + 1, menuCount - index - 1)
                    .Count(candidate => ScaleState(candidate) != "never-published");
                var availableForThisMenu = Math.Max(0, screenIds.Count - nextScreen - screensStillNeeded);
                var take = index == 0 ? Math.Min(6, availableForThisMenu) : Math.Min(1, availableForThisMenu);
                for (var count = 0; count < take && nextScreen < screenIds.Count; count++, nextScreen++)
                {
                    await product.SendAsync(HttpMethod.Put, $"/api/back-office/content/screens/{screenIds[nextScreen]}/menu", token,
                        new { menuId = menu.Id, pageId = defaultPageId }, cancellationToken).ConfigureAwait(false);
                    assigned.Add(screenIds[nextScreen]);
                }
                if (assigned.Count > 0)
                    await product.SendAsync(HttpMethod.Post, $"/api/back-office/content/menus/{menu.Id}/publish", token, null, cancellationToken).ConfigureAwait(false);
            }

            var state = requestedState is "pending-changes" or "on-screens" && assigned.Count == 0
                ? "never-published"
                : requestedState;

            if (state == "pending-changes")
                await product.SendAsync(HttpMethod.Put, $"/api/back-office/content/items/{firstItemId}", token,
                    new { name = $"Scale item {index + 1:00}", description = "Seeded for the shelf-at-scale check.", price = "9.5" }, cancellationToken).ConfigureAwait(false);

            if (state == "put-away")
            {
                if (assigned.Count > 0)
                {
                    await product.SendAsync(HttpMethod.Delete, $"/api/back-office/content/menus/{menu.Id}/screens", token, null, cancellationToken).ConfigureAwait(false);
                    await product.SendAsync(HttpMethod.Post, $"/api/back-office/content/menus/{menu.Id}/publish", token, null, cancellationToken).ConfigureAwait(false);
                    assigned.Clear();
                }
                await product.SendAsync(HttpMethod.Put, $"/api/back-office/content/menus/{menu.Id}/put-away", token,
                    new { isPutAway = true }, cancellationToken).ConfigureAwait(false);
            }
            seeded.Add(new ScaleSeedMenu(menu.Id, name, state, assigned));
        }
        return new ScaleSeedResponse(session.VenueId, seeded, screenIds);
    }

    private static string ScaleState(int index) => index switch
    {
        0 => "on-screens",
        1 or 2 => "pending-changes",
        3 => "put-away",
        4 => "never-published",
        _ => "on-screens"
    };

    private async Task<SeededScreen> CreatePairedScreenAsync(string token, string name, int widthPixels, int heightPixels, CancellationToken cancellationToken)
    {
        var registered = await RegisterScreenAsync(name, widthPixels, heightPixels, cancellationToken).ConfigureAwait(false);
        var pairing = await product.SendPublicAsync<PairingCodeResponse>(HttpMethod.Post, "/api/screens/pairing-code",
            new { screenId = registered.ScreenId }, cancellationToken).ConfigureAwait(false);
        var claimed = await product.SendAsync<ClaimedScreenResponse>(HttpMethod.Post,
            $"/api/back-office/screens/pairing/{pairing.Code}/claim", token, null, cancellationToken).ConfigureAwait(false);
        if (!claimed.Linked || claimed.ScreenId != registered.ScreenId)
            throw new InvalidOperationException("The product API did not pair the registered test player.");
        return registered;
    }

    private async Task<SeededScreen> RegisterScreenAsync(string name, int widthPixels, int heightPixels, CancellationToken cancellationToken)
    {
        var registered = await product.SendPublicAsync<RegisteredScreenResponse>(HttpMethod.Post, "/api/screens",
            new { name, location = "Automated test", platform = "web", appVersion = "ui-test", widthPixels, heightPixels }, cancellationToken).ConfigureAwait(false);
        return new SeededScreen(registered.ScreenId, registered.ScreenKey);
    }
}
