using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed record CloverOAuthTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresUtc,
    DateTime RefreshTokenExpiresUtc);

public interface ICloverOAuthGateway
{
    Uri CreateAuthorizationUri(string state);
    Task<CloverOAuthTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
    void ValidateClientId(string clientId);
    Uri CreateReturnUri(string outcome);
}

public interface ICloverOAuthConnectionService
{
    Uri Begin(Guid venueId);
    Task CompleteAsync(
        string state,
        string code,
        string merchantId,
        string clientId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PosConnectionSummary>> GetStatusAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(Guid venueId, CancellationToken cancellationToken = default);
    Uri ReturnUri(string outcome);
}
