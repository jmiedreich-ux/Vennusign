using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerAuthenticationOptionsValidator : IValidateOptions<CustomerAuthenticationOptions>
{
    private readonly bool isDevelopment;
    private readonly bool isProduction;

    public CustomerAuthenticationOptionsValidator() : this(false, false) { }
    public CustomerAuthenticationOptionsValidator(IHostEnvironment environment)
        : this(environment.IsDevelopment(), environment.IsProduction()) { }
    public CustomerAuthenticationOptionsValidator(bool isDevelopment, bool isProduction = false)
    {
        this.isDevelopment = isDevelopment;
        this.isProduction = isProduction;
    }

    public ValidateOptionsResult Validate(string? name, CustomerAuthenticationOptions options)
    {
        var failures = new List<string>();
        if (!CustomerReturnUri.IsValidOrigin(options.FrontendOrigin))
            failures.Add("CustomerAuthentication:FrontendOrigin must be an absolute HTTPS origin without a path, query, fragment, or user information.");
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
        var serverDomain = options.Passkeys.ServerDomain.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(serverDomain) || serverDomain.Contains("://", StringComparison.Ordinal) ||
            serverDomain.Contains('*') || serverDomain.Contains('/') || !Uri.CheckHostName(serverDomain).Equals(UriHostNameType.Dns))
            failures.Add("CustomerAuthentication:Passkeys:ServerDomain must be a host name without a scheme.");
        if (options.Passkeys.Origins.Count == 0 || options.Passkeys.Origins.Any(origin => !ValidPasskeyOrigin(origin, serverDomain)))
            failures.Add("CustomerAuthentication:Passkeys:Origins must contain exact HTTPS origins within the relying-party domain.");
        if (!isDevelopment && (serverDomain == "localhost" || options.Passkeys.Origins.Any(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback)))
            failures.Add("Localhost passkey relying-party settings are accepted only in Development.");
        if (isProduction && !options.RequireMfa)
            failures.Add("CustomerAuthentication:RequireMfa cannot be false in Production. Per decisions.md #7-#8 (docs/design/approved/authentication), this is a structural guarantee, not a default: the app refuses to start rather than run app/production without mandatory MFA.");
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool ValidPasskeyOrigin(string origin, string serverDomain)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo) || uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) || origin.Contains('*')) return false;
        var host = uri.Host.ToLowerInvariant();
        return host == serverDomain || host.EndsWith('.' + serverDomain, StringComparison.Ordinal);
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
