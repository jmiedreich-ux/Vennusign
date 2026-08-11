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
        var menuName = $"{label} menu {suffix}";
        var sectionName = $"{label} section {suffix}";
        var itemName = $"{label} item {suffix}";
        const string itemDescription = "Seeded for an automated UI test.";
        const decimal itemPrice = 4.50m;

        var menu = await product.SendAsync<MenuResponse>(
            HttpMethod.Post, "/api/back-office/menus", token, new { name = menuName }, cancellationToken).ConfigureAwait(false);
        var section = await product.SendAsync<SectionResponse>(
            HttpMethod.Post, $"/api/back-office/content/menus/{menu.Id}/sections", token, new { name = sectionName }, cancellationToken).ConfigureAwait(false);
        var placement = await product.SendAsync<PlaceResponse>(
            HttpMethod.Post,
            $"/api/back-office/content/menus/{menu.Id}/sections/{section.SectionId}/items",
            token,
            new { name = itemName },
            cancellationToken).ConfigureAwait(false);
        var itemId = placement.ItemId ?? throw new InvalidOperationException("The product API did not identify the item it created.");
        await product.SendAsync(
            HttpMethod.Put,
            $"/api/back-office/content/items/{itemId}",
            token,
            new { name = itemName, description = itemDescription, price = itemPrice.ToString(System.Globalization.CultureInfo.InvariantCulture) },
            cancellationToken).ConfigureAwait(false);

        Guid? screenId = null;
        if (request.IncludeScreen)
        {
            var screen = await product.SendAsync<ScreenResponse>(
                HttpMethod.Post,
                $"/api/back-office/venues/{session.VenueId}/screens",
                token,
                new { name = $"{label} screen {suffix}", location = "Automated test" },
                cancellationToken).ConfigureAwait(false);
            screenId = screen.Id;
        }

        return new SeedResponse(
            session.OrganizationId ?? Guid.Empty,
            session.VenueId,
            menu.Id,
            section.SectionId,
            itemId,
            menuName,
            sectionName,
            itemName,
            itemDescription,
            itemPrice,
            screenId,
            null);
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
            var screen = await product.SendAsync<ScreenResponse>(HttpMethod.Post,
                $"/api/back-office/venues/{session.VenueId}/screens", token,
                new { name = $"Scale screen {index + 1:00}", location = "Scale check" }, cancellationToken).ConfigureAwait(false);
            screenIds.Add(screen.Id);
        }

        var seeded = new List<ScaleSeedMenu>();
        var nextScreen = 0;
        for (var index = 0; index < Math.Clamp(request.Menus, 1, 40); index++)
        {
            var state = index switch { 0 => "on-screens", 1 or 2 => "pending-changes", 3 => "put-away", 4 => "never-published", _ => "on-screens" };
            var name = $"Scale menu {index + 1:00}";
            var menu = await product.SendAsync<MenuResponse>(HttpMethod.Post, "/api/back-office/menus", token, new { name }, cancellationToken)
                .ConfigureAwait(false);
            var section = await product.SendAsync<SectionResponse>(HttpMethod.Post,
                $"/api/back-office/content/menus/{menu.Id}/sections", token,
                new { name = index % 2 == 0 ? "Drinks" : "Snacks" }, cancellationToken).ConfigureAwait(false);

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
            if (state != "never-published")
            {
                var take = index == 0 ? Math.Min(6, screenIds.Count) : 1;
                for (var count = 0; count < take && nextScreen < screenIds.Count; count++, nextScreen++)
                {
                    await product.SendAsync(HttpMethod.Put, $"/api/back-office/content/screens/{screenIds[nextScreen]}/menu", token,
                        new { menuId = menu.Id }, cancellationToken).ConfigureAwait(false);
                    assigned.Add(screenIds[nextScreen]);
                }
                if (assigned.Count > 0)
                    await product.SendAsync(HttpMethod.Post, $"/api/back-office/content/menus/{menu.Id}/publish", token, null, cancellationToken).ConfigureAwait(false);
            }

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
}
