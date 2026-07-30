using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IEmergencyBroadcastRepository
{
    Task<IReadOnlyCollection<EmergencyBroadcast>> GetByVenueAsync(Guid venueId, CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(EmergencyBroadcast broadcast, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(EmergencyBroadcast broadcast, CancellationToken cancellationToken = default);
}
