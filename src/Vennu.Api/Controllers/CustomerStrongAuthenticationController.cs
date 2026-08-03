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
    [HttpGet("passkeys")]
    public async Task<IActionResult> ListPasskeys(CancellationToken cancellationToken)
    {
        var identity = await RequireSessionAsync(cancellationToken).ConfigureAwait(false);
        return identity is null ? Unauthorized() : Ok(await passkeys.ListAsync(identity.User.Id, cancellationToken).ConfigureAwait(false));
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpPut("passkeys/{id:guid}")]
    public async Task<IActionResult> RenamePasskey(Guid id, PasskeyNameRequest request, CancellationToken cancellationToken)
    {
        var identity = await RequireRecentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null) return StatusCode(StatusCodes.Status428PreconditionRequired, "Recent authentication is required.");
        try { await passkeys.RenameAsync(identity.User.Id, id, request.DisplayName, cancellationToken).ConfigureAwait(false); return NoContent(); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpDelete("passkeys/{id:guid}")]
    public async Task<IActionResult> RemovePasskey(Guid id, CancellationToken cancellationToken)
    {
        var identity = await RequireRecentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (identity is null) return StatusCode(StatusCodes.Status428PreconditionRequired, "Recent authentication is required.");
        try { await passkeys.RemoveAsync(identity.User.Id, id, cancellationToken).ConfigureAwait(false); return NoContent(); }
        catch (KeyNotFoundException exception) { return NotFound(exception.Message); }
        catch (InvalidOperationException exception) { return Conflict(exception.Message); }
    }

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
        try
        {
            await passkeys.CompleteRegistrationAsync(identity.Session.UserId, request.ChallengeId, request.DisplayName, request.Response, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException exception) { return BadRequest(exception.Message); }
        catch (UnauthorizedAccessException) { return Unauthorized("The passkey request expired or is no longer valid. Start again."); }
        catch (Fido2VerificationException) { return Unauthorized("The passkey could not be verified. Start again or use account recovery."); }
    }

    [HttpPost("passkeys/assertion/options")]
    public async Task<IActionResult> BeginPasskeyAssertion(PasskeyAssertionOptionsRequest request, CancellationToken cancellationToken) =>
        (await passkeys.BeginAssertionAsync(request.Email, cancellationToken).ConfigureAwait(false)) is { } result ? Ok(result) : Unauthorized();

    [HttpPost("passkeys/assertion/complete")]
    public async Task<IActionResult> CompletePasskeyAssertion(PasskeyAssertionRequest request, CancellationToken cancellationToken)
    {
        CustomerSessionIssue? result;
        try { result = await passkeys.CompleteAssertionAsync(request.ChallengeId, request.Response, cancellationToken).ConfigureAwait(false); }
        catch (UnauthorizedAccessException) { return Unauthorized("The passkey request expired or is no longer valid. Start again."); }
        catch (Fido2VerificationException) { return Unauthorized("The passkey could not be verified. Start again or use account recovery."); }
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
public sealed record PasskeyNameRequest(string DisplayName);
public sealed record PasskeyAssertionOptionsRequest(string Email);
public sealed record PasskeyAssertionRequest(Guid ChallengeId, AuthenticatorAssertionRawResponse Response);
public sealed record CodeRequest(string Code);
