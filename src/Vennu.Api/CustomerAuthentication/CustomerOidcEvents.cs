using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Api.CustomerAuthentication;

public sealed class CustomerOidcEvents(
    ICustomerAccountService accountService,
    ICustomerSessionService sessionService,
    ILogger<CustomerOidcEvents> logger) : OpenIdConnectEvents
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

    /// <summary>
    /// Identity resolution runs in <see cref="RemoteAuthenticationEvents.TicketReceived"/>, not
    /// <see cref="OpenIdConnectEvents.TokenValidated"/>, and the difference is load-bearing:
    /// TokenValidated fires straight after ID-token validation, *before* the handler calls the
    /// UserInfo endpoint and merges its claims. Entra External ID's ID token carries no
    /// <c>email</c> claim - it only supplies the address via UserInfo
    /// (https://graph.microsoft.com/oidc/userinfo) - so a check for <c>email</c> in
    /// TokenValidated rejected every Entra sign-in no matter how the tenant, app registration,
    /// optional claims, or Graph permissions were configured. TicketReceived is the last event
    /// before the handler completes, after UserInfo claims are in the principal for every
    /// provider that uses it.
    /// </summary>
    public override async Task TicketReceived(TicketReceivedContext context)
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
            // Which of the three conditions failed is otherwise invisible - the browser only
            // ever sees one generic message - and getting this wrong costs a deploy cycle to
            // re-diagnose. Claim *types* only, never values: these carry customer PII.
            logger.LogWarning(
                "Customer sign-in rejected for {Provider}: hasSubject={HasSubject}, hasEmail={HasEmail}, emailVerified={EmailVerified}, claimTypes={ClaimTypes}",
                provider,
                !string.IsNullOrWhiteSpace(subject),
                !string.IsNullOrWhiteSpace(email),
                emailVerified,
                string.Join(",", context.Principal?.Claims.Select(claim => claim.Type).Distinct() ?? []));
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
