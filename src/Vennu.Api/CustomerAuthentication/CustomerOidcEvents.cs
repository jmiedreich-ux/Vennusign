using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerOidcEvents(
    ICustomerAccountService accountService,
    ICustomerSessionService sessionService) : OpenIdConnectEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var provider = context.Scheme.Name switch
        {
            CustomerAuthenticationDefaults.GoogleScheme => ExternalIdentityProvider.Google,
            CustomerAuthenticationDefaults.AppleScheme => ExternalIdentityProvider.Apple,
            _ => throw new InvalidOperationException("Unsupported customer OIDC scheme.")
        };
        var subject = context.Principal?.FindFirstValue("sub");
        var email = context.Principal?.FindFirstValue("email");
        var verifiedValue = context.Principal?.FindFirstValue("email_verified");
        var emailVerified = bool.TryParse(verifiedValue, out var verified) && verified;
        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email) || !emailVerified)
        {
            context.Fail("The provider did not return a verified customer identity.");
            return;
        }

        var user = await accountService.ResolveExternalIdentityAsync(new ExternalIdentityProfile(
            provider,
            subject,
            email,
            true,
            context.Principal?.FindFirstValue("name") ?? email), context.HttpContext.RequestAborted).ConfigureAwait(false);
        var method = provider == ExternalIdentityProvider.Google
            ? CustomerAuthenticationMethod.Google
            : CustomerAuthenticationMethod.Apple;
        var session = await sessionService.IssueAsync(user.Id, method, context.HttpContext.RequestAborted).ConfigureAwait(false);
        CustomerSessionCookie.Append(context.Response, session.Token, session.Session.ExpiresUtc);
    }
}
