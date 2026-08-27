using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Vennu.Api.TestAutomation;
using Xunit;

namespace Vennu.Api.Tests.TestAutomation;

public sealed class TestAutomationAuthorizationTests
{
    private static readonly Guid AllowedVenue = Guid.Parse("73000000-0000-0000-0000-000000000002");

    [Fact]
    public void Requires_identity_scope_and_allowlisted_venue_together()
    {
        var configured = new TestAutomationOptions
        {
            ApiKey = "secret",
            Scopes = ["venue.reset"],
            ResetVenueIds = [AllowedVenue],
            AvailabilityVenueIds = [Guid.Parse("73000000-0000-0000-0000-000000000001")]
        };
        var authorization = new TestAutomationAuthorization(Options.Create(configured));

        Assert.False(authorization.Allows(Request(), "venue.reset", AllowedVenue));
        Assert.False(authorization.Allows(Request("wrong"), "venue.reset", AllowedVenue));
        Assert.False(authorization.Allows(Request("secret"), "availability.backdate", AllowedVenue));
        Assert.False(authorization.Allows(Request("secret"), "venue.reset", Guid.NewGuid()));
        Assert.True(authorization.Allows(Request("secret"), "venue.reset", AllowedVenue));
    }

    /*
     * Headroom has its own scope and its own allowlist, and this is why.
     *
     * The UI suite fills ONE shared venue - 98 seeds against a ceiling of 50 - so that venue is the
     * only one that ever needed its ceiling raised. It is deliberately NOT on the reset allowlist,
     * because reset WIPES a venue and tests run against that one in parallel.
     *
     * The first attempt reused the reset scope for headroom. Every call was refused, the suite
     * filled up exactly as before, and it cost two CI runs to see it. Reusing it the other way -
     * putting the shared venue on the reset list - would have been far worse: every seed would have
     * gained the power to wipe a venue other tests were using.
     */
    [Fact]
    public void Headroom_is_allowed_where_reset_is_not_and_neither_borrows_the_other()
    {
        var shared = Guid.Parse("73000000-0000-0000-0000-000000000001");
        var configured = new TestAutomationOptions
        {
            ApiKey = "secret",
            Scopes = ["venue.reset", "venue.headroom"],
            ResetVenueIds = [AllowedVenue],
            HeadroomVenueIds = [shared, AllowedVenue]
        };
        var authorization = new TestAutomationAuthorization(Options.Create(configured));

        // The shared venue: room yes, wiping no.
        Assert.True(authorization.Allows(Request("secret"), "venue.headroom", shared));
        Assert.False(authorization.Allows(Request("secret"), "venue.reset", shared));

        // The scale venue keeps both, because one test owns it at a time.
        Assert.True(authorization.Allows(Request("secret"), "venue.reset", AllowedVenue));
        Assert.True(authorization.Allows(Request("secret"), "venue.headroom", AllowedVenue));

        // And the scope still has to be granted at all.
        var withoutScope = new TestAutomationOptions
        {
            ApiKey = "secret",
            Scopes = ["venue.reset"],
            HeadroomVenueIds = [shared]
        };
        Assert.False(new TestAutomationAuthorization(Options.Create(withoutScope))
            .Allows(Request("secret"), "venue.headroom", shared));
    }

    // The env script grants what the code asks for. These drifted apart once already - the endpoint
    // asked for "venue.reset" while the shared venue was only ever on the availability and history
    // lists - and nothing failed until the suite had run for half an hour.
    [Fact]
    public void TheEnvironmentScriptGrantsTheHeadroomScopeForTheSharedVenue()
    {
        var script = File.ReadAllText(Path.Combine(RepositoryRoot(), "scripts", "start-ui-test-env.ps1"));

        Assert.Contains("TestAutomation__Scopes__3 = 'venue.headroom'", script, StringComparison.Ordinal);
        Assert.Contains("TestAutomation__HeadroomVenueIds__0 = (& $guid '73000000-0000-0000-0000-000000000001')", script, StringComparison.Ordinal);
        // Reset stays off the shared venue. If this ever passes, something has handed every seed
        // the ability to wipe a venue other tests are using.
        Assert.DoesNotContain("TestAutomation__ResetVenueIds__0 = (& $guid '73000000-0000-0000-0000-000000000001')", script, StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "scripts"))) directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
    }

    private static HttpRequest Request(string? key = null)
    {
        var request = new DefaultHttpContext().Request;
        if (key is not null) request.Headers[TestAutomationOptions.HeaderName] = key;
        return request;
    }
}
