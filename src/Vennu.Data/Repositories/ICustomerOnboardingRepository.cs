using Vennu.Core.Models;

namespace Vennu.Data.Repositories;

public interface ICustomerOnboardingRepository
{
    Task<CustomerOnboardingState?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CustomerOnboardingState> SaveAsync(
        CustomerOnboardingState state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records that the journey whose first display is <paramref name="screenId"/> has reached
    /// go-live, if it has not already. Idempotent: a journey that already carries an achievement
    /// keeps its original timestamp, so the value is the first Online report rather than the last.
    /// Returns the journey that names this screen, or null when no journey does.
    /// </summary>
    Task<CustomerOnboardingState?> LatchGoLiveByFirstScreenAsync(
        Guid screenId,
        DateTime achievedUtc,
        CancellationToken cancellationToken = default);
}
