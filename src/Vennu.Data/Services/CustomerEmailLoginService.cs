using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class CustomerEmailLoginService(
    ICustomerAuthenticationRepository authenticationRepository,
    ICustomerIdentityRepository identityRepository,
    ICustomerSessionService sessionService,
    IEmailLoginDelivery delivery,
    CustomerSessionPolicy policy,
    TimeProvider timeProvider) : ICustomerEmailLoginService
{
    public async Task RequestAsync(
        string email,
        string returnPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ValidateReturnPath(returnPath);
        var user = await identityRepository.GetUserByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is not { Status: CustomerUserStatus.Active, EmailVerifiedUtc: not null })
            return;

        var tokenValue = CustomerSessionService.CreateToken();
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var token = await authenticationRepository.CreateEmailLoginTokenAsync(new EmailLoginToken
        {
            Id = Guid.NewGuid(), UserId = user.Id, TokenHash = CustomerSessionService.Hash(tokenValue),
            ReturnPath = returnPath, CreatedUtc = utcNow, ExpiresUtc = utcNow.Add(policy.EmailLinkLifetime)
        }, cancellationToken).ConfigureAwait(false);
        await delivery.SendAsync(
            new EmailLoginDelivery(user.Email, tokenValue, token.ReturnPath, token.ExpiresUtc),
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<(CustomerSessionIssue Session, string ReturnPath)?> RedeemAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        var consumed = await authenticationRepository.ConsumeEmailLoginTokenAsync(
            CustomerSessionService.Hash(token), timeProvider.GetUtcNow().UtcDateTime, cancellationToken).ConfigureAwait(false);
        if (consumed is null) return null;
        var session = await sessionService.IssueAsync(
            consumed.UserId, CustomerAuthenticationMethod.EmailLink, cancellationToken).ConfigureAwait(false);
        return (session, consumed.ReturnPath);
    }

    private static void ValidateReturnPath(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!value.StartsWith('/') || value.StartsWith("//", StringComparison.Ordinal) || value.Length > 500)
            throw new ArgumentException("A bounded local return path is required.", nameof(value));
    }
}
