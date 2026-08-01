using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Vennu.Data.Services;

namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerSessionAuthenticationOptions : AuthenticationSchemeOptions;

public sealed class CustomerSessionAuthenticationHandler : AuthenticationHandler<CustomerSessionAuthenticationOptions>
{
    private readonly ICustomerSessionService sessionService;

    public CustomerSessionAuthenticationHandler(
        IOptionsMonitor<CustomerSessionAuthenticationOptions> options,
        ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder,
        ICustomerSessionService sessionService) : base(options, logger, encoder) =>
        this.sessionService = sessionService;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(CustomerAuthenticationDefaults.SessionCookieName, out var token) ||
            string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.NoResult();
        var identity = await sessionService.AuthenticateAsync(token, Context.RequestAborted).ConfigureAwait(false);
        if (identity is null) return AuthenticateResult.Fail("The customer session is invalid or expired.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, identity.User.Id.ToString()),
            new Claim(ClaimTypes.Name, identity.User.DisplayName),
            new Claim(ClaimTypes.Email, identity.User.Email),
            new Claim("auth_method", identity.Session.AuthenticationMethod.ToString()),
            new Claim("session_id", identity.Session.Id.ToString()),
            new Claim("auth_assurance", identity.Session.Assurance.ToString()),
            new Claim("auth_time", new DateTimeOffset(identity.Session.AuthenticatedUtc).ToUnixTimeSeconds().ToString())
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
