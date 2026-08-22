using Vennu.Core.Models;
using Vennu.Data.IntegrationTests.Fixtures;
using Vennu.Data.Repositories;

namespace Vennu.Data.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class CustomerIdentityUpsertTests(DatabaseFixture fixture)
    : InvariantCheckedTests(fixture), IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture = fixture;

    [Fact]
    public async Task VennusignSubjectRotation_ConcurrentAndRepeatedCallbacksConvergeOnOneLink()
    {
        var repository = NewRepository();
        var user = await CreateVerifiedUserAsync(repository);
        _ = await repository.UpsertExternalIdentityAsync(Identity(user.Id, "old-subject"), true);

        var callbacks = await Task.WhenAll(
            repository.UpsertExternalIdentityAsync(Identity(user.Id, "new-subject"), true),
            repository.UpsertExternalIdentityAsync(Identity(user.Id, "new-subject"), true));
        var repeated = await repository.UpsertExternalIdentityAsync(Identity(user.Id, "new-subject"), true);

        Assert.Equal(1, callbacks.Count(result => result.SubjectChanged));
        Assert.All(callbacks, result => Assert.Equal("new-subject", result.Identity.ProviderSubject));
        Assert.False(repeated.SubjectChanged);

        var linked = await repository.GetExternalIdentityAsync(
            ExternalIdentityProvider.Vennusign, "new-subject");
        Assert.NotNull(linked);
        Assert.Equal(user.Id, linked.UserId);
        Assert.Null(await repository.GetExternalIdentityAsync(
            ExternalIdentityProvider.Vennusign, "old-subject"));
    }

    [Theory]
    [InlineData(ExternalIdentityProvider.Google)]
    [InlineData(ExternalIdentityProvider.Apple)]
    public async Task ThirdPartySubjectRotation_IsRefusedAndLeavesOriginalLink(
        ExternalIdentityProvider provider)
    {
        var repository = NewRepository();
        var user = await CreateVerifiedUserAsync(repository);
        _ = await repository.UpsertExternalIdentityAsync(Identity(user.Id, "original", provider), false);

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            repository.UpsertExternalIdentityAsync(Identity(user.Id, "replacement", provider), false));

        Assert.Contains("different identity", exception.Message, StringComparison.Ordinal);
        Assert.NotNull(await repository.GetExternalIdentityAsync(provider, "original"));
        Assert.Null(await repository.GetExternalIdentityAsync(provider, "replacement"));
    }

    [Fact]
    public async Task SubjectAlreadyOwnedByAnotherUser_IsRefusedWithoutChangingEitherUser()
    {
        var repository = NewRepository();
        var first = await CreateVerifiedUserAsync(repository);
        var second = await CreateVerifiedUserAsync(repository);
        _ = await repository.UpsertExternalIdentityAsync(Identity(first.Id, "first-subject"), true);
        _ = await repository.UpsertExternalIdentityAsync(Identity(second.Id, "second-subject"), true);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            repository.UpsertExternalIdentityAsync(Identity(first.Id, "second-subject"), true));

        Assert.Equal(first.Id, (await repository.GetExternalIdentityAsync(
            ExternalIdentityProvider.Vennusign, "first-subject"))!.UserId);
        Assert.Equal(second.Id, (await repository.GetExternalIdentityAsync(
            ExternalIdentityProvider.Vennusign, "second-subject"))!.UserId);
    }

    private CustomerIdentityRepository NewRepository() =>
        new(fixture.CreateDataAccess(), TimeProvider.System);

    private async Task<CustomerUser> CreateVerifiedUserAsync(CustomerIdentityRepository repository) =>
        await repository.CreateUserAsync(new CustomerUser
        {
            Email = $"{fixture.UniqueValue("identity")}@example.com",
            DisplayName = "Identity Test",
            Status = CustomerUserStatus.Active,
            EmailVerifiedUtc = DateTime.UtcNow
        });

    private static ExternalIdentity Identity(
        Guid userId,
        string subject,
        ExternalIdentityProvider provider = ExternalIdentityProvider.Vennusign) => new()
        {
            UserId = userId,
            Provider = provider,
            ProviderSubject = subject
        };
}
