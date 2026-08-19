using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerOidcEvents(
    ICustomerAccountService accountService,
    ICustomerSessionService sessionService) : OpenIdConnectEvents
{
    public override Task RedirectToIdentityProvider(RedirectContext context)
    {
        if (context.Scheme.Name == CustomerAuthenticationDefaults.EntraScheme &&
            context.Properties is not null &&
            context.Properties.Items.TryGetValue("intent", out var intent) &&
            intent == "signup")
        {
            context.ProtocolMessage.SetParameter("prompt", "create");
        }
        return Task.CompletedTask;
    }

    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var provider = context.Scheme.Name switch
        {
            CustomerAuthenticationDefaults.GoogleScheme => ExternalIdentityProvider.Google,
            CustomerAuthenticationDefaults.AppleScheme => ExternalIdentityProvider.Apple,
            CustomerAuthenticationDefaults.EntraScheme => ExternalIdentityProvider.Vennusign,
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
        var method = provider switch
        {
            ExternalIdentityProvider.Google => CustomerAuthenticationMethod.Google,
            ExternalIdentityProvider.Apple => CustomerAuthenticationMethod.Apple,
            ExternalIdentityProvider.Vennusign => CustomerAuthenticationMethod.Vennusign,
            _ => throw new InvalidOperationException("Unsupported external identity provider.")
        };
        var session = await sessionService.IssueAsync(user.Id, method, context.HttpContext.RequestAborted).ConfigureAwait(false);
        CustomerSessionCookie.Append(context.Response, session.Token, session.Session.ExpiresUtc);
    }
}
