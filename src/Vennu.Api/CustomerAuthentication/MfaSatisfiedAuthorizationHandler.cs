using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Vennu.Core.Models;

namespace Vennu.Api.CustomerAuthentication;

/// <summary>
/// Enforces decisions.md #6-#8 (docs/design/approved/authentication): every customer session must reach
/// Strong assurance before it satisfies this requirement, except a Passkey session, which is Strong on
/// login already (device + biometric/PIN is already two factors). RequireMfa lets dev/stage exempt this
/// entirely; CustomerAuthenticationOptionsValidator refuses to let Production set RequireMfa to false, so
/// there is no config value that turns this off in app.
/// </summary>
public sealed class MfaSatisfiedAuthorizationHandler(IOptions<CustomerAuthenticationOptions> options)
    : AuthorizationHandler<MfaSatisfiedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MfaSatisfiedRequirement requirement)
    {
        if (!options.Value.RequireMfa)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var method = context.User.FindFirst("auth_method")?.Value;
        if (string.Equals(method, nameof(CustomerAuthenticationMethod.Passkey), StringComparison.Ordinal))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var assurance = context.User.FindFirst("auth_assurance")?.Value;
        if (string.Equals(assurance, nameof(CustomerAuthenticationAssurance.Strong), StringComparison.Ordinal))
            context.Succeed(requirement);

        // No explicit Fail(): leaving the requirement unsatisfied (rather than failing the whole context)
        // lets a caller with other satisfied requirements on the same policy still be told "forbidden", not
        // "unauthenticated" - matches how CustomerAuthenticated already behaves for a missing/expired session.
        return Task.CompletedTask;
    }
}
