using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Vennu.Api.Tests.Controllers;

/// <summary>
/// The safety boundary around the one endpoint in this system that deletes a
/// venue's content.
///
/// <c>POST api/test/seed/scale</c> clears every menu, item, placement, screen,
/// assignment, publish event and history entry in the venue it seeds. The
/// independent review found the original guard was a denylist of ONE venue behind
/// an <c>IsDevelopment()</c> check — so any enabled session token authorised
/// erasing its own venue, and a remote slot merely carrying the Development
/// setting was enough to reach it.
///
/// These run in the <b>Development</b> environment on purpose. The shared test
/// factory boots as "Testing", where the first guard short-circuits everything —
/// so a test written against it would pass without exercising a single one of the
/// guards that actually matter. That is the shape of test this milestone's review
/// called theatre, and it is worth avoiding on the one destructive path.
///
/// Integration, because a Development host runs migrations at startup: they point
/// at the same LocalDB database the rest of the integration suite uses, where
/// those migrations are already applied.
/// </summary>
[Trait("Category", "Integration")]
public sealed class TestSeedScaleBoundaryTests
{
    /// <summary>The only venue the action will ever clear — the scale fixture's own.</summary>
    private const string ScaleVenueId = "73000000-0000-0000-0000-000000000002";

    /// <summary>The shared acceptance venue every other spec depends on.</summary>
    private const string SharedVenueId = "73000000-0000-0000-0000-000000000001";

    private const string ScaleToken = "boundary-scale-token";
    private const string SharedToken = "boundary-shared-token";

    /// <summary>
    /// A venue that is neither the scale fixture nor the shared one — a customer
    /// venue, as far as this endpoint is concerned.
    ///
    /// It exists because the first version of this test used the SHARED venue,
    /// which the original denylist also refused: the test passed against the
    /// defect it was written to catch. An allowlist is only distinguishable from
    /// a denylist by a venue that is on neither list.
    /// </summary>
    private const string ArbitraryVenueId = "79000000-0000-0000-0000-0000000000ff";
    private const string ArbitraryToken = "boundary-arbitrary-token";

    private sealed class DevelopmentHost(bool scaleSeedEnabled) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Development, so the environment guard is satisfied and the guards
            // under test are the ones deciding.
            builder.UseEnvironment("Development");
            builder.UseSetting(
                "ConnectionStrings:VennuDatabase",
                @"Server=(localdb)\MSSQLLocalDB;Database=vennusign_dev_tests;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;");
            builder.UseSetting("PlatformOperations:ApiKey", "boundary-admin-key");
            builder.UseSetting("Stripe:Webhook:SigningSecret", "whsec_test");

            if (scaleSeedEnabled)
            {
                builder.UseSetting("TestSupport:ScaleSeedEnabled", "true");
            }

            // Two enabled tokens: one on the scale venue, one on the shared venue
            // every other spec depends on. Both are real credentials; only one of
            // them may ever reach the destructive path.
            builder.UseSetting("BackOffice:Sessions:0:AccessToken", ScaleToken);
            builder.UseSetting("BackOffice:Sessions:0:VenueId", ScaleVenueId);
            builder.UseSetting("BackOffice:Sessions:0:OrganizationId", "72000000-0000-0000-0000-000000000001");
            builder.UseSetting("BackOffice:Sessions:0:UserId", "71000000-0000-0000-0000-000000000004");
            builder.UseSetting("BackOffice:Sessions:0:DisplayName", "Boundary Scale");
            builder.UseSetting("BackOffice:Sessions:0:SystemRole", "organization_owner");

            builder.UseSetting("BackOffice:Sessions:1:AccessToken", SharedToken);
            builder.UseSetting("BackOffice:Sessions:1:VenueId", SharedVenueId);
            builder.UseSetting("BackOffice:Sessions:1:OrganizationId", "72000000-0000-0000-0000-000000000001");
            builder.UseSetting("BackOffice:Sessions:1:UserId", "71000000-0000-0000-0000-000000000001");
            builder.UseSetting("BackOffice:Sessions:1:DisplayName", "Boundary Shared");
            builder.UseSetting("BackOffice:Sessions:1:SystemRole", "organization_owner");

            builder.UseSetting("BackOffice:Sessions:2:AccessToken", ArbitraryToken);
            builder.UseSetting("BackOffice:Sessions:2:VenueId", ArbitraryVenueId);
            builder.UseSetting("BackOffice:Sessions:2:OrganizationId", "72000000-0000-0000-0000-000000000009");
            builder.UseSetting("BackOffice:Sessions:2:UserId", "71000000-0000-0000-0000-000000000009");
            builder.UseSetting("BackOffice:Sessions:2:DisplayName", "Boundary Arbitrary");
            builder.UseSetting("BackOffice:Sessions:2:SystemRole", "organization_owner");
        }
    }

    private static Task<HttpResponseMessage> SeedScaleAsync(HttpClient client, string? token) =>
        client.PostAsJsonAsync("/api/test/seed/scale", new { accessToken = token, menus = 2, screens = 2 });

    [Fact]
    public async Task InDevelopment_ItIsStillAbsentUnlessDeliberatelyEnabled()
    {
        // The environment is right and the token is valid. It is still 404,
        // because nothing turned the opt-in on — which is the guard that stops a
        // deployed slot carrying the Development setting from reaching it.
        await using var host = new DevelopmentHost(scaleSeedEnabled: false);
        using var client = host.CreateClient();

        using var response = await SeedScaleAsync(client, ScaleToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Enabled_ItStillRefusesEveryVenueButTheScaleFixture()
    {
        // Right environment, opt-in on, a genuine enabled token — and refused,
        // because the venue is not the one venue that exists to be cleared. An
        // allowlist refuses the venues nobody thought of; a denylist only refuses
        // the ones somebody remembered.
        await using var host = new DevelopmentHost(scaleSeedEnabled: true);
        using var client = host.CreateClient();

        // Both the shared venue AND an arbitrary customer-shaped venue. The
        // arbitrary one is the case that matters: the original denylist would have
        // cleared it, so it is the only assertion here that can tell an allowlist
        // from a denylist.
        foreach (var token in new[] { SharedToken, ArbitraryToken })
        {
            using var response = await SeedScaleAsync(client, token);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("scale fixture venue", body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Enabled_ItRefusesAnUnknownTokenAndNoTokenIdentically()
    {
        await using var host = new DevelopmentHost(scaleSeedEnabled: true);
        using var client = host.CreateClient();

        using var unknown = await SeedScaleAsync(client, "not-a-configured-token");
        using var absent = await SeedScaleAsync(client, null);

        // Reported identically, so the endpoint never confirms which tokens exist.
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, absent.StatusCode);
    }

    [Fact]
    public void TheAllowlistNamesTheScaleFixtureAndNotTheSharedVenue()
    {
        // Guards the constant itself. A copy-paste pointing the allowlist at the
        // shared venue would leave every test above passing while the endpoint
        // cleared the venue the whole suite depends on.
        var source = File.ReadAllText(FindSeedController());

        Assert.Contains($"Guid.Parse(\"{ScaleVenueId}\")", source, StringComparison.Ordinal);
        Assert.DoesNotContain($"Guid.Parse(\"{SharedVenueId}\")", source, StringComparison.Ordinal);
    }

    private static string FindSeedController()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName, "src", "Vennu.Api", "Controllers", "TestSupport", "TestSeedController.cs");
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not locate TestSeedController.cs from the test output directory.");
    }
}
