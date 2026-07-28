using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.Data.Repositories;

public interface IFeatureMatrixRepository
{
    Task<IReadOnlyCollection<TierFeature>> GetAllTierFeaturesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<FeatureMatrixAuditEntry>> GetRecentAuditAsync(int count, CancellationToken cancellationToken = default);
    Task<int> ApplyAsync(
        IReadOnlyCollection<FeatureMatrixChange> changes,
        string adminId,
        DateTime changedUtc,
        CancellationToken cancellationToken = default);
}
