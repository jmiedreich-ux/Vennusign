using Vennu.Core.Models;
using Vennu.Data.Repositories;
using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed class CloverOAuthConnectionService(
    IPosOAuthStateService stateService,
    ICloverOAuthGateway gateway,
    IPosConnectionService connectionService,
    IPosConnectionRepository repository) : ICloverOAuthConnectionService
{
    public Uri Begin(Guid venueId) => gateway.CreateAuthorizationUri(stateService.Create(venueId));

    public async Task CompleteAsync(
        string state,
        string code,
        string merchantId,
        string clientId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(merchantId) || merchantId.Trim().Length > 128)
            throw new InvalidOperationException("Clover returned an invalid merchant identifier.");
        gateway.ValidateClientId(clientId);
        var venueId = stateService.Consume(state);
        var token = await gateway.ExchangeCodeAsync(code, cancellationToken).ConfigureAwait(false);
        await connectionService.StoreCredentialsAsync(
            venueId,
            PosProvider.Clover,
            merchantId.Trim(),
            new PosCredentialInput(token.AccessToken, token.RefreshToken, token.AccessTokenExpiresUtc, token.RefreshTokenExpiresUtc),
            cancellationToken).ConfigureAwait(false);
    }

    public Task<IReadOnlyCollection<PosConnectionSummary>> GetStatusAsync(
        Guid venueId,
        CancellationToken cancellationToken = default) =>
        connectionService.GetAllAsync(venueId, cancellationToken);

    public Task<bool> DisconnectAsync(Guid venueId, CancellationToken cancellationToken = default) =>
        repository.DeleteAsync(venueId, PosProvider.Clover, cancellationToken);

    public Uri ReturnUri(string outcome) => gateway.CreateReturnUri(outcome);
}
