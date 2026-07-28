namespace Vennu.Data.Services;

public interface IFeatureMatrixService
{
    Task<FeatureMatrixSnapshot> GetAsync(CancellationToken cancellationToken = default);
    Task<int> ApplyAsync(
        IReadOnlyCollection<FeatureMatrixChange> changes,
        string adminId,
        CancellationToken cancellationToken = default);
}
