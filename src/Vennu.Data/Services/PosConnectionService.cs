using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.Data.Services;

public sealed class PosConnectionService(
    IPosConnectionRepository repository,
    IPosCredentialProtector credentialProtector) : IPosConnectionService
{
    public async Task<IReadOnlyCollection<PosConnectionSummary>> GetAllAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        var connections = await repository.GetAllByVenueIdAsync(venueId, cancellationToken).ConfigureAwait(false);
        return connections.Select(ToSummary).ToArray();
    }

    public async Task<PosConnectionSummary> StoreCredentialsAsync(
        Guid venueId,
        PosProvider provider,
        string externalMerchantId,
        PosCredentialInput credentials,
        CancellationToken cancellationToken = default)
    {
        RequireId(venueId, nameof(venueId));
        if (!Enum.IsDefined(provider))
        {
            throw new ArgumentOutOfRangeException(nameof(provider));
        }

        ArgumentNullException.ThrowIfNull(credentials);
        var merchantId = RequireText(externalMerchantId, nameof(externalMerchantId), 200);
        var accessToken = RequireText(credentials.AccessToken, nameof(credentials.AccessToken));
        var protectedAccessToken = credentialProtector.Protect(accessToken);
        EnsureProtected(accessToken, protectedAccessToken);

        string? protectedRefreshToken = null;
        if (!string.IsNullOrWhiteSpace(credentials.RefreshToken))
        {
            var refreshToken = credentials.RefreshToken.Trim();
            protectedRefreshToken = credentialProtector.Protect(refreshToken);
            EnsureProtected(refreshToken, protectedRefreshToken);
        }

        var saved = await repository.SaveAsync(
            venueId,
            new PosConnection
            {
                VenueId = venueId,
                Provider = provider,
                Status = PosConnectionStatus.Connected,
                ExternalMerchantId = merchantId,
                ProtectedAccessToken = protectedAccessToken,
                ProtectedRefreshToken = protectedRefreshToken,
                AccessTokenExpiresUtc = credentials.AccessTokenExpiresUtc
            },
            cancellationToken).ConfigureAwait(false);
        return ToSummary(saved);
    }

    private static PosConnectionSummary ToSummary(PosConnection connection) =>
        new(
            connection.Id,
            connection.VenueId,
            connection.Provider,
            connection.Status,
            connection.ExternalMerchantId,
            connection.AccessTokenExpiresUtc,
            connection.LastSyncedUtc,
            connection.UpdatedUtc,
            connection.LastSyncAttemptUtc,
            connection.ConsecutiveSyncFailures,
            connection.NextSyncAttemptUtc,
            connection.LastSyncErrorCode);

    private static Guid RequireId(Guid value, string parameterName) =>
        value != Guid.Empty ? value : throw new ArgumentException("A non-empty identifier is required.", parameterName);

    private static string RequireText(string value, string parameterName, int? maximumLength = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (maximumLength.HasValue && normalized.Length > maximumLength.Value)
        {
            throw new ArgumentException($"The value cannot exceed {maximumLength.Value} characters.", parameterName);
        }

        return normalized;
    }

    private static void EnsureProtected(string plaintext, string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue) ||
            string.Equals(plaintext, protectedValue, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The credential protector did not return protected data.");
        }
    }
}
