using System.Text.Json;
using Vennu.Core.Models;
using Vennu.Data.Services;
using Vennu.DataAccess;

namespace Vennu.Data.Repositories;

public sealed class FeatureMatrixRepository(ISqlDataAccess dataAccess) : IFeatureMatrixRepository
{
    private const string RecentAuditSql = """
        SELECT TOP (@Count) Id, TierId, FeatureId, AdminId, PreviousEnabled, NewEnabled, ChangedUtc
        FROM dbo.FeatureMatrixAudit
        ORDER BY ChangedUtc DESC, Id DESC;
        """;

    private const string ApplySql = """
        EXEC dbo.usp_ApplyFeatureMatrixChanges
            @ChangesJson = @ChangesJson,
            @AdminId = @AdminId,
            @ChangedUtc = @ChangedUtc;
        """;

    public async Task<IReadOnlyCollection<TierFeature>> GetAllTierFeaturesAsync(CancellationToken cancellationToken = default) =>
        (await dataAccess.QueryAllAsync<TierFeature>(cancellationToken).ConfigureAwait(false)).ToArray();

    public async Task<IReadOnlyCollection<FeatureMatrixAuditEntry>> GetRecentAuditAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Audit count must be between 1 and 500.");
        }

        return (await dataAccess.ExecuteSqlQueryAsync<FeatureMatrixAuditEntry, object>(
            RecentAuditSql,
            new { Count = count },
            cancellationToken).ConfigureAwait(false)).ToArray();
    }

    public async Task<int> ApplyAsync(
        IReadOnlyCollection<FeatureMatrixChange> changes,
        string adminId,
        DateTime changedUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminId);
        var json = JsonSerializer.Serialize(changes, JsonSerializerOptions.Web);
        var result = (await dataAccess.ExecuteSqlQueryAsync<ApplyResult, object>(
            ApplySql,
            new { ChangesJson = json, AdminId = adminId, ChangedUtc = changedUtc },
            cancellationToken).ConfigureAwait(false)).Single();
        return result.ChangedCount;
    }

    public sealed class ApplyResult
    {
        public int ChangedCount { get; set; }
    }
}
