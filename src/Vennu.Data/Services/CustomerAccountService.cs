using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class CustomerAccountService(
    ICustomerIdentityRepository identityRepository,
    TimeProvider timeProvider) : ICustomerAccountService
{
    public async Task<CustomerUser> ResolveExternalIdentityAsync(
        ExternalIdentityProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (!Enum.IsDefined(profile.Provider))
            throw new ArgumentOutOfRangeException(nameof(profile.Provider));
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.Subject);

        var linked = await identityRepository.GetExternalIdentityAsync(
            profile.Provider, profile.Subject, cancellationToken).ConfigureAwait(false);
        if (linked is not null)
            return await RequireActiveUserAsync(linked.UserId, cancellationToken).ConfigureAwait(false);

        if (!profile.EmailVerified)
            throw new UnauthorizedAccessException("The identity provider did not verify the email address.");
        ArgumentException.ThrowIfNullOrWhiteSpace(profile.Email);

        var existing = await identityRepository.GetUserByEmailAsync(profile.Email, cancellationToken).ConfigureAwait(false);
        CustomerUser user;
        if (existing is null)
        {
            var utcNow = timeProvider.GetUtcNow().UtcDateTime;
            user = await identityRepository.CreateUserAsync(new CustomerUser
            {
                Email = profile.Email,
                DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Email : profile.DisplayName,
                Status = CustomerUserStatus.Active,
                EmailVerifiedUtc = utcNow
            }, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            if (existing.Status != CustomerUserStatus.Active)
                throw new UnauthorizedAccessException("The customer account is not active.");
            if (existing.EmailVerifiedUtc is null)
                throw new InvalidOperationException("A provider identity cannot be linked automatically to an unverified existing email.");
            user = existing;
        }

        await identityRepository.LinkExternalIdentityAsync(new ExternalIdentity
        {
            UserId = user.Id,
            Provider = profile.Provider,
            ProviderSubject = profile.Subject
        }, cancellationToken).ConfigureAwait(false);
        return user;
    }

    private async Task<CustomerUser> RequireActiveUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await identityRepository.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return user is { Status: CustomerUserStatus.Active }
            ? user
            : throw new UnauthorizedAccessException("The customer account is not active.");
    }
}
