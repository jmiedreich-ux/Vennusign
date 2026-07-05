using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IScreenPairingCodeRepository
{
    Task<string> CreateAsync(ScreenPairingCode pairingCode, CancellationToken cancellationToken = default);
    Task<ScreenPairingCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ClaimAsync(string code, Guid venueId, CancellationToken cancellationToken = default);
}