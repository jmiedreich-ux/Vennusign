using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class CustomerAccountServiceTests
{
    private static readonly DateTimeOffset UtcNow = new(2026, 8, 1, 23, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ResolveExternalIdentityAsync_CreatesVerifiedUserAndProviderLink()
    {
        var repository = new IdentityRepositoryFake();
        var service = new CustomerAccountService(repository, new FixedTimeProvider(UtcNow));

        var user = await service.ResolveExternalIdentityAsync(new ExternalIdentityProfile(
            ExternalIdentityProvider.Google, "google-subject", "customer@example.com", true, "Customer"));

        Assert.Equal("customer@example.com", user.Email);
        Assert.Equal(UtcNow.UtcDateTime, user.EmailVerifiedUtc);
        Assert.Equal(user.Id, repository.LinkedIdentity!.UserId);
        Assert.Equal("google-subject", repository.LinkedIdentity.ProviderSubject);
    }

    [Fact]
    public async Task ResolveExternalIdentityAsync_UsesProviderSubjectBeforeEmail()
    {
        var linkedUser = ActiveUser();
        var repository = new IdentityRepositoryFake
        {
            LinkedLookup = new ExternalIdentity { UserId = linkedUser.Id },
            ExistingUser = linkedUser
        };

        var user = await new CustomerAccountService(repository, new FixedTimeProvider(UtcNow))
            .ResolveExternalIdentityAsync(new ExternalIdentityProfile(
                ExternalIdentityProvider.Apple, "apple-subject", "changed@example.com", false, "Changed"));

        Assert.Same(linkedUser, user);
        Assert.Null(repository.LinkedIdentity);
    }

    [Fact]
    public async Task ResolveExternalIdentityAsync_RejectsUnverifiedProviderEmail()
    {
        var service = new CustomerAccountService(new IdentityRepositoryFake(), new FixedTimeProvider(UtcNow));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ResolveExternalIdentityAsync(
            new ExternalIdentityProfile(ExternalIdentityProvider.Google, "subject", "user@example.com", false, "User")));
    }

    [Fact]
    public async Task ResolveExternalIdentityAsync_RejectsAutomaticLinkToUnverifiedExistingEmail()
    {
        var repository = new IdentityRepositoryFake
        {
            ExistingUser = new CustomerUser { Id = Guid.NewGuid(), Email = "user@example.com", Status = CustomerUserStatus.Active }
        };
        var service = new CustomerAccountService(repository, new FixedTimeProvider(UtcNow));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResolveExternalIdentityAsync(
            new ExternalIdentityProfile(ExternalIdentityProvider.Apple, "subject", "user@example.com", true, "User")));
    }

    [Fact]
    public async Task ResolveExternalIdentityAsync_AllowsVennusignSubjectChangeForVerifiedExistingEmail()
    {
        var repository = new IdentityRepositoryFake { ExistingUser = ActiveUser() };
        var service = new CustomerAccountService(repository, new FixedTimeProvider(UtcNow));

        var user = await service.ResolveExternalIdentityAsync(new ExternalIdentityProfile(
            ExternalIdentityProvider.Vennusign, "replacement-subject", "user@example.com", true, "User"));

        Assert.Same(repository.ExistingUser, user);
        Assert.True(repository.AllowSubjectChange);
        Assert.Equal("replacement-subject", repository.LinkedIdentity!.ProviderSubject);
    }

    [Theory]
    [InlineData(ExternalIdentityProvider.Google)]
    [InlineData(ExternalIdentityProvider.Apple)]
    public async Task ResolveExternalIdentityAsync_PreservesThirdPartySubjectChangeBoundary(
        ExternalIdentityProvider provider)
    {
        var repository = new IdentityRepositoryFake { ExistingUser = ActiveUser() };
        var service = new CustomerAccountService(repository, new FixedTimeProvider(UtcNow));

        _ = await service.ResolveExternalIdentityAsync(new ExternalIdentityProfile(
            provider, "replacement-subject", "user@example.com", true, "User"));

        Assert.False(repository.AllowSubjectChange);
    }

    private static CustomerUser ActiveUser() => new()
    {
        Id = Guid.NewGuid(), Email = "user@example.com", DisplayName = "User",
        Status = CustomerUserStatus.Active, EmailVerifiedUtc = UtcNow.UtcDateTime
    };

    private sealed class IdentityRepositoryFake : ICustomerIdentityRepository
    {
        public ExternalIdentity? LinkedLookup { get; set; }
        public CustomerUser? ExistingUser { get; set; }
        public ExternalIdentity? LinkedIdentity { get; private set; }
        public bool? AllowSubjectChange { get; private set; }

        public Task<CustomerUser> CreateUserAsync(CustomerUser user, CancellationToken cancellationToken = default)
        {
            user.Id = user.Id == Guid.Empty ? Guid.NewGuid() : user.Id;
            ExistingUser = user;
            return Task.FromResult(user);
        }
        public Task<CustomerUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(ExistingUser);
        public Task<CustomerUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(ExistingUser);
        public Task<ExternalIdentityLinkResult> UpsertExternalIdentityAsync(
            ExternalIdentity identity,
            bool allowSubjectChange,
            CancellationToken cancellationToken = default)
        {
            LinkedIdentity = identity;
            AllowSubjectChange = allowSubjectChange;
            return Task.FromResult(new ExternalIdentityLinkResult(identity, false));
        }
        public Task<ExternalIdentity?> GetExternalIdentityAsync(ExternalIdentityProvider provider, string providerSubject, CancellationToken cancellationToken = default) => Task.FromResult(LinkedLookup);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
