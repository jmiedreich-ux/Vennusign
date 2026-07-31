using Vennu.Api.Pos;
using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Tests.Services;

[Trait("Category", "Unit")]
public sealed class SquareOAuthConnectionServiceTests
{
    private static readonly Guid VenueId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task CompleteAsync_CorrelatesVenueAndStoresTokensThroughConnectionService()
    {
        var state = new StateFake(VenueId);
        var gateway = new GatewayFake();
        var connections = new ConnectionServiceFake();
        var service = new SquareOAuthConnectionService(
            state, gateway, connections, new RepositoryFake(), new ProtectorFake());

        await service.CompleteAsync("protected-state", "authorization-code");

        Assert.Equal("protected-state", state.Consumed);
        Assert.Equal("authorization-code", gateway.ExchangedCode);
        Assert.Equal(VenueId, connections.VenueId);
        Assert.Equal("merchant-square", connections.MerchantId);
        Assert.Equal("access-token", connections.Credentials?.AccessToken);
    }

    [Fact]
    public async Task DisconnectAsync_RevokesBeforeDeleting()
    {
        var steps = new List<string>();
        var repository = new RepositoryFake(steps)
        {
            Connection = new PosConnection
            {
                VenueId = VenueId,
                Provider = PosProvider.Square,
                ProtectedAccessToken = "protected-access"
            }
        };
        var gateway = new GatewayFake(steps);
        var service = new SquareOAuthConnectionService(
            new StateFake(VenueId), gateway, new ConnectionServiceFake(), repository, new ProtectorFake());

        Assert.True(await service.DisconnectAsync(VenueId));
        Assert.Equal(["revoke", "delete"], steps);
        Assert.Equal("access", gateway.RevokedToken);
    }

    private sealed class StateFake(Guid venueId) : IPosOAuthStateService
    {
        public string? Consumed { get; private set; }
        public string Create(Guid value) => "protected-state";
        public Guid Consume(string state) { Consumed = state; return venueId; }
    }

    private sealed class GatewayFake(List<string>? steps = null) : ISquareOAuthGateway
    {
        public string? ExchangedCode { get; private set; }
        public string? RevokedToken { get; private set; }
        public Uri CreateAuthorizationUri(string state) => new("https://connect.squareup.com/oauth2/authorize");
        public Task<SquareOAuthTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            ExchangedCode = code;
            return Task.FromResult(new SquareOAuthTokenResult("merchant-square", "access-token", "refresh-token", null));
        }
        public Task RevokeAsync(string accessToken, CancellationToken cancellationToken = default)
        {
            steps?.Add("revoke"); RevokedToken = accessToken; return Task.CompletedTask;
        }
        public Uri CreateReturnUri(string outcome) => new($"https://app.vennu.com/?pos={outcome}");
    }

    private sealed class ConnectionServiceFake : IPosConnectionService
    {
        public Guid VenueId { get; private set; }
        public string? MerchantId { get; private set; }
        public PosCredentialInput? Credentials { get; private set; }
        public Task<IReadOnlyCollection<PosConnectionSummary>> GetAllAsync(Guid venueId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<PosConnectionSummary>>([]);
        public Task<PosConnectionSummary> StoreCredentialsAsync(Guid venueId, PosProvider provider, string externalMerchantId, PosCredentialInput credentials, CancellationToken cancellationToken = default)
        {
            VenueId = venueId; MerchantId = externalMerchantId; Credentials = credentials;
            return Task.FromResult(new PosConnectionSummary(Guid.NewGuid(), venueId, provider, PosConnectionStatus.Connected, externalMerchantId, credentials.AccessTokenExpiresUtc, null, DateTime.UtcNow));
        }
    }

    private sealed class RepositoryFake(List<string>? steps = null) : IPosConnectionRepository
    {
        public PosConnection? Connection { get; init; }
        public Task<PosConnection?> GetAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) => Task.FromResult(Connection);
        public Task<IReadOnlyCollection<PosConnection>> GetAllByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyCollection<PosConnection>>([]);
        public Task<PosConnection> SaveAsync(Guid venueId, PosConnection connection, CancellationToken cancellationToken = default) => Task.FromResult(connection);
        public Task<bool> DeleteAsync(Guid venueId, PosProvider provider, CancellationToken cancellationToken = default) { steps?.Add("delete"); return Task.FromResult(true); }
    }

    private sealed class ProtectorFake : IPosCredentialProtector
    {
        public string Protect(string plaintext) => $"protected-{plaintext}";
        public string Unprotect(string protectedValue) => protectedValue["protected-".Length..];
    }
}
