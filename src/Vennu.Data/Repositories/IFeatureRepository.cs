using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface IFeatureRepository
{
    Task<IReadOnlyCollection<Feature>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Feature?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}
