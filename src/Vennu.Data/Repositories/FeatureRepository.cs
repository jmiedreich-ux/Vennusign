using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class FeatureRepository : IFeatureRepository
{
    private readonly ISqlDataAccess dataAccess;

    public FeatureRepository(ISqlDataAccess dataAccess) => this.dataAccess = dataAccess;

    public async Task<IReadOnlyCollection<Feature>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await dataAccess.QueryAllAsync<Feature>(cancellationToken).ConfigureAwait(false)).ToArray();

    public Task<Feature?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return dataAccess.QueryAsync<Feature>(new { Key = key }, cancellationToken);
    }
}
