using Vennu.DataAccess;
using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public class ScreenPairingCodeRepository : IScreenPairingCodeRepository
{
    private readonly ISqlDataAccess dataAccess;

    public ScreenPairingCodeRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<string> CreateAsync(ScreenPairingCode pairingCode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pairingCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(pairingCode.Code);

        pairingCode.CreatedUtc = pairingCode.CreatedUtc == default ? DateTime.UtcNow : pairingCode.CreatedUtc;
        await dataAccess.InsertAsync(pairingCode, cancellationToken).ConfigureAwait(false);
        return pairingCode.Code;
    }

    public Task<ScreenPairingCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return dataAccess.QueryAsync<ScreenPairingCode>(new { Code = code }, cancellationToken);
    }

    public async Task<bool> ClaimAsync(string code, Guid venueId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var pairingCode = await dataAccess.QueryAsync<ScreenPairingCode>(new { Code = code }, cancellationToken).ConfigureAwait(false);

        if (pairingCode is null || pairingCode.IsClaimed || pairingCode.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        pairingCode.VenueId = venueId;
        pairingCode.IsClaimed = true;
        pairingCode.ClaimedAt = DateTime.UtcNow;
        return await dataAccess.UpdateAsync(pairingCode, cancellationToken).ConfigureAwait(false) > 0;
    }
}