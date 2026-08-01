using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class CloverOAuthConnectionServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task CompleteAsync_ConsumesStateValidatesClientAndStoresMerchantScopedCredentials()
    {
        var state = new StateFake(VenueId);
        var gateway = new GatewayFake();
        var connections = new ConnectionServiceFake();
        var service = new CloverOAuthConnectionService(state, gateway, connections, new RepositoryFake());

        await service.CompleteAsync("protected-state", "authorization-code", "merchant-clover", "clover-client");

        Assert.Equal("protected-state", state.Consumed);
        Assert.Equal("clover-client", gateway.ValidatedClientId);
        Assert.Equal("authorization-code", gateway.ExchangedCode);
        Assert.Equal(VenueId, connections.VenueId);
        Assert.Equal(PosProvider.Clover, connections.Provider);
        Assert.Equal("merchant-clover", connections.MerchantId);
        Assert.Equal("access-secret", connections.Credentials?.AccessToken);
        Assert.Equal("refresh-secret", connections.Credentials?.RefreshToken);
        Assert.NotNull(connections.Credentials?.RefreshTokenExpiresUtc);
    }

    [Fact]
    public async Task CompleteAsync_RejectsInvalidMerchantBeforeStateConsumption()
    {
        var state = new StateFake(VenueId);
        var service = new CloverOAuthConnectionService(state, new GatewayFake(), new ConnectionServiceFake(), new RepositoryFake());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CompleteAsync("protected-state", "authorization-code", " ", "clover-client"));

        Assert.Null(state.Consumed);
    }

    private sealed class StateFake(Guid venueId) : IPosOAuthStateService
    {
        public string? Consumed { get; private set; }
        public string Create(Guid value) => "protected-state";
        public Guid Consume(string state) { Consumed = state; return venueId; }
    }

    private sealed class GatewayFake : ICloverOAuthGateway
    {
        public string? ExchangedCode { get; private set; }
        public string? ValidatedClientId { get; private set; }
        public Uri CreateAuthorizationUri(string state) => new("https://www.clover.com/oauth/v2/authorize");
        public Task<CloverOAuthTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            ExchangedCode = code;
            return Task.FromResult(new CloverOAuthTokenResult(
                "access-secret",
                "refresh-secret",
                DateTime.UtcNow.AddHours(1),
                DateTime.UtcNow.AddDays(30)));
        }
        public void ValidateClientId(string clientId) => ValidatedClientId = clientId;
        public Uri CreateReturnUri(string outcome) => new($"https://app.vennu.test/integrations?pos={outcome}");
    }

    private sealed class ConnectionServiceFake : IPosConnectionService
    {
        public Guid VenueId { get; private set; }
        public PosProvider Provider { get; private set; }
        public string? MerchantId { get; private set; }
        public PosCredentialInput? Credentials { get; private set; }
        public Task<IReadOnlyCollection<PosConnectionSummary>> GetAllAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<PosConnectionSummary>>([]);
        public Task<PosConnectionSummary> StoreCredentialsAsync(Guid venueId, PosProvider provider, string externalMerchantId, PosCredentialInput credentials, CancellationToken cancellationToken = default)
        {
            VenueId = venueId; Provider = provider; MerchantId = externalMerchantId; Credentials = credentials;
            return Task.FromResult(new PosConnectionSummary(Guid.NewGuid(), venueId, provider, PosConnectionStatus.Connected, externalMerchantId, credentials.AccessTokenExpiresUtc, null, DateTime.UtcNow));
        }
    }

    private sealed class RepositoryFake : IPosConnectionRepository
    {
        public Task<PosConnection?> GetAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult<PosConnection?>(null);
        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PosConnection>>([]);
        public Task<PosConnection> SaveAsync(Guid venueId, PosConnection connection, CancellationToken cancellationToken = default) => Task.FromResult(connection);
        public Task<bool> DeleteAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
