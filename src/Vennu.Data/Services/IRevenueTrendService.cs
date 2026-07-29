namespace Vennu.Data.Services;

public interface IRevenueTrendService
{
    Task CaptureAsync(
        RevenueSnapshot snapshot,
        CancellationToken cancellationToken = default);

    Task<RevenueTrend> GetAsync(
        int months,
        CancellationToken cancellationToken = default);
}
