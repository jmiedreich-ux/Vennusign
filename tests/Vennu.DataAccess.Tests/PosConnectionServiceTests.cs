using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

[Trait("Category", "Unit")]
public sealed class PosConnectionServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task StoreCredentialsAsync_ProtectsTokensBeforePersistence()
    {
        var repository = new RepositoryFake();
        var service = new PosConnectionService(repository, new PrefixProtector());

        var result = await service.StoreCredentialsAsync(
            VenueId,
            PosProvider.Square,
            " merchant-1 ",
            new PosCredentialInput(" access-secret ", " refresh-secret ", new DateTime(2026, 8, 1)));

        var saved = Assert.Single(repository.Saved);
        Assert.Equal("protected:access-secret", saved.ProtectedAccessToken);
        Assert.Equal("protected:refresh-secret", saved.ProtectedRefreshToken);
        Assert.Equal("merchant-1", saved.ExternalMerchantId);
        Assert.Equal(PosConnectionStatus.Connected, saved.Status);
        Assert.Equal(saved.Id, result.Id);
    }

    [Fact]
    public async Task StoreCredentialsAsync_RejectsProtectorThatReturnsPlaintext()
    {
        var repository = new RepositoryFake();
        var service = new PosConnectionService(repository, new PassThroughProtector());

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StoreCredentialsAsync(
            VenueId,
            PosProvider.Square,
            "merchant-1",
            new PosCredentialInput("access-secret", null, null)));

        Assert.Empty(repository.Saved);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCredentialFreeSummaries()
    {
        var repository = new RepositoryFake
        {
            Connections =
            [
                new PosConnection
                {
                    Id = Guid.NewGuid(),
                    VenueId = VenueId,
                    Provider = PosProvider.Toast,
                    Status = PosConnectionStatus.Connected,
                    ExternalMerchantId = "merchant-2",
                    ProtectedAccessToken = "protected:secret"
                }
            ]
        };
        var service = new PosConnectionService(repository, new PrefixProtector());

        var result = Assert.Single(await service.GetAllAsync(VenueId));

        Assert.Equal("merchant-2", result.ExternalMerchantId);
        Assert.DoesNotContain(
            typeof(PosConnectionSummary).GetProperties(),
            property => property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Credential", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Protected", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task StoreCredentialsAsync_RejectsUnsupportedProvider()
    {
        var repository = new RepositoryFake();
        var service = new PosConnectionService(repository, new PrefixProtector());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.StoreCredentialsAsync(
            VenueId,
            (PosProvider)99,
            "merchant-1",
            new PosCredentialInput("access-secret", null, null)));

        Assert.Empty(repository.Saved);
    }

    private sealed class PrefixProtector : IPosCredentialProtector
    {
        public string Protect(string plaintext) => $"protected:{plaintext}";
        public string Unprotect(string protectedValue) => protectedValue["protected:".Length..];
    }

    private sealed class PassThroughProtector : IPosCredentialProtector
    {
        public string Protect(string plaintext) => plaintext;
        public string Unprotect(string protectedValue) => protectedValue;
    }

    private sealed class RepositoryFake : IPosConnectionRepository
    {
        public IReadOnlyCollection<PosConnection> Connections { get; init; } = [];
        public List<PosConnection> Saved { get; } = [];

        public Task<PosConnection?> GetAsync(
            Guid venueId,
            PosProvider provider,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Connections.SingleOrDefault(value => value.VenueId == venueId && value.Provider == provider));

        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<PosConnection>>(
                Connections.Where(value => value.VenueId == venueId).ToArray());

        public Task<PosConnection> SaveAsync(
            Guid venueId,
            PosConnection connection,
            CancellationToken cancellationToken = default)
        {
            connection.Id = connection.Id == Guid.Empty ? Guid.NewGuid() : connection.Id;
            Saved.Add(connection);
            return Task.FromResult(connection);
        }
    }
}
