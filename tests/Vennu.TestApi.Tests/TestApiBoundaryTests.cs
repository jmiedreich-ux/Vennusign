using System.Net;
using System.Text;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vennu.TestApi;
using Xunit;

namespace Vennu.TestApi.Tests;

public sealed class TestApiBoundaryTests
{
    [Fact]
    public async Task Missing_or_wrong_key_is_indistinguishable_and_refused()
    {
        var reached = false;
        var middleware = new TestApiAuthenticationMiddleware(_ => { reached = true; return Task.CompletedTask; });
        var options = Options.Create(new TestApiOptions { ApiKey = "expected" });

        foreach (var supplied in new[] { null, "wrong" })
        {
            reached = false;
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/test/seed";
            if (supplied is not null) context.Request.Headers["X-Vennusign-Test-Api-Key"] = supplied;
            await middleware.InvokeAsync(context, options);
            Assert.False(reached);
            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        }
    }

    [Fact]
    public async Task Correct_key_reaches_the_endpoint()
    {
        var reached = false;
        var middleware = new TestApiAuthenticationMiddleware(_ => { reached = true; return Task.CompletedTask; });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test/seed";
        context.Request.Headers["X-Vennusign-Test-Api-Key"] = "expected";
        await middleware.InvokeAsync(context, Options.Create(new TestApiOptions { ApiKey = "expected" }));
        Assert.True(reached);
    }

    [Fact]
    public async Task Product_client_sends_product_and_automation_credentials_only_to_the_product_api()
    {
        var handler = new RecordingHandler();
        var client = new ProductApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://product.invalid") },
            Options.Create(new TestApiOptions { ProductAutomationKey = "automation-secret" }));

        await client.SendAsync(HttpMethod.Post, "/ordinary", "venue-token", new { value = 1 }, CancellationToken.None);
        await client.SendAutomationAsync("/automation", new { value = 2 }, CancellationToken.None);

        Assert.Equal("venue-token", handler.Requests[0].Headers.GetValues("X-Vennusign-Back-Office-Token").Single());
        Assert.False(handler.Requests[0].Headers.Contains("X-Vennusign-Test-Automation-Key"));
        Assert.Equal("automation-secret", handler.Requests[1].Headers.GetValues("X-Vennusign-Test-Automation-Key").Single());
        Assert.False(handler.Requests[1].Headers.Contains("X-Vennusign-Back-Office-Token"));
    }

    [Fact]
    public void Deployable_has_no_project_or_package_dependency_on_product_data_layers()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Vennusign.sln"))) directory = directory.Parent;
        Assert.NotNull(directory);
        var project = File.ReadAllText(Path.Combine(directory!.FullName, "src", "Vennu.TestApi", "Vennu.TestApi.csproj"));
        Assert.DoesNotContain("ProjectReference", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Vennu.Data", project, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Vennu.Api", project, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Basic_seed_is_composed_from_real_product_routes_in_order()
    {
        var handler = new ProductContractHandler();
        var service = Service(handler);

        var result = await service.SeedAsync(new SeedRequest("venue-token", false, "behavior"), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.MenuId);
        Assert.Contains(handler.Paths, path => path == "GET /api/back-office/session");
        Assert.Contains(handler.Paths, path => path == "POST /api/back-office/menus");
        Assert.Contains(handler.Paths, path => path.Contains("/sections", StringComparison.Ordinal));
        Assert.Contains(handler.Paths, path => path.Contains("/items", StringComparison.Ordinal));
        Assert.All(handler.OrdinaryRequests, request =>
            Assert.Equal("venue-token", request.Headers.GetValues("X-Vennusign-Back-Office-Token").Single()));
    }

    [Fact]
    public async Task Four_menu_scale_seed_reserves_a_screen_for_each_published_state()
    {
        var handler = new ProductContractHandler();
        var result = await Service(handler).SeedScaleAsync(new ScaleSeedRequest("venue-token", 4, 4), CancellationToken.None);

        Assert.Equal(4, result.SeededMenus.Count);
        Assert.Equal("on-screens", result.SeededMenus.ElementAt(0).State);
        Assert.Equal("pending-changes", result.SeededMenus.ElementAt(1).State);
        Assert.Equal("pending-changes", result.SeededMenus.ElementAt(2).State);
        Assert.Equal("put-away", result.SeededMenus.ElementAt(3).State);
        Assert.Single(result.SeededMenus.ElementAt(0).ScreenIds);
        Assert.Single(result.SeededMenus.ElementAt(1).ScreenIds);
        Assert.Single(result.SeededMenus.ElementAt(2).ScreenIds);
        Assert.Empty(result.SeededMenus.ElementAt(3).ScreenIds);
        Assert.Equal(5, handler.Paths.Count(path => path.EndsWith("/publish", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Seed_builds_multiple_sections_with_known_library_names()
    {
        var handler = new ProductContractHandler();
        var names = new[] { "Harbor Lemonade", "Market Catch", "House Soda", "Daily Pie" };

        var result = await Service(handler).SeedAsync(
            new SeedRequest("venue-token", false, "shapes", 4, 1, names, PageCount: 2), CancellationToken.None);

        Assert.Equal(2, result.Pages!.Count);
        Assert.Equal(4, result.Sections!.Count);
        Assert.Equal(2, result.Sections.Select(section => section.PageId).Distinct().Count());
        Assert.Equal(names, result.Items!.Select(item => item.Name));
        Assert.Equal(4, handler.Paths.Count(path => path.EndsWith("/sections", StringComparison.Ordinal)));
        Assert.Equal(4, handler.Paths.Count(path => path.EndsWith("/items", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Never_paired_state_registers_a_real_player_without_claiming_it()
    {
        var handler = new ProductContractHandler();
        var result = await Service(handler).SeedAsync(
            new SeedRequest("venue-token", true, "player", ScreenState: ScreenSeedStates.NeverPaired), CancellationToken.None);

        Assert.Equal(ScreenSeedStates.NeverPaired, result.ScreenState);
        Assert.NotNull(result.ScreenKey);
        Assert.Contains("POST /api/screens", handler.Paths);
        Assert.DoesNotContain(handler.Paths, path => path.Contains("pairing-code", StringComparison.Ordinal));
        Assert.DoesNotContain(handler.Paths, path => path.Contains("/claim", StringComparison.Ordinal));
    }

    private static SeedService Service(HttpMessageHandler handler)
    {
        var options = Options.Create(new TestApiOptions { ProductAutomationKey = "automation" });
        return new SeedService(new ProductApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://product.invalid") }, options));
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var copy = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers) copy.Headers.TryAddWithoutValidation(header.Key, header.Value);
            if (request.Content is not null) copy.Content = new StringContent(await request.Content.ReadAsStringAsync(cancellationToken), Encoding.UTF8, "application/json");
            Requests.Add(copy);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }

    private sealed class ProductContractHandler : HttpMessageHandler
    {
        private readonly Guid venueId = Guid.NewGuid();
        private int menus;
        private int sections;
        private int items;
        private int screens;
        private readonly Guid defaultPageId = Guid.NewGuid();
        private int pages = 1;

        public List<string> Paths { get; } = [];
        public List<HttpRequestMessage> OrdinaryRequests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            Paths.Add($"{request.Method.Method} {path}");
            if (request.Headers.Contains("X-Vennusign-Back-Office-Token")) OrdinaryRequests.Add(request);

            object? body = (request.Method.Method, path) switch
            {
                ("GET", "/api/back-office/session") => new { venueId, organizationId = Guid.NewGuid() },
                ("POST", "/api/back-office/menus") => new { id = GuidFrom(ref menus), name = "menu" },
                ("GET", _) when path.EndsWith("/pages", StringComparison.Ordinal) =>
                    new[] { new { pageId = defaultPageId, name = "Page 1", sortOrder = 0 } },
                ("POST", _) when path.EndsWith("/pages", StringComparison.Ordinal) =>
                    new { pageId = GuidFrom(ref pages), name = "page", sortOrder = pages - 1 },
                ("POST", _) when path.EndsWith("/sections", StringComparison.Ordinal) =>
                    new { sectionId = GuidFrom(ref sections), name = "section", sortOrder = 0 },
                ("POST", _) when path.EndsWith("/items", StringComparison.Ordinal) =>
                    new { outcome = "created", itemId = GuidFrom(ref items), sectionId = Guid.NewGuid(), sortOrder = 0, itemCountOnMenu = 1 },
                ("POST", "/api/screens") =>
                    new { screenId = GuidFrom(ref screens), screenKey = $"sc-{screens:000000}" },
                ("POST", "/api/screens/pairing-code") =>
                    new { code = $"{screens:000000}", screenId = Guid.Parse($"00000000-0000-0000-0000-{screens:000000000000}"), expiresAt = DateTime.UtcNow.AddMinutes(5) },
                ("POST", _) when path.Contains("/pairing/", StringComparison.Ordinal) =>
                    new { linked = true, screenId = Guid.Parse($"00000000-0000-0000-0000-{screens:000000000000}"), venueId },
                _ => null
            };
            return Task.FromResult(body is null
                ? new HttpResponseMessage(HttpStatusCode.NoContent)
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(body) });
        }

        private static Guid GuidFrom(ref int counter)
        {
            counter++;
            return Guid.Parse($"00000000-0000-0000-0000-{counter:000000000000}");
        }
    }
}
