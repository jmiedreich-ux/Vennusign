namespace Vennu.Data.Services;

public interface IOperationalDashboardService
{
    Task<OperationalDashboard> GetAsync(CancellationToken cancellationToken = default);
}
