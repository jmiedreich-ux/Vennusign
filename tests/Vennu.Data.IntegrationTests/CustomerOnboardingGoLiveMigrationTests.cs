using Microsoft.Data.SqlClient;

namespace Vennu.Data.IntegrationTests;

/// <summary>
/// Migration 073 against a real database.
///
/// The column is the easy half. The half worth executing is the backfill: existing
/// customers who already reached go-live must not be asked to onboard again, and
/// the only evidence the schema carries that they ever did is Screens.LastSeen -
/// NULL until a heartbeat arrives, and only ever written by one.
/// </summary>
[Trait("Category", "Integration")]
public class CustomerOnboardingGoLiveMigrationTests
{
    private const string LocalDb =
        @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";

    [Fact]
    public async Task Migration073_AddsTheColumnAndBackfillsOnlyScreensThatHaveReportedIn()
    {
        var database = $"vennusign_golive_{Guid.NewGuid():N}"[..40];
        var master = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = "master" }.ConnectionString;
        var target = new SqlConnectionStringBuilder(LocalDb) { InitialCatalog = database }.ConnectionString;

        try
        {
            await ExecuteAsync(master, $"CREATE DATABASE [{database}];");
            DatabaseMigrator.Run(target);

            Assert.Equal(1, await ScalarAsync(target,
                "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CustomerOnboardingStates') AND name = 'GoLiveAchievedUtc';"));

            // Three journeys that differ only in what their first display has done:
            // one that has reported in, one that never has, and one with no display.
            var everLive = await SeedJourneyAsync(target, "ever-live", lastSeenUtc: new DateTime(2026, 8, 1, 9, 30, 0, DateTimeKind.Utc));
            var neverLive = await SeedJourneyAsync(target, "never-live", lastSeenUtc: null);
            var unpaired = await SeedJourneyAsync(target, "unpaired", lastSeenUtc: null, pairDisplay: false);

            // Re-run only the migration's backfill. The rows above did not exist when
            // the migration ran, so this is the statement doing the work it would have
            // done for a customer who was already live when it was deployed.
            await ExecuteAsync(target, BackfillSql);

            Assert.Equal(new DateTime(2026, 8, 1, 9, 30, 0), await DateAsync(target, everLive));
            Assert.Null(await DateAsync(target, neverLive));
            Assert.Null(await DateAsync(target, unpaired));

            // Idempotent, and never moves an achievement that is already recorded.
            await ExecuteAsync(target, $"UPDATE dbo.Screens SET LastSeen = '2026-08-15T12:00:00' WHERE Id = (SELECT FirstScreenId FROM dbo.CustomerOnboardingStates WHERE UserId = '{everLive}');");
            await ExecuteAsync(target, BackfillSql);
            Assert.Equal(new DateTime(2026, 8, 1, 9, 30, 0), await DateAsync(target, everLive));
        }
        finally
        {
            await ExecuteAsync(master,
                $"IF DB_ID('{database}') IS NOT NULL BEGIN ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}]; END;");
        }
    }

    private const string BackfillSql = """
        UPDATE onboarding
        SET GoLiveAchievedUtc = screen.LastSeen
        FROM dbo.CustomerOnboardingStates AS onboarding
        INNER JOIN dbo.Screens AS screen ON screen.Id = onboarding.FirstScreenId
        WHERE onboarding.GoLiveAchievedUtc IS NULL
          AND onboarding.FirstScreenId IS NOT NULL
          AND screen.LastSeen IS NOT NULL;
        """;

    private static async Task<Guid> SeedJourneyAsync(string connectionString, string label, DateTime? lastSeenUtc, bool pairDisplay = true)
    {
        var userId = Guid.NewGuid();
        var screenId = Guid.NewGuid();
        var email = $"{label}-{userId:N}@example.com";
        var lastSeen = lastSeenUtc is null ? "NULL" : $"'{lastSeenUtc:yyyy-MM-ddTHH:mm:ss}'";

        await ExecuteAsync(connectionString, $"""
            INSERT INTO dbo.CustomerUsers (Id, Email, NormalizedEmail, DisplayName, Status)
            VALUES ('{userId}', '{email}', UPPER('{email}'), 'QA {label}', 1);

            INSERT INTO dbo.Screens (Id, ScreenKey, Name, LastSeen, Status)
            VALUES ('{screenId}', '{label[..Math.Min(label.Length, 6)]}{Random.Shared.Next(100, 999)}', 'QA {label}', {lastSeen}, 'Offline');

            INSERT INTO dbo.CustomerOnboardingStates (UserId, FirstScreenId)
            VALUES ('{userId}', {(pairDisplay ? $"'{screenId}'" : "NULL")});
            """);
        return userId;
    }

    private static async Task<DateTime?> DateAsync(string connectionString, Guid userId)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            $"SELECT GoLiveAchievedUtc FROM dbo.CustomerOnboardingStates WHERE UserId = '{userId}';", connection);
        var value = await command.ExecuteScalarAsync();
        return value is DBNull or null ? null : (DateTime)value;
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        _ = await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }
}
