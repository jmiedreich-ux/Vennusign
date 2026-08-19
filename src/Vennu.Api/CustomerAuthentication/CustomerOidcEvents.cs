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

    /// <summary>
    /// Whether the address a provider returned can be treated as verified.
    ///
    /// Google and Apple are third-party providers, so we require them to assert
    /// <c>email_verified</c> before trusting the address they hand us. Entra External ID is
    /// Vennusign's own provider - "Sign in with Vennusign" is its local-account flow, which
    /// proves control of the address with an emailed code before the account can exist - and
    /// it does not emit an <c>email_verified</c> claim at all. Requiring one there rejected
    /// every valid local-account sign-in with "The provider did not return a verified customer
    /// identity", which is why this is keyed on the provider instead of applied flat.
    /// </summary>
    public static bool HasVerifiedEmail(ExternalIdentityProvider provider, string? emailVerifiedClaim) =>
        provider == ExternalIdentityProvider.Vennusign
        || (bool.TryParse(emailVerifiedClaim, out var verified) && verified);

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
        var emailVerified = HasVerifiedEmail(provider, context.Principal?.FindFirstValue("email_verified"));
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
