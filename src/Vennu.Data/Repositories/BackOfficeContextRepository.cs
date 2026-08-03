using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class BackOfficeContextRepository(ISqlDataAccess dataAccess) : IBackOfficeContextRepository
{
    public async Task<IReadOnlyCollection<BackOfficeContextRecord>> GetAuthorizedAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A customer user ID is required.", nameof(userId));

        var contexts = await dataAccess.ExecuteSqlQueryAsync<BackOfficeContextRecord, object>(
            """
            SELECT o.Id OrganizationId, o.Name OrganizationName, v.Id VenueId, v.Name VenueName
            FROM dbo.OrganizationMemberships om
            INNER JOIN dbo.Organizations o ON o.Id = om.OrganizationId
            INNER JOIN dbo.Venues v ON v.OrganizationId = o.Id
            LEFT JOIN dbo.VenueMemberships vm
                ON vm.OrganizationId = o.Id AND vm.VenueId = v.Id AND vm.UserId = om.UserId
            WHERE om.UserId = @UserId
              AND om.RevokedUtc IS NULL
              AND (om.Role IN (1, 2) OR (vm.RevokedUtc IS NULL AND vm.Role IN (1, 2)))
            ORDER BY o.Name, v.Name, v.Id;
            """,
            new { UserId = userId },
            cancellationToken).ConfigureAwait(false);
        return contexts.ToArray();
    }
}
