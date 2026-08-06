using Vennu.Api.BackOffice;

namespace Vennu.Api.Tests.BackOffice;

[Trait("Category", "Unit")]
public sealed class LegacyBackOfficeAuthenticationTests
{
    [Fact]
    public void Validator_RejectsDuplicateOrIncompleteEnabledEntries()
    {
        var options = new BackOfficeAuthenticationOptions
        {
            Sessions =
            [
                new() { AccessToken = "duplicate", VenueId = Guid.NewGuid(), UserId = "one" },
                new() { AccessToken = "duplicate", VenueId = Guid.Empty, UserId = "" }
            ]
        };

        var result = new BackOfficeAuthenticationOptionsValidator().Validate(null, options);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failures);
        Assert.Contains(result.Failures, failure => failure.Contains("unique", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Failures, failure => failure.Contains("venue ID", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Failures, failure => failure.Contains("organization ID", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validator_IgnoresDisabledCompatibilityEntries()
    {
        var result = new BackOfficeAuthenticationOptionsValidator().Validate(null, new BackOfficeAuthenticationOptions
        {
            LegacySessionsEnabled = false,
            Sessions = [new() { Enabled = false }]
        });

        Assert.True(result.Succeeded);
    }
}
