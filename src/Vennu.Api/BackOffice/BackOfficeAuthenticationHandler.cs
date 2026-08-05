using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;

namespace Vennu.Api.BackOffice;

public sealed class BackOfficeAuthenticationHandler : AuthenticationHandler<BackOfficeAuthenticationOptions>
{
    public BackOfficeAuthenticationHandler(
        IOptionsMonitor<BackOfficeAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var canonicalPresent = Request.Headers.TryGetValue(BackOfficeAuthenticationDefaults.HeaderName, out var canonicalValues);
        var legacyPresent = Request.Headers.TryGetValue(BackOfficeAuthenticationDefaults.LegacyHeaderName, out var legacyValues);
        if (!canonicalPresent && !legacyPresent)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (canonicalPresent && legacyPresent && !TokensMatch(canonicalValues.ToString(), legacyValues.ToString()))
        {
            return Task.FromResult(AuthenticateResult.Fail("Conflicting Back Office credentials were supplied."));
        }

        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        if (!Options.LegacySessionsEnabled || Options.LegacySessionsRetireAfterUtc is DateTime retireAt && retireAt <= utcNow)
            return Task.FromResult(AuthenticateResult.Fail("Legacy venue access has been retired."));

        var suppliedToken = canonicalPresent ? canonicalValues.ToString() : legacyValues.ToString();
        var session = Options.Sessions.FirstOrDefault(candidate =>
            candidate.Enabled &&
            candidate.RevokedUtc is null &&
            (candidate.ExpiresUtc is null || candidate.ExpiresUtc > utcNow) &&
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
            new(ClaimTypes.Role, "BackOffice"),
            new(BackOfficeAuthenticationDefaults.VenueIdClaim, session.VenueId.ToString()),
            new(BackOfficeAuthenticationDefaults.OrganizationIdClaim, session.OrganizationId.ToString()),
            new(BackOfficeAuthenticationDefaults.SystemRoleClaim, session.SystemRole),
            new(BackOfficeAuthenticationDefaults.AuthenticationSourceClaim, "legacy-config")
        };

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
