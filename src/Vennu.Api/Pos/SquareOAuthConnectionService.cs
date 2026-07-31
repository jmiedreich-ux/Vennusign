using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class SquareOAuthConnectionService(
    IPosOAuthStateService stateService,
    ISquareOAuthGateway gateway,
    IPosConnectionService connectionService,
    IPosConnectionRepository repository,
    IPosCredentialProtector credentialProtector) : ISquareOAuthConnectionService
{
    public Uri Begin(Guid venueId) => gateway.CreateAuthorizationUri(stateService.Create(venueId));

    public async Task CompleteAsync(string state, string code, CancellationToken cancellationToken = default)
    {
        var venueId = stateService.Consume(state);
        var token = await gateway.ExchangeCodeAsync(code, cancellationToken).ConfigureAwait(false);
        await connectionService.StoreCredentialsAsync(
            venueId, PosProvider.Square, token.MerchantId,
            new PosCredentialInput(token.AccessToken, token.RefreshToken, token.ExpiresUtc),
            cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyCollection<PosConnectionSummary>> GetStatusAsync(
        Guid venueId, CancellationToken cancellationToken = default) =>
        connectionService.GetAllAsync(venueId, cancellationToken);

    public async Task<bool> DisconnectAsync(Guid venueId, CancellationToken cancellationToken = default)
    {
        var connection = await repository.GetAsync(venueId, PosProvider.Square, cancellationToken).ConfigureAwait(false);
        if (connection is null) return false;
        var accessToken = credentialProtector.Unprotect(connection.ProtectedAccessToken);
        await gateway.RevokeAsync(accessToken, cancellationToken).ConfigureAwait(false);
        return await repository.DeleteAsync(venueId, PosProvider.Square, cancellationToken).ConfigureAwait(false);
    }

    public Uri ReturnUri(string outcome) => gateway.CreateReturnUri(outcome);
}
