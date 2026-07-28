namespace Vennu.Data.Services;

public interface IRevenueSnapshotService
{
    Task<RevenueSnapshot> GetAsync(CancellationToken cancellationToken = default);
}
