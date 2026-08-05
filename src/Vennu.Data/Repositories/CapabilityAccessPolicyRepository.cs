using Vennu.Core.Models;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public interface ICapabilityAccessPolicyRepository
{
    Task<CapabilityAccessPolicy> GetAsync(
        Guid organizationId,
        Guid venueId,
        CapabilityId capability,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}

public sealed class CapabilityAccessPolicyRepository(ISqlDataAccess dataAccess) : ICapabilityAccessPolicyRepository
{
    public async Task<CapabilityAccessPolicy> GetAsync(
        Guid organizationId,
        Guid venueId,
        CapabilityId capability,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty) throw new ArgumentException("Organization ID is required.", nameof(organizationId));
        if (venueId == Guid.Empty) throw new ArgumentException("Venue ID is required.", nameof(venueId));
        var definition = Version1CapabilityRegistry.Get(capability);
        var row = (await dataAccess.ExecuteSqlQueryAsync<CapabilityAccessPolicyRow, object>(
            """
            SELECT TOP (1)
                d.CapabilityId,
                COALESCE(r.RolloutState, CASE WHEN d.Classification = 4 THEN 2 ELSE 1 END) AS RolloutState,
                CASE WHEN d.Classification IN (1, 3) OR e.Id IS NOT NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS Entitled,
                CASE WHEN d.Classification <> 2 OR a.Id IS NOT NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS AddOnAttached,
                l.LimitValue AS AllowanceLimit,
                COALESCE(u.UsedValue, 0) AS AllowanceUsed,
                r.RetryAfterUtc
            FROM dbo.CapabilityDefinitions d
            LEFT JOIN dbo.CapabilityRollouts r ON r.CapabilityId = d.CapabilityId
                AND r.StartsUtc <= @UtcNow AND (r.EndsUtc IS NULL OR r.EndsUtc > @UtcNow)
                AND (r.VenueId = @VenueId OR (r.VenueId IS NULL AND (r.OrganizationId = @OrganizationId OR r.OrganizationId IS NULL)))
            LEFT JOIN dbo.OrganizationCapabilityEntitlements e ON e.OrganizationId = @OrganizationId
                AND e.CapabilityId = d.CapabilityId AND e.StartsUtc <= @UtcNow
                AND (e.EndsUtc IS NULL OR e.EndsUtc > @UtcNow) AND e.RevokedUtc IS NULL
            LEFT JOIN dbo.CapabilityAddOnAttachments a ON a.OrganizationId = @OrganizationId
                AND a.CapabilityId = d.CapabilityId AND a.AttachedUtc <= @UtcNow AND a.DetachedUtc IS NULL
            LEFT JOIN dbo.CapabilityAllowances l ON l.OrganizationId = @OrganizationId
                AND l.CapabilityId = d.CapabilityId AND (l.VenueId IS NULL OR l.VenueId = @VenueId)
                AND l.StartsUtc <= @UtcNow AND (l.EndsUtc IS NULL OR l.EndsUtc > @UtcNow)
            LEFT JOIN dbo.CapabilityAllowanceUsage u ON u.AllowanceId = l.Id
            WHERE d.CapabilityId = @CapabilityId
            ORDER BY CASE WHEN r.VenueId = @VenueId THEN 0 WHEN r.OrganizationId = @OrganizationId THEN 1 ELSE 2 END,
                CASE WHEN l.VenueId = @VenueId THEN 0 ELSE 1 END;
            """,
            new
            {
                OrganizationId = organizationId,
                VenueId = venueId,
                CapabilityId = capability.Value,
                UtcNow = utcNow
            },
            cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        return row is null
            ? CapabilityAccessPolicy.DefaultFor(definition)
            : new CapabilityAccessPolicy(
                capability,
                Enum.IsDefined(typeof(CapabilityRolloutState), row.RolloutState)
                    ? (CapabilityRolloutState)row.RolloutState
                    : CapabilityRolloutState.Unavailable,
                row.Entitled,
                row.AddOnAttached,
                row.AllowanceLimit,
                row.AllowanceUsed,
                row.RetryAfterUtc);
    }

    private sealed class CapabilityAccessPolicyRow
    {
        public int RolloutState { get; set; }
        public bool Entitled { get; set; }
        public bool AddOnAttached { get; set; }
        public int? AllowanceLimit { get; set; }
        public int AllowanceUsed { get; set; }
        public DateTime? RetryAfterUtc { get; set; }
    }
}
