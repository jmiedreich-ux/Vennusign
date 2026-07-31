using Vennu.Data.Services;

namespace Vennu.Api.Pos;

public sealed record SquareOAuthTokenResult(
    string MerchantId,
    string AccessToken,
    string? RefreshToken,
    DateTime? ExpiresUtc);

public interface ISquareOAuthGateway
{
    Uri CreateAuthorizationUri(string state);
    Task<SquareOAuthTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
    Task RevokeAsync(string accessToken, CancellationToken cancellationToken = default);
    Uri CreateReturnUri(string outcome);
}

public interface IPosOAuthStateService
{
    string Create(Guid venueId);
    Guid Consume(string state);
}

public interface ISquareOAuthConnectionService
{
    Uri Begin(Guid venueId);
    Task CompleteAsync(string state, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PosConnectionSummary>> GetStatusAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(Guid venueId, CancellationToken cancellationToken = default);
    Uri ReturnUri(string outcome);
}
