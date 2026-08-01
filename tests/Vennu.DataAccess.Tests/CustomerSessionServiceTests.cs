using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;
using System.Security.Cryptography;
using System.Text;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class CustomerSessionServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 23, 10, 0, TimeSpan.Zero);
    private readonly CustomerSessionPolicy policy = new()
    {
        AbsoluteLifetime = TimeSpan.FromDays(30), IdleLifetime = TimeSpan.FromDays(7), TouchInterval = TimeSpan.FromMinutes(5)
    };

    [Fact]
    public async Task IssueAsync_ReturnsOpaqueTokenButPersistsOnlyHash()
    {
        var user = ActiveUser();
        var authentication = new AuthenticationRepositoryFake();
        var service = CreateService(authentication, new IdentityRepositoryFake(user));

        var issued = await service.IssueAsync(user.Id, CustomerAuthenticationMethod.Google);

        Assert.NotEmpty(issued.Token);
        Assert.DoesNotContain(issued.Token, authentication.Session!.TokenHash, StringComparison.Ordinal);
        Assert.Equal(64, authentication.Session.TokenHash.Length);
        Assert.Equal(UtcNow.UtcDateTime.AddDays(30), authentication.Session.ExpiresUtc);
    }

    [Fact]
    public async Task AuthenticateAsync_RejectsIdleExpiredSession()
    {
        var user = ActiveUser();
        var authentication = new AuthenticationRepositoryFake
        {
            Session = Session(user.Id, UtcNow.UtcDateTime.AddDays(-8), UtcNow.UtcDateTime.AddDays(2))
        };

        var result = await CreateService(authentication, new IdentityRepositoryFake(user)).AuthenticateAsync("token");

        Assert.Null(result);
        Assert.Equal(0, authentication.TouchCount);
    }

    [Fact]
    public async Task AuthenticateAsync_TouchesActiveSessionAfterInterval()
    {
        var user = ActiveUser();
        var authentication = new AuthenticationRepositoryFake
        {
            Session = Session(user.Id, UtcNow.UtcDateTime.AddMinutes(-6), UtcNow.UtcDateTime.AddDays(2))
        };

        var result = await CreateService(authentication, new IdentityRepositoryFake(user)).AuthenticateAsync("token");

        Assert.NotNull(result);
        Assert.Equal(1, authentication.TouchCount);
        Assert.Equal(UtcNow.UtcDateTime, result.Session.LastSeenUtc);
    }

    [Fact]
    public async Task RevokeAsync_HashesRawTokenBeforeRepositoryCall()
    {
        var authentication = new AuthenticationRepositoryFake();

        Assert.True(await CreateService(authentication, new IdentityRepositoryFake(ActiveUser())).RevokeAsync("raw-token"));
        Assert.Equal(
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("raw-token"))),
            authentication.LastRevokedHash);
    }

    [Fact]
    public async Task IssueStrongAsync_RecordsStrongAssuranceAndRecentStepUp()
    {
        var authentication = new AuthenticationRepositoryFake();
        var service = CreateService(authentication, new IdentityRepositoryFake(ActiveUser()));

        var issued = await service.IssueStrongAsync(authentication.UserId, CustomerAuthenticationMethod.Passkey);

        Assert.Equal(CustomerAuthenticationAssurance.Strong, issued.Session.Assurance);
        Assert.Equal(UtcNow.UtcDateTime, issued.Session.StepUpUtc);
        Assert.True(service.IsRecent(issued.Session));
    }

    private CustomerSessionService CreateService(AuthenticationRepositoryFake auth, IdentityRepositoryFake identity) =>
        new(auth, identity, policy, new FixedTimeProvider(UtcNow));

    private static CustomerUser ActiveUser() => new()
    {
        Id = Guid.NewGuid(), Email = "user@example.com", DisplayName = "User", Status = CustomerUserStatus.Active
    };

    private static CustomerAuthSession Session(Guid userId, DateTime lastSeen, DateTime expires) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, TokenHash = new string('A', 64),
        AuthenticationMethod = CustomerAuthenticationMethod.Google,
        AuthenticatedUtc = lastSeen, CreatedUtc = lastSeen, LastSeenUtc = lastSeen, ExpiresUtc = expires
    };

    private sealed class AuthenticationRepositoryFake : ICustomerAuthenticationRepository
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public CustomerAuthSession? Session { get; set; }
        public int TouchCount { get; private set; }
        public string? LastRevokedHash { get; private set; }
        public Task<CustomerAuthSession> CreateSessionAsync(CustomerAuthSession session, CancellationToken cancellationToken = default) { Session = session; return Task.FromResult(session); }
        public Task<CustomerAuthSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) => Task.FromResult(Session);
        public Task<bool> TouchSessionAsync(Guid sessionId, DateTime lastSeenUtc, CancellationToken cancellationToken = default) { TouchCount++; return Task.FromResult(true); }
        public Task<bool> RevokeSessionAsync(string tokenHash, DateTime revokedUtc, CancellationToken cancellationToken = default) { LastRevokedHash = tokenHash; return Task.FromResult(true); }
        public Task<EmailLoginToken> CreateEmailLoginTokenAsync(EmailLoginToken token, CancellationToken cancellationToken = default) => Task.FromResult(token);
        public Task<EmailLoginToken?> ConsumeEmailLoginTokenAsync(string tokenHash, DateTime consumedUtc, CancellationToken cancellationToken = default) => Task.FromResult<EmailLoginToken?>(null);
    }

    private sealed class IdentityRepositoryFake(CustomerUser user) : ICustomerIdentityRepository
    {
        public Task<CustomerUser> CreateUserAsync(CustomerUser value, CancellationToken cancellationToken = default) => Task.FromResult(value);
        public Task<CustomerUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult<CustomerUser?>(user);
        public Task<CustomerUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<CustomerUser?>(user);
        public Task<ExternalIdentity> LinkExternalIdentityAsync(ExternalIdentity identity, CancellationToken cancellationToken = default) => Task.FromResult(identity);
        public Task<ExternalIdentity?> GetExternalIdentityAsync(ExternalIdentityProvider provider, string providerSubject, CancellationToken cancellationToken = default) => Task.FromResult<ExternalIdentity?>(null);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
