using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IOperationalEventRepository
{
    Task AddAsync(OperationalEvent operationalEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OperationalEvent>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}
