#nullable enable
using RepoDb;
using Vennu.Data.Models;
using Vennu.Data.Repositories;
using Vennu.DataAccess.Infrastructure;

namespace Vennu.DataAccess.Repositories;

public class ScreenPairingCodeRepository : IScreenPairingCodeRepository
{
    private readonly ISqlConnectionFactory connectionFactory;

    public ScreenPairingCodeRepository(ISqlConnectionFactory connectionFactory) => this.connectionFactory = connectionFactory;

    public async Task<string> CreateAsync(ScreenPairingCode pairingCode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pairingCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(pairingCode.Code);

        pairingCode.CreatedUtc = pairingCode.CreatedUtc == default ? DateTime.UtcNow : pairingCode.CreatedUtc;

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await connection.InsertAsync(pairingCode);
        return pairingCode.Code;
    }

    public async Task<ScreenPairingCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var pairingCodes = await connection.QueryAsync<ScreenPairingCode>(new { Code = code });
        return pairingCodes.FirstOrDefault();
    }

    public async Task<bool> ClaimAsync(string code, Guid venueId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var pairingCodes = await connection.QueryAsync<ScreenPairingCode>(new { Code = code });
        var pairingCode = pairingCodes.FirstOrDefault();

        if (pairingCode is null || pairingCode.IsClaimed || pairingCode.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        pairingCode.VenueId = venueId;
        pairingCode.IsClaimed = true;
        pairingCode.ClaimedAt = DateTime.UtcNow;
        return await connection.UpdateAsync(pairingCode) > 0;
    }
}
