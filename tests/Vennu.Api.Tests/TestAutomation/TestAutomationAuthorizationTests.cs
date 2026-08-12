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

    private static HttpRequest Request(string? key = null)
    {
        var request = new DefaultHttpContext().Request;
        if (key is not null) request.Headers[TestAutomationOptions.HeaderName] = key;
        return request;
    }
}
