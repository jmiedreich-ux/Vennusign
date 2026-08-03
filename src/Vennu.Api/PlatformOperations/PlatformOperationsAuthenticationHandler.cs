using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Vennu.Api.PlatformOperations;

public sealed class PlatformOperationsAuthenticationHandler : AuthenticationHandler<PlatformOperationsAuthenticationOptions>
{
    public PlatformOperationsAuthenticationHandler(
        IOptionsMonitor<PlatformOperationsAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var canonicalPresent = Request.Headers.TryGetValue(PlatformOperationsAuthenticationDefaults.HeaderName, out var canonicalValues);
        var legacyPresent = Request.Headers.TryGetValue(PlatformOperationsAuthenticationDefaults.LegacyHeaderName, out var legacyValues);
        if (!canonicalPresent && !legacyPresent)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (canonicalPresent && legacyPresent && !KeysMatch(canonicalValues.ToString(), legacyValues.ToString()))
        {
            return Task.FromResult(AuthenticateResult.Fail("Conflicting Platform Operations credentials were supplied."));
        }

        var suppliedKey = canonicalPresent ? canonicalValues.ToString() : legacyValues.ToString();
        var configuredKey = Options.ApiKey;
        if (string.IsNullOrWhiteSpace(configuredKey) || !KeysMatch(suppliedKey, configuredKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Platform Operations API key."));
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "platform-operations"),
                new Claim(ClaimTypes.Name, "Platform Operations"),
                new Claim(ClaimTypes.Role, "PlatformOperations")
            ],
            Scheme.Name);
        identity.AddClaims(Options.ConfigurationPermissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => new Claim("vennusign:configuration_permission", permission.Trim().ToLowerInvariant())));
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool KeysMatch(string suppliedKey, string configuredKey)
    {
        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(suppliedKey));
        var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
        return CryptographicOperations.FixedTimeEquals(suppliedHash, configuredHash);
    }
}
