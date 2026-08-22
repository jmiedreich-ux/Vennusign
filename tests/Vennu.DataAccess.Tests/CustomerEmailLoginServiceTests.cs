using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class CustomerEmailLoginServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 23, 20, 0, TimeSpan.Zero);

    [Fact]
    public async Task RequestAsync_UnknownEmail_DoesNotCreateOrDeliverToken()
    {
        var auth = new AuthenticationRepositoryFake();
        var delivery = new DeliveryFake();
        var service = CreateService(auth, new IdentityRepositoryFake(null), delivery);

        await service.RequestAsync("missing@example.com", "/welcome");

        Assert.Null(auth.EmailToken);
        Assert.Null(delivery.Delivery);
    }

    [Fact]
    public async Task RequestAsync_VerifiedEmail_PersistsHashAndDeliversRawSingleUseToken()
    {
        var user = new CustomerUser
        {
            Id = Guid.NewGuid(), Email = "user@example.com", Status = CustomerUserStatus.Active,
            EmailVerifiedUtc = UtcNow.UtcDateTime
        };
        var auth = new AuthenticationRepositoryFake();
        var delivery = new DeliveryFake();
        var service = CreateService(auth, new IdentityRepositoryFake(user), delivery);

        await service.RequestAsync(user.Email, "/welcome");

        Assert.NotNull(auth.EmailToken);
        Assert.NotNull(delivery.Delivery);
        Assert.Equal(64, auth.EmailToken.TokenHash.Length);
        Assert.NotEqual(delivery.Delivery.Token, auth.EmailToken.TokenHash);
        Assert.Equal("/welcome", delivery.Delivery.ReturnPath);
        Assert.Equal(UtcNow.UtcDateTime.AddMinutes(15), auth.EmailToken.ExpiresUtc);
    }

    [Fact]
    public async Task RequestAsync_RejectsProtocolRelativeReturnPath()
    {
        var service = CreateService(new AuthenticationRepositoryFake(), new IdentityRepositoryFake(null), new DeliveryFake());

        await Assert.ThrowsAsync<ArgumentException>(() => service.RequestAsync("user@example.com", "//attacker.example"));
    }

    private static CustomerEmailLoginService CreateService(
        AuthenticationRepositoryFake authentication,
        IdentityRepositoryFake identity,
        DeliveryFake delivery) =>
        new(authentication, identity, new SessionServiceFake(), delivery,
            new CustomerSessionPolicy { EmailLinkLifetime = TimeSpan.FromMinutes(15) },
            new FixedTimeProvider(UtcNow));

    private sealed class AuthenticationRepositoryFake : ICustomerAuthenticationRepository
    {
        public EmailLoginToken? EmailToken { get; private set; }
        public Task<CustomerAuthSession> CreateSessionAsync(CustomerAuthSession session, CancellationToken cancellationToken = default) => Task.FromResult(session);
        public Task<CustomerAuthSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult<CustomerAuthSession?>(null);
        public Task<bool> TouchSessionAsync(Guid sessionId, DateTime lastSeenUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> RevokeSessionAsync(string tokenHash, DateTime revokedUtc, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<EmailLoginToken> CreateEmailLoginTokenAsync(EmailLoginToken token, CancellationToken cancellationToken = default) { EmailToken = token; return Task.FromResult(token); }
        public Task<EmailLoginToken?> ConsumeEmailLoginTokenAsync(string tokenHash, DateTime consumedUtc, CancellationToken cancellationToken = default) => Task.FromResult<EmailLoginToken?>(null);
    }

    private sealed class IdentityRepositoryFake(CustomerUser? user) : ICustomerIdentityRepository
    {
        public Task<CustomerUser> CreateUserAsync(CustomerUser value, CancellationToken cancellationToken = default) => Task.FromResult(value);
        public Task<CustomerUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(user);
        public Task<CustomerUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(user);
        public Task<ExternalIdentityLinkResult> UpsertExternalIdentityAsync(ExternalIdentity identity, bool allowSubjectChange, CancellationToken cancellationToken = default) => Task.FromResult(new ExternalIdentityLinkResult(identity, false));
        public Task<ExternalIdentity?> GetExternalIdentityAsync(ExternalIdentityProvider provider, string providerSubject, CancellationToken cancellationToken = default) => Task.FromResult<ExternalIdentity?>(null);
    }

    private sealed class SessionServiceFake : ICustomerSessionService
    {
        public Task<CustomerSessionIssue> IssueAsync(Guid userId, CustomerAuthenticationMethod method, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<CustomerSessionIdentity?> AuthenticateAsync(string token, CancellationToken cancellationToken = default) => Task.FromResult<CustomerSessionIdentity?>(null);
        public Task<bool> RevokeAsync(string token, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class DeliveryFake : IEmailLoginDelivery
    {
        public EmailLoginDelivery? Delivery { get; private set; }
        public Task SendAsync(EmailLoginDelivery delivery, CancellationToken cancellationToken = default) { Delivery = delivery; return Task.CompletedTask; }
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
