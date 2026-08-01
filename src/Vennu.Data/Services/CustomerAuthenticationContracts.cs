using Vennu.Core.Models;

namespace Vennu.Data.Services;

public sealed class CustomerSessionPolicy
{
    public TimeSpan AbsoluteLifetime { get; init; } = TimeSpan.FromDays(30);
    public TimeSpan IdleLifetime { get; init; } = TimeSpan.FromDays(7);
    public TimeSpan TouchInterval { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan EmailLinkLifetime { get; init; } = TimeSpan.FromMinutes(15);
}

public sealed record ExternalIdentityProfile(
    ExternalIdentityProvider Provider,
    string Subject,
    string Email,
    bool EmailVerified,
    string DisplayName);

public sealed record CustomerSessionIssue(string Token, CustomerAuthSession Session, CustomerUser User);

public sealed record CustomerSessionIdentity(CustomerAuthSession Session, CustomerUser User);

public sealed record EmailLoginDelivery(string Email, string Token, string ReturnPath, DateTime ExpiresUtc);

public interface ICustomerAccountService
{
    Task<CustomerUser> ResolveExternalIdentityAsync(
        ExternalIdentityProfile profile,
        CancellationToken cancellationToken = default);
}

public interface ICustomerSessionService
{
    Task<CustomerSessionIssue> IssueAsync(
        Guid userId,
        CustomerAuthenticationMethod method,
        CancellationToken cancellationToken = default);

    Task<CustomerSessionIdentity?> AuthenticateAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default);
}

public interface IEmailLoginDelivery
{
    Task SendAsync(EmailLoginDelivery delivery, CancellationToken cancellationToken = default);
}

public interface ICustomerEmailLoginService
{
    Task RequestAsync(string email, string returnPath, CancellationToken cancellationToken = default);
    Task<(CustomerSessionIssue Session, string ReturnPath)?> RedeemAsync(string token, CancellationToken cancellationToken = default);
}
