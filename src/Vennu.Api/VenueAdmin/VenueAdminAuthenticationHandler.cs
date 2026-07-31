using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Vennu.Api.VenueAdmin;

public sealed class VenueAdminAuthenticationHandler : AuthenticationHandler<VenueAdminAuthenticationOptions>
{
    public VenueAdminAuthenticationHandler(
        IOptionsMonitor<VenueAdminAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(VenueAdminAuthenticationDefaults.HeaderName, out var suppliedValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var suppliedToken = suppliedValues.ToString();
        var session = Options.Sessions.FirstOrDefault(candidate =>
            candidate.VenueId != Guid.Empty &&
            !string.IsNullOrWhiteSpace(candidate.AccessToken) &&
            TokensMatch(suppliedToken, candidate.AccessToken));
        if (session is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid venue access token."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId),
            new(ClaimTypes.Name, session.DisplayName),
            new(ClaimTypes.Role, "VenueAdmin"),
            new(VenueAdminAuthenticationDefaults.VenueIdClaim, session.VenueId.ToString())
        };
        claims.AddRange(session.Capabilities
            .Where(capability => !string.IsNullOrWhiteSpace(capability))
            .Select(capability => new Claim(
                VenueAdminAuthenticationDefaults.CapabilitiesClaim,
                capability.Trim().ToLowerInvariant())));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool TokensMatch(string suppliedToken, string configuredToken)
    {
        var suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(suppliedToken));
        var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredToken));
        return CryptographicOperations.FixedTimeEquals(suppliedHash, configuredHash);
    }
}
