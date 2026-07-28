using Vennu.Core.Models;
using Vennu.Data.Repositories;

namespace Vennu.DataAccess.Tests;

public class ProcessedStripeEventRepositoryTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public async Task Try_claim_uses_atomic_locked_merge_with_stale_lease()
    {
        var capturedSql = string.Empty;
        var dataAccess = new FakeSqlDataAccess
        {
            ExecuteSqlQueryHandler = (sql, _) =>
            {
                capturedSql = sql;
                return new object[]
                {
                    new ProcessedStripeEvent
                    {
                        EventId = "evt_1",
                        EventType = "invoice.paid",
                        Status = "processing"
                    }
                };
            }
        };
        var repository = new ProcessedStripeEventRepository(dataAccess);

        var result = await repository.TryClaimAsync(
            "evt_1",
            "invoice.paid",
            new DateTime(2026, 7, 28, 15, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 28, 15, 25, 0, DateTimeKind.Utc));

        Assert.NotNull(result);
        Assert.Contains("MERGE dbo.ProcessedStripeEvents WITH (HOLDLOCK)", capturedSql, StringComparison.Ordinal);
        Assert.Contains("target.StartedUtc <= @StaleBeforeUtc", capturedSql, StringComparison.Ordinal);
        Assert.Contains("target.Status = 'failed'", capturedSql, StringComparison.Ordinal);
    }
}
