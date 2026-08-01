using System.Security.Claims;
using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vennu.Api.CustomerAuthentication;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/customer-auth/strong")]
public sealed class CustomerStrongAuthenticationController(
    ICustomerPasskeyService passkeys,
    ICustomerStrongAuthenticationService strongAuthentication,
    ICustomerSessionService sessions) : ControllerBase
{
    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("passkeys/registration/options")]
    public async Task<IActionResult> BeginPasskeyRegistration(CancellationToken cancellationToken)
    {
        var identity = await RequireRecentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null) return StatusCode(StatusCodes.Status428PreconditionRequired, "Recent authentication is required.");
        return Ok(await passkeys.BeginRegistrationAsync(identity.Session.UserId, cancellationToken).ConfigureAwait(false));
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("passkeys/registration/complete")]
    public async Task<IActionResult> CompletePasskeyRegistration(PasskeyRegistrationRequest request, CancellationToken cancellationToken)
    {
        var identity = await RequireRecentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null) return StatusCode(StatusCodes.Status428PreconditionRequired, "Recent authentication is required.");
        await passkeys.CompleteRegistrationAsync(identity.Session.UserId, request.ChallengeId, request.DisplayName, request.Response, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("passkeys/assertion/options")]
    public async Task<IActionResult> BeginPasskeyAssertion(PasskeyAssertionOptionsRequest request, CancellationToken cancellationToken) =>
        (await passkeys.BeginAssertionAsync(request.Email, cancellationToken).ConfigureAwait(false)) is { } result ? Ok(result) : Unauthorized();

    [HttpPost("passkeys/assertion/complete")]
    public async Task<IActionResult> CompletePasskeyAssertion(PasskeyAssertionRequest request, CancellationToken cancellationToken)
    {
        var result = await passkeys.CompleteAssertionAsync(request.ChallengeId, request.Response, cancellationToken).ConfigureAwait(false);
        if (result is null) return Unauthorized();
        CustomerSessionCookie.Append(Response, result.Token, result.Session.ExpiresUtc);
        return Ok(new { result.User.Id, result.User.Email, result.User.DisplayName, AuthenticationMethod = "Passkey", Assurance = "Strong" });
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("totp/enrollment")]
    public async Task<IActionResult> BeginTotpEnrollment(CancellationToken cancellationToken)
    {
        var identity = await RequireRecentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null) return StatusCode(StatusCodes.Status428PreconditionRequired, "Recent authentication is required.");
        return Ok(await strongAuthentication.BeginTotpEnrollmentAsync(identity.User.Id, identity.User.Email, cancellationToken).ConfigureAwait(false));
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("totp/enrollment/complete")]
    public async Task<IActionResult> CompleteTotpEnrollment(CodeRequest request, CancellationToken cancellationToken)
    {
        var identity = await RequireSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null) return Unauthorized();
        var codes = await strongAuthentication.CompleteTotpEnrollmentAsync(identity.User.Id, request.Code, cancellationToken).ConfigureAwait(false);
        return codes is null ? Unauthorized() : Ok(new { RecoveryCodes = codes });
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("step-up/totp")]
    public async Task<IActionResult> StepUpTotp(CodeRequest request, CancellationToken cancellationToken)
    {
        var identity = await RequireSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null || !await strongAuthentication.VerifyTotpAsync(identity.User.Id, request.Code, cancellationToken).ConfigureAwait(false)) return Unauthorized();
        return await sessions.StepUpAsync(identity.Session.Id, Vennu.Core.Models.CustomerAuthenticationMethod.Totp, cancellationToken).ConfigureAwait(false) ? NoContent() : Unauthorized();
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPost("step-up/recovery-code")]
    public async Task<IActionResult> StepUpRecoveryCode(CodeRequest request, CancellationToken cancellationToken)
    {
        var identity = await RequireSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null || !await strongAuthentication.RedeemRecoveryCodeAsync(identity.User.Id, request.Code, cancellationToken).ConfigureAwait(false)) return Unauthorized();
        return await sessions.StepUpAsync(identity.Session.Id, Vennu.Core.Models.CustomerAuthenticationMethod.RecoveryCode, cancellationToken).ConfigureAwait(false) ? NoContent() : Unauthorized();
    }

    private async Task<CustomerSessionIdentity?> RequireRecentSessionAsync(CancellationToken cancellationToken)
    {
        var identity = await RequireSessionAsync(cancellationToken).ConfigureAwait(false);
        return identity is not null && sessions.IsRecent(identity.Session) ? identity : null;
    }
    private Task<CustomerSessionIdentity?> RequireSessionAsync(CancellationToken cancellationToken) =>
        Request.Cookies.TryGetValue(CustomerAuthenticationDefaults.SessionCookieName, out var token) && !string.IsNullOrWhiteSpace(token)
            ? sessions.AuthenticateAsync(token, cancellationToken) : Task.FromResult<CustomerSessionIdentity?>(null);
}

public sealed record PasskeyRegistrationRequest(Guid ChallengeId, string DisplayName, AuthenticatorAttestationRawResponse Response);
public sealed record PasskeyAssertionOptionsRequest(string Email);
public sealed record PasskeyAssertionRequest(Guid ChallengeId, AuthenticatorAssertionRawResponse Response);
public sealed record CodeRequest(string Code);
