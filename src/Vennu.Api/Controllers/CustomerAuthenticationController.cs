using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vennu.Api.Contracts.CustomerAuthentication;
using Vennu.Api.CustomerAuthentication;
using Vennu.Data.Services;

namespace Vennu.Api.Controllers;

[ApiController]
[Route("api/customer-auth")]
public sealed class CustomerAuthenticationController(
    ICustomerEmailLoginService emailLoginService,
    ICustomerSessionService sessionService,
    IOptions<CustomerAuthenticationOptions> options,
    ILogger<CustomerAuthenticationController> logger) : ControllerBase
{
    [HttpGet("external/{provider}")]
    public IActionResult BeginExternalSignIn(string provider, [FromQuery] string returnPath = "/", [FromQuery] string? intent = null)
    {
        if (!CustomerReturnUri.TryCreate(options.Value.FrontendOrigin, returnPath, out var returnUri))
            return BadRequest("A local return path and valid trusted frontend origin are required.");
        var (scheme, enabled) = provider.ToLowerInvariant() switch
        {
            "google" => (CustomerAuthenticationDefaults.GoogleScheme, options.Value.Google.Enabled),
            "apple" => (CustomerAuthenticationDefaults.AppleScheme, options.Value.Apple.Enabled),
            "vennusign" => (CustomerAuthenticationDefaults.EntraScheme, options.Value.Entra.Enabled),
            _ => (string.Empty, false)
        };
        if (string.IsNullOrEmpty(scheme)) return NotFound();
        if (!enabled) return Problem("The requested identity provider is not configured.", statusCode: StatusCodes.Status503ServiceUnavailable);
        var properties = new AuthenticationProperties { RedirectUri = returnUri.AbsoluteUri };
        if (intent == "signup") properties.Items["intent"] = "signup";
        return Challenge(properties, scheme);
    }

    [HttpPost("email-links")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> RequestEmailLink(
        RequestEmailLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !IsLocalReturnPath(request.ReturnPath))
            return BadRequest("A valid email and local return path are required.");
        try
        {
            await emailLoginService.RequestAsync(request.Email, request.ReturnPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Customer email-link request could not be delivered.");
        }
        return Accepted();
    }

    [HttpPost("email-links/redeem")]
    public async Task<ActionResult<CustomerSessionResponse>> RedeemEmailLink(
        RedeemEmailLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token)) return Unauthorized();
        var result = await emailLoginService.RedeemAsync(request.Token, cancellationToken).ConfigureAwait(false);
        if (result is null) return Unauthorized();
        CustomerSessionCookie.Append(Response, result.Value.Session.Token, result.Value.Session.Session.ExpiresUtc);
        return Ok(ResponseFor(result.Value.Session));
    }

    [Authorize(Policy = CustomerAuthenticationDefaults.AuthorizationPolicy)]
    [HttpGet("session")]
    public ActionResult<CustomerSessionResponse> GetSession() => Ok(new CustomerSessionResponse(
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
        User.FindFirstValue(ClaimTypes.Email)!,
        User.FindFirstValue(ClaimTypes.Name)!,
        User.FindFirstValue("auth_method")!));

    [HttpDelete("session")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeSession(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(CustomerAuthenticationDefaults.SessionCookieName, out var token) &&
            !string.IsNullOrWhiteSpace(token))
            await sessionService.RevokeAsync(token, cancellationToken).ConfigureAwait(false);
        CustomerSessionCookie.Delete(Response);
        return NoContent();
    }

    private static CustomerSessionResponse ResponseFor(CustomerSessionIssue issue) => new(
        issue.User.Id, issue.User.Email, issue.User.DisplayName, issue.Session.AuthenticationMethod.ToString());

    private static bool IsLocalReturnPath(string? value) => CustomerReturnUri.IsLocalPath(value);
}
