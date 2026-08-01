using Microsoft.Extensions.Options;

namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerAuthenticationOptionsValidator : IValidateOptions<CustomerAuthenticationOptions>
{
    public ValidateOptionsResult Validate(string? name, CustomerAuthenticationOptions options)
    {
        var failures = new List<string>();
        ValidateProvider(options.Google, "Google", failures);
        ValidateProvider(options.Apple, "Apple", failures);
        if (options.EmailDelivery.Enabled &&
            (options.EmailDelivery.Endpoint is null || !options.EmailDelivery.Endpoint.IsAbsoluteUri ||
             options.EmailDelivery.Endpoint.Scheme != Uri.UriSchemeHttps))
            failures.Add("CustomerAuthentication:EmailDelivery:Endpoint must be an absolute HTTPS URI when enabled.");
        if (options.AbsoluteSessionLifetime <= TimeSpan.Zero || options.AbsoluteSessionLifetime > TimeSpan.FromDays(90))
            failures.Add("Absolute session lifetime must be between zero and 90 days.");
        if (options.IdleSessionLifetime <= TimeSpan.Zero || options.IdleSessionLifetime > options.AbsoluteSessionLifetime)
            failures.Add("Idle session lifetime must be positive and no longer than the absolute lifetime.");
        if (options.SessionTouchInterval <= TimeSpan.Zero || options.SessionTouchInterval >= options.IdleSessionLifetime)
            failures.Add("Session touch interval must be positive and shorter than the idle lifetime.");
        if (options.EmailLinkLifetime <= TimeSpan.Zero || options.EmailLinkLifetime > TimeSpan.FromHours(1))
            failures.Add("Email link lifetime must be between zero and one hour.");
        if (options.RecentAuthenticationWindow <= TimeSpan.Zero || options.RecentAuthenticationWindow > TimeSpan.FromHours(1))
            failures.Add("Recent-authentication window must be between zero and one hour.");
        if (string.IsNullOrWhiteSpace(options.Passkeys.ServerDomain) || options.Passkeys.ServerDomain.Contains("://", StringComparison.Ordinal))
            failures.Add("CustomerAuthentication:Passkeys:ServerDomain must be a host name without a scheme.");
        if (options.Passkeys.Origins.Count == 0 || options.Passkeys.Origins.Any(origin =>
            !Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps))
            failures.Add("CustomerAuthentication:Passkeys:Origins must contain absolute HTTPS origins.");
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateProvider(CustomerOidcProviderOptions provider, string name, List<string> failures)
    {
        if (!provider.Enabled) return;
        if (string.IsNullOrWhiteSpace(provider.ClientId))
            failures.Add($"CustomerAuthentication:{name}:ClientId is required when enabled.");
        if (string.IsNullOrWhiteSpace(provider.ClientSecret))
            failures.Add($"CustomerAuthentication:{name}:ClientSecret is required when enabled.");
    }
}
