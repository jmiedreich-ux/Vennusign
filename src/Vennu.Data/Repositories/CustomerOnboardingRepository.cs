using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class CustomerOnboardingRepository(ISqlDataAccess dataAccess) : ICustomerOnboardingRepository
{
    private const string SelectSql = """
        SELECT UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, CreatedUtc, UpdatedUtc
        FROM dbo.CustomerOnboardingStates
        WHERE UserId = @UserId;
        """;

    private const string SaveSql = """
        MERGE dbo.CustomerOnboardingStates WITH (HOLDLOCK) AS target
        USING (SELECT @UserId AS UserId) AS source
            ON target.UserId = source.UserId
        WHEN MATCHED THEN UPDATE SET
            OrganizationId = @OrganizationId,
            SelectedTierId = @SelectedTierId,
            VenueId = @VenueId,
            FirstScreenId = @FirstScreenId,
            UpdatedUtc = @UpdatedUtc
        WHEN NOT MATCHED THEN INSERT
            (UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, CreatedUtc, UpdatedUtc)
            VALUES
            (@UserId, @OrganizationId, @SelectedTierId, @VenueId, @FirstScreenId, @CreatedUtc, @UpdatedUtc);

        SELECT UserId, OrganizationId, SelectedTierId, VenueId, FirstScreenId, CreatedUtc, UpdatedUtc
        FROM dbo.CustomerOnboardingStates
        WHERE UserId = @UserId;
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
}
