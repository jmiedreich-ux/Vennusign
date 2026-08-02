using Microsoft.Extensions.Options;

namespace Vennu.Api.VenueAdmin;

public sealed class VenueAdminAuthenticationOptionsValidator : IValidateOptions<VenueAdminAuthenticationOptions>
{
    public ValidateOptionsResult Validate(string? name, VenueAdminAuthenticationOptions options)
    {
        var failures = new List<string>();
        foreach (var session in options.Sessions.Where(session => session.Enabled))
        {
            if (string.IsNullOrWhiteSpace(session.AccessToken)) failures.Add("Every enabled legacy venue session requires an access token.");
            if (session.VenueId == Guid.Empty) failures.Add("Every enabled legacy venue session requires a venue ID.");
            if (string.IsNullOrWhiteSpace(session.UserId)) failures.Add("Every enabled legacy venue session requires a user ID.");
        }
        var duplicateTokens = options.Sessions.Where(session => session.Enabled && !string.IsNullOrWhiteSpace(session.AccessToken))
            .GroupBy(session => session.AccessToken, StringComparer.Ordinal).Any(group => group.Count() > 1);
        if (duplicateTokens) failures.Add("Enabled legacy venue access tokens must be unique.");
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
