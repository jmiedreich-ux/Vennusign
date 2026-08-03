using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class PlatformOperationsOnboardingRepository(ISqlDataAccess dataAccess) : IPlatformOperationsOnboardingRepository
{
    private const string SearchSql = """
        SELECT TOP (200)
            S.UserId,
            U.DisplayName AS CustomerName,
            U.Email AS CustomerEmail,
            S.OrganizationId,
            O.Name AS OrganizationName,
            S.VenueId,
            V.Name AS VenueName,
            COALESCE(OS.TierId, S.SelectedTierId) AS TierId,
            T.Name AS TierName,
            COALESCE(OS.Status, 'not-selected') AS SubscriptionStatus,
            OS.TrialEndsAt,
            S.FirstScreenId,
            SC.Name AS FirstScreenName,
            CASE
                WHEN S.FirstScreenId IS NULL THEN 'not-paired'
                WHEN LOWER(COALESCE(SC.Status, 'offline')) = 'online' THEN 'online'
                ELSE 'paired-offline'
            END AS FirstScreenStatus,
            SC.LastSeen AS FirstScreenLastSeenUtc,
            S.UpdatedUtc AS LastActivityUtc
        FROM dbo.CustomerOnboardingStates S
        INNER JOIN dbo.CustomerUsers U ON U.Id = S.UserId
        LEFT JOIN dbo.Organizations O ON O.Id = S.OrganizationId
        LEFT JOIN dbo.OrganizationSubscriptions OS ON OS.OrganizationId = S.OrganizationId
        LEFT JOIN dbo.SubscriptionTiers T ON T.Id = COALESCE(OS.TierId, S.SelectedTierId)
        LEFT JOIN dbo.Venues V ON V.Id = S.VenueId
        LEFT JOIN dbo.Screens SC ON SC.Id = S.FirstScreenId
        WHERE @Search IS NULL
           OR U.DisplayName LIKE '%' + @Search + '%'
           OR U.Email LIKE '%' + @Search + '%'
           OR O.Name LIKE '%' + @Search + '%'
           OR V.Name LIKE '%' + @Search + '%'
        ORDER BY S.UpdatedUtc DESC, S.UserId;
        """;

    public async Task<IReadOnlyCollection<PlatformOperationsOnboardingRecord>> SearchAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        var normalized = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        if (normalized?.Length > 100) throw new ArgumentException("Search cannot exceed 100 characters.", nameof(search));

        return (await dataAccess.ExecuteSqlQueryAsync<PlatformOperationsOnboardingRecord, object>(
            SearchSql,
            new { Search = normalized },
            cancellationToken).ConfigureAwait(false)).ToArray();
    }
}
