using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class CustomerOnboardingRepository(ISqlDataAccess dataAccess) : ICustomerOnboardingRepository
{
    private const string SelectSql = """
        SELECT UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, GoLiveAchievedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.CustomerOnboardingStates
        WHERE UserId = @UserId;
        """;

    // GoLiveAchievedUtc is deliberately COALESCEd against the stored value rather than
    // overwritten. A heartbeat can latch it between the moment a caller reads the state
    // and the moment it saves; without this, that save would silently un-complete
    // onboarding. The column only ever moves from NULL to a timestamp.
    private const string SaveSql = """
        MERGE dbo.CustomerOnboardingStates WITH (HOLDLOCK) AS target
        USING (SELECT @UserId AS UserId) AS source
            ON target.UserId = source.UserId
        WHEN MATCHED THEN UPDATE SET
            OrganizationId = @OrganizationId,
            SelectedTierId = @SelectedTierId,
            VenueId = @VenueId,
            FirstScreenId = @FirstScreenId,
            GoLiveAchievedUtc = COALESCE(target.GoLiveAchievedUtc, @GoLiveAchievedUtc),
            UpdatedUtc = @UpdatedUtc
        WHEN NOT MATCHED THEN INSERT
            (UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, GoLiveAchievedUtc, CreatedUtc, UpdatedUtc)
            VALUES
            (@UserId, @OrganizationId, @SelectedTierId, @VenueId, @FirstScreenId, @GoLiveAchievedUtc, @CreatedUtc, @UpdatedUtc);

        SELECT UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, GoLiveAchievedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.CustomerOnboardingStates
        WHERE UserId = @UserId;
        """;

    // Latches the achievement for whichever onboarding journey names this screen as its
    // first display. Matching on GoLiveAchievedUtc IS NULL keeps every later heartbeat a
    // no-op, so the timestamp is the first Online report and not the most recent one.
    // A screen no onboarding journey names updates nothing and returns nothing.
    private const string LatchGoLiveSql = """
        UPDATE dbo.CustomerOnboardingStates
        SET GoLiveAchievedUtc = @AchievedUtc,
            UpdatedUtc = @AchievedUtc
        WHERE FirstScreenId = @ScreenId
          AND GoLiveAchievedUtc IS NULL;

        SELECT UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, GoLiveAchievedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.CustomerOnboardingStates
        WHERE FirstScreenId = @ScreenId;
        """;

    public async Task<CustomerOnboardingState?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required.", nameof(userId));
        return (await dataAccess.ExecuteSqlQueryAsync<CustomerOnboardingState, object>(
            SelectSql,
            new { UserId = userId },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
    }

    public async Task<CustomerOnboardingState> SaveAsync(
        CustomerOnboardingState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.UserId == Guid.Empty) throw new ArgumentException("User ID is required.", nameof(state));
        var values = await dataAccess.ExecuteSqlQueryAsync<CustomerOnboardingState, CustomerOnboardingState>(
            SaveSql,
            state,
            cancellationToken).ConfigureAwait(false);
        return values.Single();
    }

    private const string SelectByFirstScreenSql = """
        SELECT UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, GoLiveAchievedUtc, CreatedUtc, UpdatedUtc
        FROM dbo.CustomerOnboardingStates
        WHERE FirstScreenId = @ScreenId;
        """;

    public async Task<CustomerOnboardingState?> GetByFirstScreenIdAsync(
        Guid screenId,
        CancellationToken cancellationToken = default)
    {
        if (screenId == Guid.Empty) throw new ArgumentException("Screen ID is required.", nameof(screenId));
        return (await dataAccess.ExecuteSqlQueryAsync<CustomerOnboardingState, object>(
            SelectByFirstScreenSql,
            new { ScreenId = screenId },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
    }

    public async Task<CustomerOnboardingState?> LatchGoLiveByFirstScreenAsync(
        Guid screenId,
        DateTime achievedUtc,
        CancellationToken cancellationToken = default)
    {
        if (screenId == Guid.Empty) throw new ArgumentException("Screen ID is required.", nameof(screenId));
        return (await dataAccess.ExecuteSqlQueryAsync<CustomerOnboardingState, object>(
            LatchGoLiveSql,
            new { ScreenId = screenId, AchievedUtc = achievedUtc },
            cancellationToken).ConfigureAwait(false)).SingleOrDefault();
    }
}
