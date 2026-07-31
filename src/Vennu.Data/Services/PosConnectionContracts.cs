using Vennu.Core.Models;

namespace Vennu.Data.Services;

public interface IPosCredentialProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}

public sealed record PosCredentialInput(
    string AccessToken,
    string? RefreshToken,
    DateTime? AccessTokenExpiresUtc);

public sealed record PosConnectionSummary(
    Guid Id,
    Guid VenueId,
    PosProvider Provider,
    PosConnectionStatus Status,
    string ExternalMerchantId,
    DateTime? AccessTokenExpiresUtc,
    DateTime? LastSyncedUtc,
    DateTime UpdatedUtc);

public interface IPosConnectionService
{
    Task<IReadOnlyCollection<PosConnectionSummary>> GetAllAsync(
        Guid venueId,
        CancellationToken cancellationToken = default);

    Task<PosConnectionSummary> StoreCredentialsAsync(
        Guid venueId,
        PosProvider provider,
        string externalMerchantId,
        PosCredentialInput credentials,
        CancellationToken cancellationToken = default);
}
