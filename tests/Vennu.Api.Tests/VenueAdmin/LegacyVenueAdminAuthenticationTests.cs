using Vennu.Api.VenueAdmin;

namespace Vennu.Api.Tests.VenueAdmin;

[Trait("Category", "Unit")]
public sealed class LegacyVenueAdminAuthenticationTests
{
    [Fact]
    public void Validator_RejectsDuplicateOrIncompleteEnabledEntries()
    {
        var options = new VenueAdminAuthenticationOptions
        {
            Sessions =
            [
                new() { AccessToken = "duplicate", VenueId = Guid.NewGuid(), UserId = "one" },
                new() { AccessToken = "duplicate", VenueId = Guid.Empty, UserId = "" }
            ]
        };

        var result = new VenueAdminAuthenticationOptionsValidator().Validate(null, options);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Failures, failure => failure.Contains("unique", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Failures, failure => failure.Contains("venue ID", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validator_IgnoresDisabledCompatibilityEntries()
    {
        var result = new VenueAdminAuthenticationOptionsValidator().Validate(null, new VenueAdminAuthenticationOptions
        {
            LegacySessionsEnabled = false,
            Sessions = [new() { Enabled = false }]
        });

        Assert.True(result.Succeeded);
    }
}
