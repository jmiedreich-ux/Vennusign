using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Vennu.Api.Admin;

public sealed class SuperAdminAuthenticationHandler : AuthenticationHandler<SuperAdminAuthenticationOptions>
{
    public SuperAdminAuthenticationHandler(
        IOptionsMonitor<SuperAdminAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(SuperAdminAuthenticationDefaults.HeaderName, out var suppliedValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var suppliedKey = suppliedValues.ToString();
        var configuredKey = Options.ApiKey;
        if (string.IsNullOrWhiteSpace(configuredKey) || !KeysMatch(suppliedKey, configuredKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Super Admin API key."));
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "super-admin"),
                new Claim(ClaimTypes.Name, "Super Admin"),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            ],
            Scheme.Name);
        identity.AddClaims(Options.ConfigurationPermissions
            .Where(permission => !string.IsNullOrWhiteSpace(permission))
            .Select(permission => new Claim("vennu:configuration_permission", permission.Trim().ToLowerInvariant())));
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

